//=======================================================================================
// Transient Fault Handling Framework for SQL Azure, Storage, Service Bus & Cache
//
// This sample is supplemental to the technical guidance published on the Windows Azure
// Customer Advisory Team blog at http://windowsazurecat.com/. 
//
//=======================================================================================
// Copyright © 2011 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
namespace Microsoft.AzureCAT.Samples.TransientFaultHandling.SqlAzure
{
    #region Using references
    using System;
    using System.Net;
    using System.Text;
    using System.Data.SqlClient;
    using System.Data;
    using System.Xml;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// Provides a reliable way of opening connections to and executing commands against the SQL Azure 
    /// databases taking into account potential network unreliability and a requirement for connection retry.
    /// </summary>
    public sealed class ReliableSqlConnection : IDbConnection, IDisposable, ICloneable
    {
        #region Private members
        private static readonly ConcurrentDictionary<string, string> connectionStringCache = new ConcurrentDictionary<string, string>();
        private static readonly string[] protocolMonikers = new string[] { "tcp:", "np:", "via:", "lpc:" };
        private static readonly Regex portNumberRegex = new Regex(",\\s*\\d+$", RegexOptions.Compiled | RegexOptions.Singleline); 

        private readonly SqlConnection underlyingConnection;
        private readonly RetryPolicy connectionRetryPolicy;
        private readonly RetryPolicy commandRetryPolicy;
        private readonly RetryPolicy connectionStringFailoverPolicy;

        private string connectionString;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the SqlAzureConnection class with a given connection string. Uses the default
        /// retry policy for connections and commands unless retry settings are provided in the connection string.
        /// </summary>
        /// <param name="connectionString">The connection string used to open the SQL Azure database.</param>
        public ReliableSqlConnection(string connectionString)
            : this(connectionString, RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicy())
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlAzureConnection class with a given connection string
        /// and a policy defining whether to retry a request if the connection fails to be opened or a command
        /// fails to be successfully executed.
        /// </summary>
        /// <param name="connectionString">The connection string used to open the SQL Azure database.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a request if a connection or a command fails.</param>
        public ReliableSqlConnection(string connectionString, RetryPolicy retryPolicy)
            : this(connectionString, retryPolicy, ChooseBetween(RetryPolicyFactory.GetDefaultSqlCommandRetryPolicy(), retryPolicy))
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlAzureConnection class with a given connection string
        /// and a policy defining whether to retry a request if the connection fails to be opened or a command
        /// fails to be successfully executed.
        /// </summary>
        /// <param name="connectionString">The connection string used to open the SQL Azure database.</param>
        /// <param name="connectionRetryPolicy">The retry policy defining whether to retry a request if a connection fails to be established.</param>
        /// <param name="commandRetryPolicy">The retry policy defining whether to retry a request if a command fails to be executed.</param>
        public ReliableSqlConnection(string connectionString, RetryPolicy connectionRetryPolicy, RetryPolicy commandRetryPolicy)
        {
            this.connectionString = connectionString;
            this.underlyingConnection = new SqlConnection(GetDnsSafeConnectionString(connectionString, true));
            this.connectionRetryPolicy = connectionRetryPolicy;
            this.commandRetryPolicy = commandRetryPolicy;

            // Configure a special retry policy that enables detecting network connectivity errors to be able to determine whether we need to failover
            // to the original connection string containing the DNS name of the SQL Azure server.
            this.connectionStringFailoverPolicy = new RetryPolicy<NetworkConnectivityErrorDetectionStrategy>(1, TimeSpan.FromMilliseconds(1)) { FastFirstRetry = true };
            this.connectionStringFailoverPolicy.RetryOccurred += (currentRetryCount, lastException, delay) => 
            {
                // Step 1 - remove the cached connection string since we can no longer rely on the IP address specified in the cached version.
                string removedValue;
                connectionStringCache.TryRemove(connectionString, out removedValue);

                // Reset the connection string in the underlying connection object to match the original connection string specified upon initialization.
                ConnectionString = connectionString;
            };
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the connection string for opening a connection to the SQL Azure database.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return this.connectionString; 
            }
            set 
            { 
                this.connectionString = value;
                this.underlyingConnection.ConnectionString = GetDnsSafeConnectionString(value, true); 
            }
        }

        /// <summary>
        /// Specified the policy which decides whether to retry a connection request, based on how many 
        /// times the request has been made and the reason for the last failure. 
        /// </summary>
        public RetryPolicy ConnectionRetryPolicy
        {
            get { return this.connectionRetryPolicy; }
        }

        /// <summary>
        /// Specified the policy which decides whether to retry a command, based on how many 
        /// times the request has been made and the reason for the last failure. 
        /// </summary>
        public RetryPolicy CommandRetryPolicy
        {
            get { return this.commandRetryPolicy; }
        }

        /// <summary>
        /// An instance of the SqlConnection object that represents the connection to an instance of SQL Azure database.
        /// </summary>
        public SqlConnection Current
        {
            get { return this.underlyingConnection; }
        }

        /// <summary>
        /// Returns the CONTEXT_INFO value that was set for the current session. This value can be used for tracing the query execution problems. 
        /// </summary>
        public Guid SessionTracingId
        {
            get
            {
                try
                {
                    using (IDbCommand query = SqlCommandFactory.CreateGetContextInfoCommand(Current))
                    {
                        // Execute the query in retry-aware fashion, retry if necessary.
                        return ExecuteCommand<Guid>(query);
                    }
                }
                catch
                {
                    // Any failure will result in returning a default GUID value. This is by design.
                    return Guid.Empty;
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString and ConnectionRetryPolicy properties.
        /// </summary>
        /// <returns>An object representing the open connection.</returns>
        public SqlConnection Open()
        {
            return Open(ConnectionRetryPolicy);
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString and specified retry policy.
        /// </summary>
        /// <param name="retryPolicy">The retry policy defining whether to retry a request if the connection fails to be opened.</param>
        /// <returns>An object representing the open connection.</returns>
        public SqlConnection Open(RetryPolicy retryPolicy)
        {
            // Check if retry policy was specified, if not, disable retries by executing the Open method using RetryPolicy.NoRetry.
            (retryPolicy != null ? retryPolicy : RetryPolicy.NoRetry).ExecuteAction(() =>
            {
                this.connectionStringFailoverPolicy.ExecuteAction(() =>
                {
                    if (this.underlyingConnection.State != ConnectionState.Open)
                    {
                        this.underlyingConnection.Open();
                    }
                });
            });

            return this.underlyingConnection;
        }

        /// <summary>
        /// Executes a SQL command and returns a result defined by the specified type <typeparamref name="T"/>. This method uses the retry policy specified when 
        /// instantiating the SqlAzureConnection class (or the default retry policy if no policy was set at construction time).
        /// </summary>
        /// <typeparam name="T">Either IDataReader, XmlReader or any .NET type defining the type of result to be returned.</typeparam>
        /// <param name="command">A SqlCommand object containing the SQL command to be executed.</param>
        /// <returns>An instance of an IDataReader, XmlReader or any .NET object containing the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command)
        {
            return ExecuteCommand<T>(command, CommandRetryPolicy, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a SQL command and returns a result defined by the specified type <typeparamref name="T"/>. This method uses the retry policy specified when 
        /// instantiating the SqlAzureConnection class (or the default retry policy if no policy was set at construction time).
        /// </summary>
        /// <typeparam name="T">Either IDataReader, XmlReader or any .NET type defining the type of result to be returned.</typeparam>
        /// <param name="command">A SqlCommand object containing the SQL command to be executed.</param>
        /// <param name="behavior">Provides a description of the results of the query and its effect on the database.</param>
        /// <returns>An instance of an IDataReader, XmlReader or any .NET object containing the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command, CommandBehavior behavior)
        {
            return ExecuteCommand<T>(command, CommandRetryPolicy, behavior);
        }

        /// <summary>
        /// Executes a SQL command using the specified retry policy and returns a result defined by the specified type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Either IDataReader, XmlReader or any .NET type defining the type of result to be returned.</typeparam>
        /// <param name="command">A SqlCommand object containing the SQL command to be executed.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a command if a connection fails while executing the command.</param>
        /// <returns>An instance of an IDataReader, XmlReader or any .NET object containing the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command, RetryPolicy retryPolicy)
        {
            return ExecuteCommand<T>(command, retryPolicy, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a SQL command using the specified retry policy and returns a result defined by the specified type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Either IDataReader, XmlReader or any .NET type defining the type of result to be returned.</typeparam>
        /// <param name="command">A SqlCommand object containing the SQL command to be executed.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a command if a connection fails while executing the command.</param>
        /// <param name="behavior">Provides a description of the results of the query and its effect on the database.</param>
        /// <returns>An instance of an IDataReader, XmlReader or any .NET object containing the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command, RetryPolicy retryPolicy, CommandBehavior behavior)
        {
            return (retryPolicy != null ? retryPolicy : RetryPolicy.NoRetry).ExecuteAction<T>(() =>
            {
                return this.connectionStringFailoverPolicy.ExecuteAction<T>(() =>
                {
                    // Make sure the command has been associated with a valid connection. If not, associate it with an opened SQL connection.
                    if (command.Connection == null)
                    {
                        // Open a new connection and assign it to the command object.
                        command.Connection = Open();
                    }

                    // Verify whether or not the connection is valid and is open. This code may be retried therefore
                    // it is important to ensure that a connection is re-established should it have previously failed.
                    if (command.Connection.State != ConnectionState.Open)
                    {
                        command.Connection.Open();
                    }

                    Type resultType = typeof(T);

                    if (resultType.IsAssignableFrom(typeof(IDataReader)))
                    {
                        return (T)command.ExecuteReader(behavior);
                    }
                    else if (resultType.IsAssignableFrom(typeof(XmlReader)))
                    {
                        if (command is SqlCommand)
                        {
                            object result = null;
                            XmlReader xmlReader = (command as SqlCommand).ExecuteXmlReader();

                            if ((behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
                            {
                                // Implicit conversion from XmlReader to <T> via an intermediary object.
                                result = new SqlXmlReader(command.Connection, xmlReader);
                            }
                            else
                            {
                                // Implicit conversion from XmlReader to <T> via an intermediary object.
                                result = xmlReader;
                            }

                            return (T)result;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    else if (resultType == typeof(NonQueryResult))
                    {
                        NonQueryResult result = new NonQueryResult();
                        result.RecordsAffected = command.ExecuteNonQuery();

                        return (T)Convert.ChangeType(result, resultType);
                    }
                    else
                    {
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            return (T)Convert.ChangeType(result, resultType);
                        }
                        else
                        {
                            return default(T);
                        }   
                    }
                });
            });
        }

        /// <summary>
        /// Executes a SQL command and returns the number of rows affected.
        /// </summary>
        /// <param name="command">A SqlCommand object containing the SQL command to be executed.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteCommand(IDbCommand command)
        {
            return ExecuteCommand(command, CommandRetryPolicy);
        }

        /// <summary>
        /// Executes a SQL command and returns the number of rows affected.
        /// </summary>
        /// <param name="command">A SqlCommand object containing the SQL command to be executed.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a command if a connection fails while executing the command.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteCommand(IDbCommand command, RetryPolicy retryPolicy)
        {
            NonQueryResult result = ExecuteCommand<NonQueryResult>(command, retryPolicy);

            return result.RecordsAffected;
        }

        /// <summary>
        /// Modifies the specified connection string to potentially improve reliability of a connection to SQL Azure database. If asynchronous mode is
        /// requested, modifications are performed in background and may not be reflected immediately.
        /// </summary>
        /// <param name="connectionString">The connection string that will be parsed and modified if required.</param>
        /// <param name="async">True if action needs to be asynchronous, otherwise false. When asynchronous action is requested, modifications are performed in the background and may not be reflected immediately.</param>
        /// <returns>The modified connection string some parameters of which are modified to improve reliability of the connection to a SQL Azure database.</returns>
        public static string GetDnsSafeConnectionString(string connectionString, bool async = false)
        {
            if (!String.IsNullOrEmpty(connectionString))
            {
                string safeConnectionString = connectionString;

                // Check if we already keep this connection string in the cache of safe connection strings. If so, we should use the cached copy unless it comes back as null.
                if (!connectionStringCache.TryGetValue(connectionString, out safeConnectionString))
                {
                    // The worker task is responsible for replacing the server name with its resolved IP address.
                    Action workerTask = () =>
                    {
                        // First step - add a "placeholder" into the connection string cache. This will ensure that while we are performing DNS name resolution,
                        // no other threads that are using the ReliableSqlConnection class would attempt to do the same.
                        if (connectionStringCache.TryAdd(connectionString, null))
                        {
                            // Disassemble the connection string using SqlConnectionStringBuilder.
                            var conStringBuilder = new SqlConnectionStringBuilder(connectionString);

                            string protocolMoniker = null, hostName = conStringBuilder.DataSource;
                            IPAddress hostAddress = null;

                            // Should not be dealing with an empty data source.
                            if (!String.IsNullOrEmpty(hostName))
                            {
                                // If data source is already represented by an IP address, we should skip the next section altogether.
                                if (!IPAddress.TryParse(hostName, out hostAddress))
                                {
                                    // Check if the server name starts with a protocol moniker. We should remove it prior to performing IP address resolution.
                                    foreach (var moniker in protocolMonikers)
                                    {
                                        if (hostName.StartsWith(moniker))
                                        {
                                            protocolMoniker = moniker;
                                            hostName = hostName.Remove(0, moniker.Length);
                                        }
                                    }

                                    // Check if the server name contains a port number. If so, get rid of it.
                                    if (portNumberRegex.IsMatch(hostName))
                                    {
                                        hostName = portNumberRegex.Replace(hostName, String.Empty);
                                    }

                                    // Check if the server name refers to a SQL instance name (also applies to SQL Express instances). If so, don't attempt to resolve the IP address.
                                    if (!hostName.Contains(@"\"))
                                    {
                                        // Resolve the IP address by host name (DB server name specified in the connection string).
                                        var hostAddresses = Dns.GetHostAddresses(hostName);

                                        // Make sure we have at least 1 resolved IP address.
                                        if (hostAddresses != null && hostAddresses.Length > 0)
                                        {
                                            // Replace DB server name with its resolved IP address.
                                            conStringBuilder.DataSource = String.Concat(protocolMoniker, hostAddresses[0].ToString());
                                        }
                                    }
                                }
                            }

                            // Put the modified connection string into the cache so that it can be reused for subsequent requests.
                            connectionStringCache.TryUpdate(connectionString, conStringBuilder.ConnectionString, null);
                        }
                    };

                    // The fault handler task is responsible for removing the cached connection string.
                    Action<Task> faultHandlerTask = (t) =>
                    {
                        try
                        {
                            if (t != null)
                            {
                                // This assignment is to prevent an error which can manifest itself if exceptions are not observed.
                                // A Task’s exception(s) were not observed either by Waiting on the Task or accessing its Exception property. 
                                // As a result, the unobserved exception was re-thrown by the finalizer thread.
                                var e = t.Exception;

                                // Wait for task to complete. Should happen instantly.
                                t.Wait();
                            }
                        }
                        catch
                        {
                            // Must not re-throw.
                        }
                        finally
                        {
                            string removedValue;
                            connectionStringCache.TryRemove(connectionString, out removedValue);
                        }
                    };

                    if (async)
                    {
                        Task.Factory.StartNew(workerTask, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).ContinueWith(faultHandlerTask, TaskContinuationOptions.OnlyOnFaulted);
                    }
                    else
                    {
                        try
                        {
                            workerTask();
                        }
                        catch
                        {
                            faultHandlerTask(null);
                        }
                    }
                }

                return safeConnectionString ?? connectionString;
            }
            else
            {
                return connectionString;
            }
        }

        /// <summary>
        /// Clears the connection string cache. 
        /// </summary>
        public static void ClearConnectionStringCache()
        {
            connectionStringCache.Clear();
        }
        #endregion

        #region IDbConnection implementation
        /// <summary>
        /// Begins a database transaction with the specified System.Data.IsolationLevel value.
        /// </summary>
        /// <param name="level">One of the System.Data.IsolationLevel values.</param>
        /// <returns>An object representing the new transaction.</returns>
        public IDbTransaction BeginTransaction(IsolationLevel level)
        {
            return this.underlyingConnection.BeginTransaction(level);
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>An object representing the new transaction.</returns>
        public IDbTransaction BeginTransaction()
        {
            return this.underlyingConnection.BeginTransaction();
        }

        /// <summary>
        /// Changes the current database for an open Connection object.
        /// </summary>
        /// <param name="databaseName">The name of the database to use in place of the current database.</param>
        public void ChangeDatabase(string databaseName)
        {
            this.underlyingConnection.ChangeDatabase(databaseName);
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString
        /// property of the provider-specific Connection object.
        /// </summary>
        void IDbConnection.Open()
        {
            this.Open();
        }

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        public void Close()
        {
            this.underlyingConnection.Close();
        }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating
        /// the attempt and generating an error.
        /// </summary>
        public int ConnectionTimeout
        {
            get { return this.underlyingConnection.ConnectionTimeout; }
        }

        /// <summary>
        /// Creates and returns a SqlCommand object associated with the underlying SqlConnection.
        /// </summary>
        /// <returns>A System.Data.SqlClient.SqlCommand object.</returns>
        public SqlCommand CreateCommand()
        {
            return this.underlyingConnection.CreateCommand();
        }

        /// <summary>
        /// Creates and returns an object implementing the IDbCommand interface which is associated 
        /// with the underlying SqlConnection.
        /// </summary>
        /// <returns>A System.Data.SqlClient.SqlCommand object.</returns>
        IDbCommand IDbConnection.CreateCommand()
        {
            return this.underlyingConnection.CreateCommand();
        }

        /// <summary>
        /// Gets the name of the current database or the database to be used after a
        /// connection is opened.
        /// </summary>
        public string Database
        {
            get { return this.underlyingConnection.Database; }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        public ConnectionState State
        {
            get { return this.underlyingConnection.State; }
        }
        #endregion

        #region ICloneable implementation
        /// <summary>
        /// Creates a new connection that is a copy of the current instance including the connection
        /// string, connection retry policy and command retry policy.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        object ICloneable.Clone()
        {
            return new ReliableSqlConnection(this.ConnectionString, this.ConnectionRetryPolicy, this.CommandRetryPolicy);
        }
        #endregion

        #region IDisposable implementation
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">A flag indicating that managed resources must be released.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.underlyingConnection.State == ConnectionState.Open)
                {
                    this.underlyingConnection.Close();
                }

                this.underlyingConnection.Dispose();
            }
        }
        #endregion

        #region Private helper classes
        /// <summary>
        /// This helpers class is intended to be used exclusively for fetching the number of affected records when executing a command using ExecuteNonQuery.
        /// </summary>
        private sealed class NonQueryResult
        {
            public int RecordsAffected { get; set; }
        }

        /// <summary>
        /// Allows to choose the retry policy from a selection of the main and default one depending on whether or not the main policy is specified.
        /// </summary>
        /// <param name="mainPolicy">The main policy to be chosen if not null.</param>
        /// <param name="defaultPolicy">The default policy to be chosen as an alternative.</param>
        /// <returns>The chosen policy object.</returns>
        private static RetryPolicy ChooseBetween(RetryPolicy mainPolicy, RetryPolicy defaultPolicy)
        {
            return mainPolicy != null ? mainPolicy : defaultPolicy;
        }
        #endregion

        /// <summary>
        /// Implements a strategy that detects network connectivity errors such as host not found.
        /// </summary>
        private sealed class NetworkConnectivityErrorDetectionStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                SqlException sqlException = null;

                if (ex != null && (sqlException = ex as SqlException) != null)
                {
                    switch (sqlException.Number)
                    {
                        // SQL Error Code: 11001
                        // A network-related or instance-specific error occurred while establishing a connection to SQL Server. 
                        // The server was not found or was not accessible. Verify that the instance name is correct and that SQL 
                        // Server is configured to allow remote connections. (provider: TCP Provider, error: 0 - No such host is known.)
                        case 11001:
                            return true;
                    }
                }

                return false;
            }
        }
    }
}
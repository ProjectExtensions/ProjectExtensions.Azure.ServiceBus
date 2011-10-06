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
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    using Microsoft.AzureCAT.Samples.TransientFaultHandling.Properties;
    #endregion

    /// <summary>
    /// Provides factory methods for instantiating SQL commands.
    /// </summary>
    public static class SqlCommandFactory
    {
        #region Public members
        /// <summary>
        /// Returns the default timeout which will be applied to all SQL commands constructed by this factory class.
        /// </summary>
        public const int DefaultCommandTimeoutSeconds = 60;
        #endregion

        #region Generic SQL commands
        /// <summary>
        /// Creates a generic command of type Stored Procedure and assigns the default command timeout.
        /// </summary>
        /// <param name="connection">The database connection object to be associated with the new command.</param>
        /// <returns>A new SQL command initialized with the respective command type and initial settings.</returns>
        public static IDbCommand CreateCommand(IDbConnection connection)
        {
            Guard.ArgumentNotNull(connection, "connection");

            IDbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = DefaultCommandTimeoutSeconds;

            return command;
        }

        /// <summary>
        /// Creates a generic command of type Stored Procedure and assigns the specified command text and default command timeout.
        /// </summary>
        /// <param name="connection">The database connection object to be associated with the new command.</param>
        /// <param name="commandText">The text of the command to run against the data source.</param>
        /// <returns>A new SQL command initialized with the respective command type, text and initial settings.</returns>
        public static IDbCommand CreateCommand(IDbConnection connection, string commandText)
        {
            Guard.ArgumentNotNullOrEmptyString(commandText, "commandText");

            IDbCommand command = CreateCommand(connection);
            command.CommandText = commandText;

            return command;
        }
        #endregion

        #region Other system commands
        /// <summary>
        /// Creates a SQL command that is intended to return the connection's context ID which is useful for tracing purposes.
        /// </summary>
        /// <param name="connection">The database connection object to be associated with the new command.</param>
        /// <returns>A new SQL command initialized with the respective command type, text and initial settings.</returns>
        public static IDbCommand CreateGetContextInfoCommand(IDbConnection connection)
        {
            Guard.ArgumentNotNull(connection, "connection");

            IDbCommand command = CreateCommand(connection);

            command.CommandType = CommandType.Text;
            command.CommandText = SqlCommandResources.QueryGetContextInfo;

            return command;
        } 
        #endregion
    }
}

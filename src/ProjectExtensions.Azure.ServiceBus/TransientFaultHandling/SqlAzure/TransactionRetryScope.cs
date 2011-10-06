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
    using System.Transactions;
    #endregion

    /// <summary>
    /// Provides support for retry policy-aware transactional scope.
    /// </summary>
    public sealed class TransactionRetryScope : IDisposable
    {
        #region Private members
        private readonly RetryPolicy retryPolicy;
        private readonly TransactionScopeInitializer transactionScopeInit;
        private readonly Action unitOfWork;
        private TransactionScope transactionScope;

        private delegate TransactionScope TransactionScopeInitializer();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class. 
        /// Implements no retry policy, it just invokes the unit of work exactly once.
        /// </summary>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(Action unitOfWork)
            : this(TransactionScopeOption.Required, unitOfWork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified retry policy.
        /// </summary>
        /// <param name="retryPolicy">The retry policy defining whether to retry the execution of the entire scope should a transient fault be encountered.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(RetryPolicy retryPolicy, Action unitOfWork)
            : this(TransactionScopeOption.Required, retryPolicy, unitOfWork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified requirements.
        /// Implements no retry policy, it just invokes the unit of work exactly once.
        /// </summary>
        /// <param name="scopeOption">An instance of the <see cref="System.Transactions.TransactionScopeOption"/> enumeration that describes the transaction requirements associated with this transaction scope.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(TransactionScopeOption scopeOption, Action unitOfWork)
            : this(scopeOption, RetryPolicy.NoRetry, unitOfWork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified timeout value and requirements.
        /// Implements no retry policy, it just invokes the unit of work exactly once.
        /// </summary>
        /// <param name="scopeOption">An instance of the <see cref="System.Transactions.TransactionScopeOption"/> enumeration that describes the transaction requirements associated with this transaction scope.</param>
        /// <param name="scopeTimeout">The TimeSpan after which the transaction scope times out and aborts the transaction.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout, Action unitOfWork)
            : this(scopeOption, scopeTimeout, RetryPolicy.NoRetry, unitOfWork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified timeout value, transaction scope options and retry policy.
        /// Uses the ReadCommitted isolation level by default.
        /// </summary>
        /// <param name="scopeOption">An instance of the <see cref="System.Transactions.TransactionScopeOption"/> enumeration that describes the transaction requirements associated with this transaction scope.</param>
        /// <param name="scopeTimeout">The TimeSpan after which the transaction scope times out and aborts the transaction.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry the execution of the entire scope should a transient fault be encountered.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout, RetryPolicy retryPolicy, Action unitOfWork)
        {
            this.transactionScopeInit = () =>
            {
                TransactionOptions txOptions = new TransactionOptions();

                txOptions.IsolationLevel = IsolationLevel.ReadCommitted;
                txOptions.Timeout = scopeTimeout;

                return new TransactionScope(scopeOption, txOptions);
            };

            this.transactionScope = this.transactionScopeInit();
            this.retryPolicy = retryPolicy;
            this.unitOfWork = unitOfWork;

            // Set up the callback method for the specified retry policy.
            InitializeRetryPolicy();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified requirements.
        /// Implements no retry policy, it just invokes the unit of work exactly once.
        /// </summary>
        /// <param name="scopeOption">An instance of the <see cref="System.Transactions.TransactionScopeOption"/> enumeration that describes the transaction requirements associated with this transaction scope.</param>
        /// <param name="transactionOptions">A <see cref="System.Transactions.TransactionOptions"/> structure that describes the transaction options to use if a new transaction is created. If an existing transaction is used, the timeout value in this parameter applies to the transaction scope. If that time expires before the scope is disposed, the transaction is aborted.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions, Action unitOfWork)
            : this(scopeOption, transactionOptions, RetryPolicy.NoRetry, unitOfWork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified requirements and specified retry policy.
        /// </summary>
        /// <param name="scopeOption">An instance of the <see cref="System.Transactions.TransactionScopeOption"/> enumeration that describes the transaction requirements associated with this transaction scope.</param>
        /// <param name="txOptions">A <see cref="System.Transactions.TransactionOptions"/> structure that describes the transaction options to use if a new transaction is created. If an existing transaction is used, the timeout value in this parameter applies to the transaction scope. If that time expires before the scope is disposed, the transaction is aborted.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry the execution of the entire scope should a transient fault be encountered.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(TransactionScopeOption scopeOption, TransactionOptions txOptions, RetryPolicy retryPolicy, Action unitOfWork)
        {
            this.transactionScopeInit = () =>
            {
                return new TransactionScope(scopeOption, txOptions);
            };

            this.transactionScope = this.transactionScopeInit();
            this.retryPolicy = retryPolicy;
            this.unitOfWork = unitOfWork;

            // Set up the callback method for the specified retry policy.
            InitializeRetryPolicy();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified requirements and retry policy.
        /// Uses the ReadCommitted isolation level by default.
        /// </summary>
        /// <param name="scopeOption">An instance of the <see cref="System.Transactions.TransactionScopeOption"/> enumeration that describes the transaction requirements associated with this transaction scope.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry the execution of the entire scope should a transient fault be encountered.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(TransactionScopeOption scopeOption, RetryPolicy retryPolicy, Action unitOfWork)
            : this(scopeOption, TimeSpan.Zero, retryPolicy, unitOfWork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class and sets the specified transaction as the ambient transaction, 
        /// so that transactional work done inside the scope uses this transaction. Implements no retry policy, it just invokes the unit of work exactly once.
        /// </summary>
        /// <param name="tx">The transaction to be set as the ambient transaction, so that transactional work done inside the scope uses this transaction.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(Transaction tx, Action unitOfWork)
            : this(tx, RetryPolicy.NoRetry, unitOfWork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified retry policy and sets the specified transaction as the ambient transaction, 
        /// so that transactional work done inside the scope uses this transaction.
        /// </summary>
        /// <param name="tx">The transaction to be set as the ambient transaction, so that transactional work done inside the scope uses this transaction.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry the execution of the entire scope should a transient fault be encountered.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(Transaction tx, RetryPolicy retryPolicy, Action unitOfWork)
        {
            this.transactionScopeInit = () =>
            {
                return new TransactionScope(tx);
            };

            this.transactionScope = this.transactionScopeInit();
            this.retryPolicy = retryPolicy;
            this.unitOfWork = unitOfWork;

            // Set up the callback method for the specified retry policy.
            InitializeRetryPolicy();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified timeout value, and sets the specified transaction as the ambient transaction, 
        /// so that transactional work done inside the scope uses this transaction. Implements no retry policy, it just invokes the unit of work exactly once.
        /// </summary>
        /// <param name="tx">The transaction to be set as the ambient transaction, so that transactional work done inside the scope uses this transaction.</param>
        /// <param name="scopeTimeout">The TimeSpan after which the transaction scope times out and aborts the transaction.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(Transaction tx, TimeSpan scopeTimeout, Action unitOfWork)
            : this(tx, scopeTimeout, RetryPolicy.NoRetry, unitOfWork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRetryScope"/> class with the specified timeout value, and sets the specified transaction as the ambient transaction, 
        /// so that transactional work done inside the scope uses this transaction. Uses the with the specified retry policy.
        /// </summary>
        /// <param name="tx">The transaction to be set as the ambient transaction, so that transactional work done inside the scope uses this transaction.</param>
        /// <param name="scopeTimeout">The TimeSpan after which the transaction scope times out and aborts the transaction.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry the execution of the entire scope should a transient fault be encountered.</param>
        /// <param name="unitOfWork">A delegate representing the executable unit of work which will be retried if fails.</param>
        public TransactionRetryScope(Transaction tx, TimeSpan scopeTimeout, RetryPolicy retryPolicy, Action unitOfWork)
        {
            this.transactionScopeInit = () =>
            {
                return new TransactionScope(tx, scopeTimeout);
            };

            this.transactionScope = this.transactionScopeInit();
            this.retryPolicy = retryPolicy;
            this.unitOfWork = unitOfWork;

            // Set up the callback method for the specified retry policy.
            InitializeRetryPolicy();
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Returns the policy which defines whether to retry the execution of the entire scope should a transient fault be encountered.
        /// </summary>
        public RetryPolicy RetryPolicy
        {
            get { return this.retryPolicy; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Executes the underlying unit of work and retries as prescribed by the current retry policy.
        /// </summary>
        public void InvokeUnitOfWork()
        {
            this.retryPolicy.ExecuteAction(this.unitOfWork);
        }

        /// <summary>
        /// Indicates that all operations within the scope are completed successfully.
        /// </summary>
        public void Complete()
        {
            // Invoke the main method to indicate that all operations within the scope are completed successfully.
            if (this.transactionScope != null)
            {
                this.transactionScope.Complete();
            }
        }
        #endregion

        #region IDisposable implementation
        /// <summary>
        /// Ends the transaction scope.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (this.transactionScope != null)
            {
                this.transactionScope.Dispose();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Configures the specified retry policy to work with a transactional scope.
        /// </summary>
        private void InitializeRetryPolicy()
        {
            this.retryPolicy.RetryOccurred += (currentRetryCount, lastException, delay) =>
            {
                try
                {
                    // Should recycle the scope upon failure. This will also rollback the entire transaction.
                    if (this.transactionScope != null)
                    {
                        this.transactionScope.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    // Something went wrong when disposing the transactional scope, we should interrupt the retry cycle.
                    throw new RetryLimitExceededException(ex);
                }

                // Get a new instance of a transactional scope.
                this.transactionScope = this.transactionScopeInit();
            };
        }

        /// <summary>
        /// Executes the specified unit of work as prescribed by the current retry policy.
        /// </summary>
        private void InvokeUnitOfWork(Action action)
        {
            bool success = false;

            try
            {
                this.retryPolicy.ExecuteAction(action);
                success = true;

                // The unit of work was successful. We should now complete the scope. This will intentionally happen outside the retry scope.
                Complete();
            }
            finally
            {
                if (!success)
                {
                    // This simulates a failover action as if we utilized the Using block. Since we invoke the unit of work from within the class constructor,
                    // it's actual Dispose method will never be called in the event of a failure. Therefore, we should manually rollback any transactions and clean up.
                    this.transactionScope.Dispose();
                    this.transactionScope = null;
                }
            }
        }
        #endregion
    }
}

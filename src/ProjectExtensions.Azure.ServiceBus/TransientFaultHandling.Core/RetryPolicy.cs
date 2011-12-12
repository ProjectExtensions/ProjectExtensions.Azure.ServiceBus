//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Transient Fault Handling Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

namespace Microsoft.Practices.TransientFaultHandling
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides the base implementation of the retry mechanism for unreliable actions and transient conditions.
    /// </summary>
    public class RetryPolicy
    {
        /// <summary>
        /// Returns a default policy that does no retries, it just invokes action exactly once.
        /// </summary>
        public static readonly RetryPolicy NoRetry = new RetryPolicy<TransientErrorIgnoreStrategy>(0);

        /// <summary>
        /// Returns a default policy that implements a fixed retry interval configured with the default <see cref="FixedInterval"/> retry strategy.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryPolicy DefaultFixed = new RetryPolicy<TransientErrorCatchAllStrategy>(new FixedInterval());

        /// <summary>
        /// Returns a default policy that implements a progressive retry interval configured with the default <see cref="Incremental"/> retry strategy.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryPolicy DefaultProgressive = new RetryPolicy<TransientErrorCatchAllStrategy>(new Incremental());

        /// <summary>
        /// Returns a default policy that implements a random exponential retry interval configured with the default <see cref="FixedInterval"/> retry strategy.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryPolicy DefaultExponential = new RetryPolicy<TransientErrorCatchAllStrategy>(new ExponentialBackoff());

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and parameters defining the progressive delay between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy"/> that is responsible for detecting transient conditions.</param>
        /// <param name="retryStrategy">The retry strategy to use for this retry policy.</param>
        public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, RetryStrategy retryStrategy)
        {
            Guard.ArgumentNotNull(errorDetectionStrategy, "errorDetectionStrategy");
            Guard.ArgumentNotNull(retryStrategy, "retryPolicy");

            this.ErrorDetectionStrategy = errorDetectionStrategy;

            if (errorDetectionStrategy == null)
            {
                throw new InvalidOperationException("The error detection strategy type must implement the ITransientErrorDetectionStrategy interface.");
            }

            this.RetryStrategy = retryStrategy;
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and default fixed time interval between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy"/> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount)
            : this(errorDetectionStrategy, new FixedInterval(retryCount))
        {
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and fixed time interval between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy"/> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="retryInterval">The interval between retries.</param>
        public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan retryInterval)
            : this(errorDetectionStrategy, new FixedInterval(retryCount, retryInterval))
        {
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and back-off parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy"/> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="minBackoff">The minimum back-off time.</param>
        /// <param name="maxBackoff">The maximum back-off time.</param>
        /// <param name="deltaBackoff">The time value that will be used for calculating a random delta in the exponential delay between retries.</param>
        public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
            : this(errorDetectionStrategy, new ExponentialBackoff(retryCount, minBackoff, maxBackoff, deltaBackoff))
        {
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and parameters defining the progressive delay between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy"/> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
        /// <param name="increment">The incremental time value that will be used for calculating the progressive delay between retries.</param>
        public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : this(errorDetectionStrategy, new Incremental(retryCount, initialInterval, increment))
        {
        }

        /// <summary>
        /// An instance of a callback delegate that will be invoked whenever a retry condition is encountered.
        /// </summary>
        public event EventHandler<RetryingEventArgs> Retrying;

        /// <summary>
        /// Gets the retry strategy.
        /// </summary>
        public RetryStrategy RetryStrategy { get; private set; }

        /// <summary>
        /// Gets the instance of the error detection strategy.
        /// </summary>
        public ITransientErrorDetectionStrategy ErrorDetectionStrategy { get; private set; }

        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="action">A delegate representing the executable action which doesn't return any results.</param>
        public virtual void ExecuteAction(Action action)
        {
            Guard.ArgumentNotNull(action, "action");

            this.ExecuteAction(() => { action(); return default(object); });
        }

        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of result expected from the executable action.</typeparam>
        /// <param name="func">A delegate representing the executable action which returns the result of type R.</param>
        /// <returns>The result from the action.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with Guard")]
        public virtual TResult ExecuteAction<TResult>(Func<TResult> func)
        {
            Guard.ArgumentNotNull(func, "func");

            int retryCount = 0;
            TimeSpan delay = TimeSpan.Zero;
            Exception lastError;

            var shouldRetry = this.RetryStrategy.GetShouldRetry();

            for (;;)
            {
                lastError = null;

                try
                {
                    return func();
                }
                catch (RetryLimitExceededException limitExceededEx)
                {
                    // The user code can throw a RetryLimitExceededException to force the exit from the retry loop.
                    // The RetryLimitExceeded exception can have an inner exception attached to it. This is the exception
                    // which we will have to throw up the stack so that callers can handle it.
                    if (limitExceededEx.InnerException != null)
                    {
                        throw limitExceededEx.InnerException;
                    }
                    else
                    {
                        return default(TResult);
                    }
                }
                catch (Exception ex)
                {
                    lastError = ex;

                    if (!(this.ErrorDetectionStrategy.IsTransient(lastError) && shouldRetry(retryCount++, lastError, out delay)))
                    {
                        throw;
                    }
                }

                // Perform an extra check in the delay interval. Should prevent from accidentally ending up with the value of -1 that will block a thread indefinitely. 
                // In addition, any other negative numbers will cause an ArgumentOutOfRangeException fault that will be thrown by Thread.Sleep.
                if (delay.TotalMilliseconds < 0)
                {
                    delay = TimeSpan.Zero;
                }

                this.OnRetrying(retryCount, lastError, delay);

                if (retryCount > 1 || !this.RetryStrategy.FastFirstRetry)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        /// <summary>
        /// Repetitively executes the specified asynchronous action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="beginAction">The begin method of the async pattern.</param>
        /// <param name="endAction">The end method of the async pattern.</param>
        /// <param name="successHandler">The action to perform when the async operation is done.</param>
        /// <param name="faultHandler">The fault handler delegate that will be triggered if the operation cannot be successfully invoked despite retry attempts.</param>
        public virtual void ExecuteAction(Action<AsyncCallback> beginAction, Action<IAsyncResult> endAction, Action successHandler, Action<Exception> faultHandler)
        {
            Guard.ArgumentNotNull(endAction, "endAction");
            successHandler = successHandler ?? (() => { });

            this.ExecuteAction<object>(
                beginAction,
                ar => { endAction(ar); return null; },
                _ => successHandler(),
                faultHandler);
        }

        /// <summary>
        /// Repetitively executes the specified asynchronous action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of the object returned by the async operation.</typeparam>
        /// <param name="beginAction">The begin method of the async pattern.</param>
        /// <param name="endAction">The end method of the async pattern.</param>
        /// <param name="successHandler">The action to perform when the async operation is done.</param>
        /// <param name="faultHandler">The fault handler delegate that will be triggered if the operation cannot be successfully invoked despite retry attempts.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Needs to catch all exceptions to test them.")]
        public virtual void ExecuteAction<TResult>(Action<AsyncCallback> beginAction, Func<IAsyncResult, TResult> endAction, Action<TResult> successHandler, Action<Exception> faultHandler)
        {
            Guard.ArgumentNotNull(beginAction, "beginAction");
            Guard.ArgumentNotNull(endAction, "endAction");
            successHandler = successHandler ?? (_ => { });
            faultHandler = faultHandler ?? (_ => { });

            int retryCount = 0;
            AsyncCallback endInvoke = null;
            Func<Action, bool> executeWithRetry = null;

            var shouldRetry = this.RetryStrategy.GetShouldRetry();

            // Configure a custom callback delegate that invokes the end operation and the success handler if the operation succeedes
            endInvoke =
                ar =>
                {
                    var result = default(TResult);

                    if (executeWithRetry(() => result = endAction(ar)))
                    {
                        successHandler(result);
                    }
                };

            // Utility delegate to invoke an action and implement the core retry logic
            // If the action succeeds (i.e. does not throw an exception) it returns true.
            // If the action throws, it analizes it for retries. If a retry is required, it restarts the async operation; otherwise, it invokes the fault handler.
            executeWithRetry =
                a =>
                {
                    try
                    {
                        // Invoke the callback delegate which can throw an exception if the main async operation has completed with a fault.
                        a();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Capture the original exception for analysis.
                        var lastError = ex;

                        // Handling of RetryLimitExceededException needs to be done separately. This exception type indicates the application's intent to exit from the retry loop.
                        if (lastError is RetryLimitExceededException)
                        {
                            if (lastError.InnerException != null)
                            {
                                faultHandler(lastError.InnerException);
                            }
                            else
                            {
                                faultHandler(lastError);
                            }
                        }
                        else
                        {
                            var delay = TimeSpan.Zero;

                            // Check if we should continue retrying on this exception. If not, invoke the fault handler so that user code can take control.
                            if (!(this.ErrorDetectionStrategy.IsTransient(lastError) && shouldRetry(retryCount++, lastError, out delay)))
                            {
                                faultHandler(lastError);
                            }
                            else
                            {
                                // Notify the respective subscribers about this exception.
                                this.OnRetrying(retryCount, lastError, delay);

                                // Sleep for the defined interval before repetitively executing the main async operation.
                                if (retryCount > 1 || !this.RetryStrategy.FastFirstRetry)
                                {
                                    Thread.Sleep(delay);
                                }

                                executeWithRetry(() => beginAction(endInvoke));
                            }
                        }

                        return false;
                    }
                };

            // Invoke the the main async operation for the first time which should return control to the caller immediately.
            executeWithRetry(() => beginAction(endInvoke));
        }

        /// <summary>
        /// Notifies the subscribers whenever a retry condition is encountered.
        /// </summary>
        /// <param name="retryCount">The current retry attempt count.</param>
        /// <param name="lastError">The exception which caused the retry conditions to occur.</param>
        /// <param name="delay">The delay indicating how long the current thread will be suspended for before the next iteration will be invoked.</param>
        protected virtual void OnRetrying(int retryCount, Exception lastError, TimeSpan delay)
        {
            if (this.Retrying != null)
            {
                this.Retrying(this, new RetryingEventArgs(retryCount, delay, lastError));
            }
        }

        #region Private classes
        /// <summary>
        /// Implements a strategy that ignores any transient errors.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through generics")]
        private sealed class TransientErrorIgnoreStrategy : ITransientErrorDetectionStrategy
        {
            /// <summary>
            /// Always return false.
            /// </summary>
            /// <param name="ex">The exception.</param>
            /// <returns>Returns false.</returns>
            public bool IsTransient(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Implements a strategy that treats all exceptions as transient errors.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through generics")]
        private sealed class TransientErrorCatchAllStrategy : ITransientErrorDetectionStrategy
        {
            /// <summary>
            /// Always return true.
            /// </summary>
            /// <param name="ex">The exception.</param>
            /// <returns>Returns true.</returns>
            public bool IsTransient(Exception ex)
            {
                return true;
            }
        }
        #endregion
    }
}

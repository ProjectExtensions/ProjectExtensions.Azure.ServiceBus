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
namespace Microsoft.AzureCAT.Samples.TransientFaultHandling
{
    #region Using references
    using System;
    using System.Threading;
    using System.Globalization;
    #endregion

    /// <summary>
    /// Defines a callback delegate which will be invoked whenever a retry condition is encountered.
    /// </summary>
    /// <param name="currentRetryCount">The current retry attempt count.</param>
    /// <param name="lastException">The exception which caused the retry conditions to occur.</param>
    /// <param name="delay">The delay indicating how long the current thread will be suspended for before the next iteration will be invoked.</param>
    public delegate void RetryCallbackDelegate(int currentRetryCount, Exception lastException, TimeSpan delay);

    /// <summary>
    /// Provides the base implementation of the retry mechanism for unreliable actions and transient conditions.
    /// </summary>
    public abstract class RetryPolicy
    {
        #region Public members
        /// <summary>
        /// The default number of retry attempts.
        /// </summary>
        public static readonly int DefaultClientRetryCount = 10;

        /// <summary>
        /// The default amount of time used when calculating a random delta in the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultClientBackoff = TimeSpan.FromSeconds(10.0);

        /// <summary>
        /// The default maximum amount of time used when calculating the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultMaxBackoff = TimeSpan.FromSeconds(30.0);

        /// <summary>
        /// The default minimum amount of time used when calculating the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultMinBackoff = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// The default amount of time defining an interval between retries.
        /// </summary>
        public static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// The default amount of time defining a time increment between retry attempts in the progressive delay policy.
        /// </summary>
        public static readonly TimeSpan DefaultRetryIncrement = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Returns a default policy that does no retries, it just invokes action exactly once.
        /// </summary>
        public static readonly RetryPolicy NoRetry = new RetryPolicy<TransientErrorIgnoreStrategy>(0);

        /// <summary>
        /// Returns a default policy that implements a fixed retry interval configured with <see cref="RetryPolicy.DefaultClientRetryCount"/> and <see cref="RetryPolicy.DefaultRetryInterval"/> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryPolicy DefaultFixed = new RetryPolicy<TransientErrorCatchAllStrategy>(DefaultClientRetryCount, DefaultRetryInterval);

        /// <summary>
        /// Returns a default policy that implements a progressive retry interval configured with <see cref="RetryPolicy.DefaultClientRetryCount"/>, <see cref="RetryPolicy.DefaultRetryInterval"/> and <see cref="RetryPolicy.DefaultRetryIncrement"/> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryPolicy DefaultProgressive = new RetryPolicy<TransientErrorCatchAllStrategy>(DefaultClientRetryCount, DefaultRetryInterval, DefaultRetryIncrement);

        /// <summary>
        /// Returns a default policy that implements a random exponential retry interval configured with <see cref="RetryPolicy.DefaultClientRetryCount"/>, <see cref="RetryPolicy.DefaultMinBackoff"/>, <see cref="RetryPolicy.DefaultMaxBackoff"/> and <see cref="RetryPolicy.DefaultClientBackoff"/> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static readonly RetryPolicy DefaultExponential = new RetryPolicy<TransientErrorCatchAllStrategy>(DefaultClientRetryCount, DefaultMinBackoff, DefaultMaxBackoff, DefaultClientBackoff);
        #endregion

        #region Public properties
        /// <summary>
        /// An instance of a callback delegate which will be invoked whenever a retry condition is encountered.
        /// </summary>
        public event RetryCallbackDelegate RetryOccurred;

        /// <summary>
        /// Gets or sets a flag indicating whether or not the very first retry attempt will be made immediately
        /// whereas the subsequent retries will remain subject to retry interval.
        /// </summary>
        public bool FastFirstRetry { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="action">A delegate representing the executable action which doesn't return any results.</param>
        public abstract void ExecuteAction(Action action);

        /// <summary>
        /// Repetitively executes the specified asynchronous action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="action">A delegate representing the executable action that must invoke an asynchronous operation and return its <see cref="IAsyncResult"/>.</param>
        /// <param name="callback">The callback delegate that will be triggered when the main asynchronous operation completes.</param>
        /// <param name="faultHandler">The fault handler delegate that will be triggered if the operation cannot be successfully invoked despite retry attempts.</param>
        public abstract void ExecuteAction(Action<AsyncCallback> action, Action<IAsyncResult> callback, Action<Exception> faultHandler);

        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="T">The type of result expected from the executable action.</typeparam>
        /// <param name="func">A delegate representing the executable action which returns the result of type T.</param>
        /// <returns>The result from the action.</returns>
        public abstract T ExecuteAction<T>(Func<T> func);
        #endregion

        #region Protected members
        /// <summary>
        /// Notifies the subscribers whenever a retry condition is encountered.
        /// </summary>
        /// <param name="retryCount">The current retry attempt count.</param>
        /// <param name="lastError">The exception which caused the retry conditions to occur.</param>
        /// <param name="delay">The delay indicating how long the current thread will be suspended for before the next iteration will be invoked.</param>
        protected virtual void OnRetryOccurred(int retryCount, Exception lastError, TimeSpan delay)
        {
            if (RetryOccurred != null)
            {
                RetryOccurred(retryCount, lastError, delay);
            }
        }
        #endregion

        #region Private classes
        /// <summary>
        /// Implements a strategy that ignores any transient errors.
        /// </summary>
        private sealed class TransientErrorIgnoreStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Implements a strategy that treats all exceptions as transient errors.
        /// </summary>
        private sealed class TransientErrorCatchAllStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                return true;
            }
        }
        #endregion
    }

    /// <summary>
    /// Extends the base <see cref="RetryPolicy"/> implementation with strategy objects capable of detecting transient conditions.
    /// </summary>
    /// <typeparam name="T">The type implementing the <see cref="ITransientErrorDetectionStrategy"/> interface which is responsible for detecting transient conditions.</typeparam>
    public class RetryPolicy<T> : RetryPolicy where T : ITransientErrorDetectionStrategy, new()
    {
        #region Private members
        private readonly T errorDetectionStrategy = new T();
        private readonly ShouldRetry shouldRetry;

        /// <summary>
        /// Defines a delegate that is responsible for notifying the subscribers whenever a retry condition is encountered.
        /// </summary>
        /// <param name="retryCount">The current retry attempt count.</param>
        /// <param name="lastException">The exception which caused the retry conditions to occur.</param>
        /// <param name="delay">The delay indicating how long the current thread will be suspended for before the next iteration will be invoked.</param>
        protected delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with default settings.
        /// </summary>
        private RetryPolicy()
        {
            FastFirstRetry = true;
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and default fixed time interval between retries.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        public RetryPolicy(int retryCount) : this(retryCount, DefaultRetryInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and fixed time interval between retries.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="retryInterval">The interval between retries.</param>
        public RetryPolicy(int retryCount, TimeSpan retryInterval) : this()
        {
            Guard.ArgumentNotNegativeValue(retryCount, "retryCount");
            Guard.ArgumentNotNegativeValue(retryInterval.Ticks, "retryInterval");

            if (0 == retryCount)
            {
                this.shouldRetry = delegate(int currentRetryCount, Exception lastException, out TimeSpan interval)
                {
                    interval = TimeSpan.Zero;
                    return false;
                };
            }
            else
            {
                this.shouldRetry = delegate(int currentRetryCount, Exception lastException, out TimeSpan interval)
                {
                    if (this.errorDetectionStrategy.IsTransient(lastException))
                    {
                        interval = retryInterval;
                        return (currentRetryCount < retryCount);
                    }
                    else
                    {
                        interval = TimeSpan.Zero;
                        return false;
                    }
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and back-off parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="minBackoff">The minimum back-off time.</param>
        /// <param name="maxBackoff">The maximum back-off time.</param>
        /// <param name="deltaBackoff">The time value which will be used for calculating a random delta in the exponential delay between retries.</param>
        public RetryPolicy(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff) : this()
        {
            Guard.ArgumentNotNegativeValue(retryCount, "retryCount");
            Guard.ArgumentNotNegativeValue(minBackoff.Ticks, "minBackoff");
            Guard.ArgumentNotNegativeValue(maxBackoff.Ticks, "maxBackoff");
            Guard.ArgumentNotNegativeValue(deltaBackoff.Ticks, "deltaBackoff");
            Guard.ArgumentNotGreaterThan(minBackoff.TotalMilliseconds, maxBackoff.TotalMilliseconds, "minBackoff");

            this.shouldRetry = delegate(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
            {
                if (this.errorDetectionStrategy.IsTransient(lastException) && currentRetryCount < retryCount)
                {
                    Random random = new Random();

                    int delta = (int)((Math.Pow(2.0, (double)currentRetryCount) - 1.0) * random.Next((int)(deltaBackoff.TotalMilliseconds * 0.8), (int)(deltaBackoff.TotalMilliseconds * 1.2)));
                    int interval = (int)Math.Min(checked(minBackoff.TotalMilliseconds + delta), maxBackoff.TotalMilliseconds);

                    retryInterval = TimeSpan.FromMilliseconds((double)interval);

                    return true;
                }

                retryInterval = TimeSpan.Zero;
                return false;
            };
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and parameters defining the progressive delay between retries.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval which will apply for the first retry.</param>
        /// <param name="increment">The incremental time value which will be used for calculating the progressive delay between retries.</param>
        public RetryPolicy(int retryCount, TimeSpan initialInterval, TimeSpan increment) : this()
        {
            Guard.ArgumentNotNegativeValue(retryCount, "retryCount");
            Guard.ArgumentNotNegativeValue(initialInterval.Ticks, "initialInterval");

            this.shouldRetry = delegate(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
            {
                if (this.errorDetectionStrategy.IsTransient(lastException) && currentRetryCount < retryCount)
                {
                    retryInterval = TimeSpan.FromMilliseconds(initialInterval.TotalMilliseconds + increment.TotalMilliseconds * currentRetryCount);

                    return true;
                }

                retryInterval = TimeSpan.Zero;
                return false;
            };
        }
        #endregion

        #region RetryPolicy implementation
        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="action">A delegate representing the executable action which doesn't return any results.</param>
        public override void ExecuteAction(Action action)
        {
            int retryCount = 0;
            TimeSpan delay = TimeSpan.Zero;
            Exception lastError = null;

            for (; ; )
            {
                lastError = null;

                try
                {
                    action();
                    return;
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
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lastError = ex;

                    if (!this.shouldRetry(retryCount++, lastError, out delay))
                    {
                        throw;
                    }
                }

                // Perform an extra check in the delay interval. Should prevent from accidentally ending up with the value of -1 which will block a thread indefinitely. 
                // In addition, any other negative numbers will cause an ArgumentOutOfRangeException fault which will be thrown by Thread.Sleep.
                if (delay.TotalMilliseconds < 0)
                {
                    delay = TimeSpan.Zero;
                }

                OnRetryOccurred(retryCount, lastError, delay);

                if (retryCount > 1 || !FastFirstRetry)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="R">The type of result expected from the executable action.</typeparam>
        /// <param name="func">A delegate representing the executable action which returns the result of type R.</param>
        /// <returns>The result from the action.</returns>
        public override R ExecuteAction<R>(Func<R> func)
        {
            int retryCount = 0;
            TimeSpan delay = TimeSpan.Zero;
            Exception lastError = null;

            for (; ; )
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
                        return default(R);
                    }
                }
                catch (Exception ex)
                {
                    lastError = ex;

                    if (!this.shouldRetry(retryCount++, lastError, out delay))
                    {
                        throw;
                    }
                }

                // Perform an extra check in the delay interval. Should prevent from accidentally ending up with the value of -1 which will block a thread indefinitely. 
                // In addition, any other negative numbers will cause an ArgumentOutOfRangeException fault which will be thrown by Thread.Sleep.
                if (delay.TotalMilliseconds < 0)
                {
                    delay = TimeSpan.Zero;
                }

                OnRetryOccurred(retryCount, lastError, delay);

                if (retryCount > 1 || !FastFirstRetry)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        /// <summary>
        /// Repetitively executes the specified asynchronous action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="action">A delegate representing the executable action that must invoke an asynchronous operation and return its <see cref="IAsyncResult"/>.</param>
        /// <param name="callback">The callback delegate that will be triggered when the main asynchronous operation completes.</param>
        /// <param name="faultHandler">The fault handler delegate that will be triggered if the operation cannot be successfully invoked despite retry attempts.</param>
        public override void ExecuteAction(Action<AsyncCallback> action, Action<IAsyncResult> callback, Action<Exception> faultHandler)
        {
            int retryCount = 0;
            AsyncCallback endInvoke = null;

            // Configure a custom callback delegate that implements the core retry logic.
            endInvoke = ((ar) =>
            {
                Exception lastError = null;
                TimeSpan delay = TimeSpan.Zero;

                try
                {
                    // Invoke the callback delegate which can throw an exception if the main async operation has completed with a fault.
                    callback(ar);
                    return;
                }
                catch (Exception ex)
                {
                    // Capture the original exception for analysis.
                    lastError = ex;
                }

                // Check if the main async operation has been unsuccessful.
                if (lastError != null)
                {
                    // Handling of RetryLimitExceededException needs to be done separately. This exception type indicates the application's intent to exit from the retry loop.
                    if (lastError is RetryLimitExceededException)
                    {
                        if (lastError.InnerException != null)
                        {
                            faultHandler(lastError.InnerException);
                        }
                        return;
                    }

                    // Check if we should continue retrying on this exception. If not, invoke the fault handler so that user code can take control.
                    if (!this.shouldRetry(retryCount++, lastError, out delay))
                    {
                        faultHandler(lastError);
                        return;
                    }

                    // Notify the respective subscribers about this exception.
                    OnRetryOccurred(retryCount, lastError, delay);

                    // Sleep for the defined interval before repetitively executing the main async operation.
                    if (retryCount > 1 || !FastFirstRetry)
                    {
                        Thread.Sleep(delay);
                    }

                    action(endInvoke);
                }
            });

            // Invoke the the main async operation for the first time which should return control to the caller immediately.
            action(endInvoke);
        }
        #endregion
    }
}

using NLog;
using System;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Extension Methods
    /// </summary>
    public static class ExtensionMethods {

        static Logger logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Execute and catch any exception that is thrown and swallow it.
        /// </summary>
        /// <param name="action">The action to perform</param>
        public static void ExecuteAndReturn(Action action) {
            try {
                action();
            }
            catch (Exception ex) {
                logger.Error("ExecuteAndReturn Failed {0}", ex);
            }
        }

    }
}

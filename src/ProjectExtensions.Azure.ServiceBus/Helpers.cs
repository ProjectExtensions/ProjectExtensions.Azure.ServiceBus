using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using NLog;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Helper Classes
    /// </summary>
    public static class Helpers {
        
        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Execute a method 5 times and it it failes that many times, throw the exception back
        /// </summary>
        /// <remarks>Very crude retry logic</remarks>
        /// <param name="action"></param>
        public static void Execute(Action action) {
            Execute(action, 5);
        }

        /// <summary>
        /// Execute a method n times and it it failes that many times, throw the exception back
        /// </summary>
        /// <remarks>Very crude retry logic</remarks>
        /// <param name="action"></param>
        /// <param name="retries"></param>
        public static void Execute(Action action, int retries) {

            for (int i = 0; i < retries; i++) {
                try {
                    action();
                    break;
                }
                catch (Exception ex) {
                    logger.Log(LogLevel.Error, "Send Action={0} Count={1} Error={2}", action.Target.ToString(), i, ex.ToString());
                    if (i + 1 == retries) {
                        throw;
                    }                    
                }
            }
        }

        /// <summary>
        /// Calculates MD5 hash
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="enc">Character encoding</param>
        /// <returns>MD5 hash</returns>
        public static string CalculateMD5(string text) {
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(text);
            var cryptoTransformSHA1 = new MD5CryptoServiceProvider();
            string hash = BitConverter.ToString(
                cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");

            return hash;
        }
    }
}

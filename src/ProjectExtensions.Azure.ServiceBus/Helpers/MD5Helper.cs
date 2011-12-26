using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace ProjectExtensions.Azure.ServiceBus.Helpers {
    
    class MD5Helper {

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ProjectExtensions.Azure.ServiceBus {

    static class CloudEnvironment {
    
        /// <summary>
        /// Ensures that the HttpContext object is safe to use in the given context and returns an object that rolls the HttpContext object back to its original state.
        /// </summary>
        /// <returns>An object that needs to be explicitly disposed so that HttpContext can return back to its original state.</returns>
        public static IDisposable EnsureSafeHttpContext() {
            HttpContext oldHttpContext = HttpContext.Current;
            HttpContext.Current = null;

            return new AnonymousDisposable(() => {
                HttpContext.Current = oldHttpContext;
            });
        }
    }
}

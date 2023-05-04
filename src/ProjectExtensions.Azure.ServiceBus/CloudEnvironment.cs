using System;
using ProjectExtensions.Azure.ServiceBus;

namespace ProjectExtensions.Azure.ServiceBus {

    static class CloudEnvironment {

        /// <summary>
        /// Ensures that the HttpContext object is safe to use in the given context and returns an object that rolls the HttpContext object back to its original state.
        /// </summary>
        /// <returns>An object that needs to be explicitly disposed so that HttpContext can return back to its original state.</returns>
        public static IDisposable EnsureSafeHttpContext() {
            var oldHttpContext = HttpContextHelper.Current;
            //HttpContextHelper.Current = null;

            return new AnonymousDisposable(() => {
                //HttpContextHelper.Current = oldHttpContext;
            });
        }
    }
}

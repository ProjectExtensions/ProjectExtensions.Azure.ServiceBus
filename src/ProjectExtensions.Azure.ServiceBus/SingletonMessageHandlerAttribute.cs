using System;

namespace ProjectExtensions.Azure.ServiceBus {
    /// <summary>
    /// Marks message handler as a singleton.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonMessageHandlerAttribute : Attribute {}
}
using System;

namespace ProjectExtensions.Azure.ServiceBus {
    /// <summary>
    /// Marks message handler as a singleton.
    /// </summary>
    [Obsolete("Use the Singleton property of MessageHandlerConfiguration", true)]
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonMessageHandlerAttribute : Attribute { }
}
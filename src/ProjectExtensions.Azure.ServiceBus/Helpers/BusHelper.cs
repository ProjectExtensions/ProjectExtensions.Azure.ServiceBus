using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.TransientFaultHandling;
using NLog;

namespace ProjectExtensions.Azure.ServiceBus.Helpers {

    /// <summary>
    /// Generic code in the bus that can be reused by the mocks
    /// </summary>
    internal static class BusHelper {

        internal static void SubscribeOrUnsubscribeType(Action<string> logInfo, Type type, IBusConfiguration config, Action<ServiceBusEnpointData> callback) {
            Guard.ArgumentNotNull(type, "type");
            Guard.ArgumentNotNull(callback, "callback");

            logInfo(string.Format("SubscribeOrUnsubscribeType={0}", type.FullName));
            var interfaces = type.GetInterfaces()
                            .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IHandleMessages<>) || i.GetGenericTypeDefinition() == typeof(IHandleCompetingMessages<>)))
                            .ToList();

            if (interfaces.Count == 0) {
                throw new ApplicationException(string.Format("Type {0} does not implement IHandleMessages or IHandleCompetingMessages", type.FullName));
            }

            //for each interface we find, we need to register it with the bus.
            foreach (var foundInterface in interfaces) {

                var implementedMessageType = foundInterface.GetGenericArguments()[0];
                //due to the limits of 50 chars we will take the name and a MD5 for the name.
                var hashName = implementedMessageType.FullName + "|" + type.FullName;

                var hash = MD5Helper.CalculateMD5(hashName);
                var fullName = (IsCompetingHandler(foundInterface) ? "C_" : config.ServiceBusApplicationId + "_") + hash;

                var info = new ServiceBusEnpointData() {
                    AttributeData = type.GetCustomAttributes(typeof(MessageHandlerConfigurationAttribute), false).FirstOrDefault() as MessageHandlerConfigurationAttribute,
                    DeclaredType = type,
                    MessageType = implementedMessageType,
                    SubscriptionName = fullName,
                    ServiceType = foundInterface
                };

                if (!config.Container.IsRegistered(type)) {
                    if (info.IsReusable) {
                        config.Container.Register(type, type);
                    }
                    else {
                        config.Container.Register(type, type, true);
                    }
                }

                callback(info);
            }

            config.Container.Build();
        }

        static bool IsCompetingHandler(Type type) {
            return type.GetGenericTypeDefinition() == typeof(IHandleCompetingMessages<>);
        }
    }
}

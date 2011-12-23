using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ProjectExtensions.Azure.ServiceBus.Container;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using Microsoft.Practices.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Class used for configuration
    /// </summary>
    public class BusConfiguration : ProjectExtensions.Azure.ServiceBus.IBusConfiguration {

        static object lockObject = new object();
        static BusConfiguration configuration;

        internal IAzureBusContainer container;
        List<Assembly> registeredAssemblies = new List<Assembly>();
        List<Type> registeredSubscribers = new List<Type>();

        /// <summary>
        /// ctor
        /// </summary>
        internal BusConfiguration() {
            MaxThreads = 1;
            TopicName = "pro_ext_topic";
        }

        /// <summary>
        /// The Service Bus
        /// </summary>
        public IBus Bus {
            get {
                return container.Resolve<IBus>();
            }
        }

        /// <summary>
        /// The IOC Container for the application
        /// </summary>
        public static IAzureBusContainer Container {
            get {
                return configuration.container;
            }
            internal set {
                Guard.ArgumentNotNull(value, "Container");
                configuration.container = value;
            }
        }

        /// <summary>
        /// Gets and sets the IOC container for the current instance.
        /// </summary>
        public IAzureBusContainer InstanceContainer {
            get {
                return configuration.container;
            }
            set {
                Guard.ArgumentNotNull(value, "conttainer");
                container = value;
            }
        }

        /// <summary>
        /// DefaultSerializer
        /// </summary>
        public IServiceBusSerializer DefaultSerializer {
            get {
                return container.Resolve<IServiceBusSerializer>();
            }
        }

        /// <summary>
        /// Instance of BusConfiguration
        /// </summary>
        public static BusConfiguration Instance {
            get {
                //TODO should this throw an exception if it was not created?
                return configuration;
            }
        }

        /// <summary>
        /// Max Threads to call the message handlers from the bus messages being received
        /// </summary>
        public byte MaxThreads {
            get;
            internal set;
        }

        /// <summary>
        /// List of RegisteredAssemblies
        /// </summary>
        public IList<Assembly> RegisteredAssemblies {
            get {
                return this.registeredAssemblies;
            }
        }

        /// <summary>
        /// List of RegisteredSubscribers
        /// </summary>
        public IList<Type> RegisteredSubscribers {
            get {
                return this.registeredSubscribers;
            }
        }

        /// <summary>
        /// ServiceBusApplicationId
        /// </summary>
        public string ServiceBusApplicationId {
            get;
            internal set;
        }

        /// <summary>
        /// ServiceBusNamespace
        /// </summary>
        public string ServiceBusNamespace {
            get;
            internal set;
        }

        /// <summary>
        /// ServiceBusIssuerName
        /// </summary>
        public string ServiceBusIssuerName {
            get;
            internal set;
        }

        /// <summary>
        /// ServiceBusIssuerKey
        /// </summary>
        public string ServiceBusIssuerKey {
            get;
            internal set;
        }

        /// <summary>
        /// ServicePath
        /// </summary>
        public string ServicePath {
            get;
            internal set;
        }

        /// <summary>
        /// TopicName
        /// </summary>
        public string TopicName {
            get;
            internal set;
        }

        internal void Configure() {
            if (string.IsNullOrWhiteSpace(ServiceBusApplicationId)) {
                throw new ApplicationException("ApplicationId must be set.");
            }

            container.RegisterBus(this);
            if (!container.IsRegistered(typeof(IAzureBusReceiver))) {
                container.Register(typeof(IAzureBusReceiver), typeof(AzureBusReceiver));
            }
            if (!container.IsRegistered(typeof(IAzureBusSender))) {
                container.Register(typeof(IAzureBusSender), typeof(AzureBusSender));
            }
            if (!container.IsRegistered(typeof(IServiceBusSerializer))) {
                container.Register(typeof(IServiceBusSerializer), typeof(JsonServiceBusSerializer));
            }
            container.Build();
            
            //Set the Bus property so that the receiver will register the end points
            var prime = this.Bus;
        }

        /// <summary>
        /// Get the settings builder optionally passing in your existing IOC Container
        /// </summary>
        /// <returns></returns>
        public static BusConfigurationBuilder WithSettings() {
            if (configuration == null) {
                lock (lockObject) {
                    if (configuration == null) {
                        configuration = new BusConfiguration();
                    }
                }
            }

            return new BusConfigurationBuilder(configuration);
        }

        internal void AddRegisteredAssembly(Assembly value) {
            Guard.ArgumentNotNull(value, "value");
            if (!this.registeredAssemblies.Contains(value)) {
                this.registeredAssemblies.Add(value);
            }
        }

        internal void AddRegisteredSubscriber(Type value) {
            Guard.ArgumentNotNull(value, "value");
            if (!this.registeredSubscribers.Contains(value)) {
                this.registeredSubscribers.Add(value);
            }
        }

    }
}

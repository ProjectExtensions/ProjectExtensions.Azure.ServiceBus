using Microsoft.Practices.TransientFaultHandling;
using ProjectExtensions.Azure.ServiceBus.AzureServiceBusFactories;
using ProjectExtensions.Azure.ServiceBus.Container;
using ProjectExtensions.Azure.ServiceBus.Factories;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Receiver;
using ProjectExtensions.Azure.ServiceBus.Sender;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Class used for configuration
    /// </summary>
    public class BusConfiguration : IBusConfiguration {
        static readonly BusConfiguration configuration = new BusConfiguration();

        IAzureBusContainer container;
        List<Assembly> registeredAssemblies = new List<Assembly>();
        List<Type> registeredSubscribers = new List<Type>();

        //Explicit static constructor so type will not be marked beforefieldinit.  
        //See http://www.yoda.arachsys.com/csharp/singleton.html.
        static BusConfiguration() {
        }

        /// <summary>
        /// ctor
        /// </summary>
        private BusConfiguration() {
            MaxThreads = 1;
            TopicName = "pro_ext_topic";
        }

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static IBusConfiguration Instance {
            get {
                return configuration;
            }
        }

        //we are changing this to be built on the configure because once it is resolved once, nothing can change.
        //we don't allow the configuration to be changed on it so there is no point to resolving every time.

        /// <summary>
        /// The Service Bus
        /// </summary>
        public IBus Bus {
            get;
            private set;
        }

        /// <summary>
        /// The IOC Container for the application
        /// </summary>
        /// <remarks>The setter can only be called once.  This is normally done via the builder.
        /// You should not call the setter directly.</remarks>
        public IAzureBusContainer Container {
            get {
                return container;
            }
            set {
                if (container != null) {
                    throw new NotSupportedException("The container can only be set once.");
                }
                Guard.ArgumentNotNull(value, "Container");
                container = value;
            }
        }


        /// <summary>
        /// DefaultSerializer
        /// </summary>
        public IServiceBusSerializer DefaultSerializer {
            get {
                return container.DefaultSerializer;
            }
        }

        /// <summary>
        /// Enable Partitioning
        /// </summary>
        public bool EnablePartitioning {
            get;
            internal set;
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

        /// <summary>
        /// Apply the configuration
        /// </summary>
        public void Configure() {
            if (string.IsNullOrWhiteSpace(ServiceBusApplicationId)) {
                throw new ApplicationException("ApplicationId must be set.");
            }

            container.RegisterConfiguration();
            if (!container.IsRegistered(typeof(IBus))) {
                container.Register(typeof(IBus), typeof(AzureBus));
            }
            if (!container.IsRegistered(typeof(IAzureBusReceiver))) {
                container.Register(typeof(IAzureBusReceiver), typeof(AzureBusReceiver));
            }
            if (!container.IsRegistered(typeof(IAzureBusSender))) {
                container.Register(typeof(IAzureBusSender), typeof(AzureBusSender));
            }
            if (!container.IsRegistered(typeof(IServiceBusConfigurationFactory))) {
                container.Register(typeof(IServiceBusConfigurationFactory), typeof(GenericServiceBusConfigurationFactory));
            }
            //Only used by Azure since we can't create an instance of the Token without calling AppFabric.
            if (!container.IsRegistered(typeof(IServiceBusTokenProvider))) {
                container.Register(typeof(IServiceBusTokenProvider), typeof(ServiceBusTokenProvider));
            }
            if (!container.IsRegistered(typeof(INamespaceManager))) {
                container.Register(typeof(INamespaceManager), typeof(ServiceBusNamespaceManagerFactory));
            }
            if (!container.IsRegistered(typeof(IMessagingFactory))) {
                container.Register(typeof(IMessagingFactory), typeof(ServiceBusMessagingFactoryFactory));
            }
            if (!container.IsRegistered(typeof(IServiceBusSerializer))) {
                container.Register(typeof(IServiceBusSerializer), typeof(JsonServiceBusSerializer));
            }
            container.Build();

            //Set the Bus property so that the receiver will register the end points
            this.Bus = container.Bus;
            this.Bus.Initialize();
        }

        /// <summary>
        /// Get the settings builder optionally passing in your existing IOC Container
        /// </summary>
        /// <returns></returns>
        public static BusConfigurationBuilder WithSettings() {
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

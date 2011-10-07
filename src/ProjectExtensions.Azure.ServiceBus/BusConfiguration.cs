using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Autofac;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using Microsoft.AzureCAT.Samples.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Class used for configuration
    /// </summary>
    public class BusConfiguration : ProjectExtensions.Azure.ServiceBus.IBusConfiguration {

        static object lockObject = new object();
        static BusConfiguration configuration;

        IContainer container;
        List<Assembly> registeredAssemblies = new List<Assembly>();
        List<Type> registeredSubscribers = new List<Type>();

        /// <summary>
        /// ctor
        /// </summary>
        internal BusConfiguration(IContainer container = null) {
            MaxThreads = 1;
            TopicName = "pro_ext_topic";
            this.container = container;
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
        /// The IOC Container
        /// </summary>
        public static IContainer Container {
            get {
                return configuration.container;
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

        internal void Configure(ContainerBuilder builder) {
            Guard.ArgumentNotNull(builder, "builder");
            if (string.IsNullOrWhiteSpace(ServiceBusApplicationId)) {
                throw new ApplicationException("ApplicationId must be set.");
            }

            builder.Register<AzureBus>(item => new AzureBus(this)).As<IBus>().SingleInstance();

            builder.RegisterType<AzureBusReceiver>().As<IAzureBusReceiver>().SingleInstance();
            builder.RegisterType<AzureBusSender>().As<IAzureBusSender>().SingleInstance();

            if (container == null) {
                container = builder.Build();
            }
            else {
                builder.Update(container);
            }

            //Set the Bus property so that the receiver will register the end points
            var prime = this.Bus;
        }

        /// <summary>
        /// Get the settings builder optionally passing in your existing IOC Container
        /// </summary>
        /// <param name="container">Your optional existing IOC container.</param>
        /// <returns></returns>
        public static BusConfigurationBuilder WithSettings() {
            return WithSettings(null);
        }

        /// <summary>
        /// Get the settings builder optionally passing in your existing IOC Container
        /// </summary>
        /// <param name="container">Your optional existing IOC container. Use <see cref="WithSettings()"/> if you do not have an existing container.</param>
        /// <returns></returns>
        public static BusConfigurationBuilder WithSettings(IContainer container) {
            if (configuration == null) {
                lock (lockObject) {
                    if (configuration == null) {
                        configuration = new BusConfiguration(container);
                    }
                }
            }
            var builder = new ContainerBuilder();
            //last one in wins so if one is registered it will be called.
            builder.RegisterType<JsonServiceBusSerializer>().As<IServiceBusSerializer>().SingleInstance();
            return new BusConfigurationBuilder(builder, configuration);
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

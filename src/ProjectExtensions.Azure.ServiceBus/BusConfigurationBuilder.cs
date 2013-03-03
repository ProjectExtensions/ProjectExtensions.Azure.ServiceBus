using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System.Configuration;
using Microsoft.Practices.TransientFaultHandling;
using ProjectExtensions.Azure.ServiceBus.Helpers;
using ProjectExtensions.Azure.ServiceBus.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Builder for the configuration
    /// </summary>
    public class BusConfigurationBuilder {
        BusConfiguration configuration;

        internal BusConfigurationBuilder(BusConfiguration configuration) {
            Guard.ArgumentNotNull(configuration, "configuration");
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the bus configuration associated with the builder
        /// </summary>
        public IBusConfiguration Configuration {
            get { return configuration; }
        }

        /// <summary>
        /// Mark the configuration as complete
        /// </summary>
        public void Configure() {
            Configuration.Configure();
        }

        /// <summary>
        /// Set the ServiceBusApplicationId
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BusConfigurationBuilder ServiceBusApplicationId(string value) {
            Guard.ArgumentNotNullOrEmptyString(value, "value");
            if (value.Length > 10) {
                throw new ArgumentOutOfRangeException("The length must not be greater than 10.");
            }
            configuration.ServiceBusApplicationId = value;
            return this;
        }

        /// <summary>
        /// Set the DefaultSerializer
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public BusConfigurationBuilder DefaultSerializer(IServiceBusSerializer serializer) {
            configuration.Container.Register(typeof(IServiceBusSerializer), serializer.GetType());
            return this;
        }

        /// <summary>
        /// Set the Max Threads that will pull from the bus. (Not Implemented)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BusConfigurationBuilder MaxThreads(byte value) {
            Guard.ArgumentNotZeroOrNegativeValue(value, "value");
            configuration.MaxThreads = value;
            return this;
        }

        /// <summary>
        /// Read the ServiceBus Application and Service Bus Settings from the config file
        /// </summary>
        /// <returns></returns>
        public BusConfigurationBuilder ReadFromConfigFile() {
            var setting = ConfigurationManager.AppSettings["ServiceBusApplicationId"];
            configuration.ServiceBusApplicationId = setting;

            setting = ConfigurationManager.AppSettings["ServiceBusIssuerKey"];
            if (string.IsNullOrWhiteSpace(setting)) {
                throw new ArgumentNullException("ServiceBusIssuerKey", "The ServiceBusIssuerKey must be set.");
            }
            configuration.ServiceBusIssuerKey = setting;

            setting = ConfigurationManager.AppSettings["ServiceBusIssuerName"];
            if (string.IsNullOrWhiteSpace(setting)) {
                throw new ArgumentNullException("ServiceBusIssuerName", "The ServiceBusIssuerName must be set.");
            }
            configuration.ServiceBusIssuerName = setting;

            setting = ConfigurationManager.AppSettings["ServiceBusNamespace"];
            if (string.IsNullOrWhiteSpace(setting)) {
                throw new ArgumentNullException("ServiceBusNamespace", "The ServiceBusNamespace must be set.");
            }
            configuration.ServiceBusNamespace = setting;

            return this;
        }

        /// <summary>
        /// Read From ConfigurationSettings. You would call this instead of calling ReadFromConfigFile
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public BusConfigurationBuilder ReadFromConfigurationSettings(IServiceBusSetupConfiguration settings) {

            //do the three required items first.
            configuration.ServiceBusIssuerKey = settings.ServiceBusIssuerKey;
            if (string.IsNullOrWhiteSpace(configuration.ServiceBusIssuerKey)) {
                throw new ArgumentNullException("ServiceBusIssuerKey", "The ServiceBusIssuerKey must be set.");
            }

            configuration.ServiceBusIssuerName = settings.ServiceBusIssuerName;
            if (string.IsNullOrWhiteSpace(configuration.ServiceBusIssuerName)) {
                throw new ArgumentNullException("ServiceBusIssuerName", "The ServiceBusIssuerName must be set.");
            }

            configuration.ServiceBusNamespace = settings.ServiceBusNamespace;
            if (string.IsNullOrWhiteSpace(configuration.ServiceBusNamespace)) {
                throw new ArgumentNullException("ServiceBusNamespace", "The ServiceBusNamespace must be set.");
            }

            //Now go and do all of the other properties.

            if (settings.DefaultSerializer != null) {
                this.DefaultSerializer(settings.DefaultSerializer);
            }

            if (!string.IsNullOrWhiteSpace(settings.ServiceBusApplicationId)) {
                this.ServiceBusApplicationId(settings.ServiceBusApplicationId);
            }

            if (!string.IsNullOrWhiteSpace(settings.ServicePath)) {
                this.ServicePath(settings.ServicePath);
            }

            if (!string.IsNullOrWhiteSpace(settings.TopicName)) {
                this.TopicName(settings.TopicName);
            }

            foreach (var item in settings.AssembliesToRegister) {
                this.RegisterAssembly(item);
            }

            foreach (var item in settings.TypesToRegister) {
                this.RegisterSubscriber(item);
            }

            return this;
        }

        /// <summary>
        /// Auto discover all of the Subscribers in the assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public BusConfigurationBuilder RegisterAssembly(Assembly assembly) {
            Guard.ArgumentNotNull(assembly, "assembly");
            configuration.AddRegisteredAssembly(assembly);
            return this;
        }

        /// <summary>
        /// Register just one subscriber.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BusConfigurationBuilder RegisterSubscriber<T>() {
            configuration.AddRegisteredSubscriber(typeof(T));
            return this;
        }

        /// <summary>
        /// Register just one subscriber.
        /// </summary>
        /// <param name="t">The type to register.</param>
        /// <returns></returns>
        public BusConfigurationBuilder RegisterSubscriber(Type t) {
            configuration.AddRegisteredSubscriber(t);
            return this;
        }

        /// <summary>
        /// ServiceBusNamespace
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BusConfigurationBuilder ServiceBusNamespace(string value) {
            Guard.ArgumentNotNull(value, "value");
            configuration.ServiceBusNamespace = value;
            return this;
        }

        /// <summary>
        /// ServiceBusIssuerName
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BusConfigurationBuilder ServiceBusIssuerName(string value) {
            Guard.ArgumentNotNull(value, "value");
            configuration.ServiceBusIssuerName = value;
            return this;
        }

        /// <summary>
        /// ServiceBusIssuerKey
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BusConfigurationBuilder ServiceBusIssuerKey(string value) {
            Guard.ArgumentNotNull(value, "value");
            configuration.ServiceBusIssuerKey = value;
            return this;
        }

        /// <summary>
        /// ServicePath
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BusConfigurationBuilder ServicePath(string value) {
            Guard.ArgumentNotNull(value, "value");
            configuration.ServicePath = value;
            return this;
        }

        /// <summary>
        /// Override the default TopicName
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BusConfigurationBuilder TopicName(string value) {
            Guard.ArgumentNotNull(value, "value");
            configuration.TopicName = value;
            return this;
        }

    }
}

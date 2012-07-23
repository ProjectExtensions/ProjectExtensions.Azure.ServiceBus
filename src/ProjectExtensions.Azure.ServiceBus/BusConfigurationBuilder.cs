using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System.Configuration;
using Microsoft.Practices.TransientFaultHandling;
using ProjectExtensions.Azure.ServiceBus.Helpers;

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
            get { return configuration;}
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
        /// Set the Max Threads that will pull from the bus.
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
            var setting = AzureConfigurationHelper.GetConfig("ServiceBusApplicationId");
            configuration.ServiceBusApplicationId = setting;

            setting = AzureConfigurationHelper.GetConfig("ServiceBusIssuerKey");
            if (string.IsNullOrWhiteSpace(setting)) {
                throw new ArgumentNullException("ServiceBusIssuerKey", "The ServiceBusIssuerKey must be set.");
            }
            configuration.ServiceBusIssuerKey = setting;

            setting = AzureConfigurationHelper.GetConfig("ServiceBusIssuerName");
            if (string.IsNullOrWhiteSpace(setting)) {
                throw new ArgumentNullException("ServiceBusIssuerName", "The ServiceBusIssuerName must be set.");
            }
            configuration.ServiceBusIssuerName = setting;

            setting = AzureConfigurationHelper.GetConfig("ServiceBusNamespace");
            if (string.IsNullOrWhiteSpace(setting)) {
                throw new ArgumentNullException("ServiceBusNamespace", "The ServiceBusNamespace must be set.");
            }
            configuration.ServiceBusNamespace = setting;

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

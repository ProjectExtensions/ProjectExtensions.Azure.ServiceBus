using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using NLog;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.TransientFaultHandling.ServiceBus;
using System.Net;
using ProjectExtensions.Azure.ServiceBus.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Base class for the sender and receiver client.
    /// </summary>
    abstract class AzureSenderReceiverBase : IDisposable {
        static Logger logger = LogManager.GetCurrentClassLogger();

        internal static string TYPE_HEADER_NAME = "x_proj_ext_type"; //- are not allowed if you filter.

        protected IBusConfiguration configuration;
        protected IServiceBusConfigurationFactory configurationFactory;

        protected RetryPolicy<ServiceBusTransientErrorDetectionStrategy> retryPolicy
            = new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(30, RetryStrategy.LowMinBackoff, TimeSpan.FromSeconds(5.0), RetryStrategy.LowClientBackoff);
        protected RetryPolicy<ServiceBusTransientErrorToDetermineExistanceDetectionStrategy> verifyRetryPolicy
            = new RetryPolicy<ServiceBusTransientErrorToDetermineExistanceDetectionStrategy>(5, RetryStrategy.LowMinBackoff, TimeSpan.FromSeconds(2.0), RetryStrategy.LowClientBackoff);
        protected TopicDescription topic;

        /// <summary>
        /// Base class used to send and receive messages.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configurationFactory"></param>
        public AzureSenderReceiverBase(IBusConfiguration configuration, IServiceBusConfigurationFactory configurationFactory) {
            Guard.ArgumentNotNull(configuration, "configuration");
            Guard.ArgumentNotNull(configurationFactory, "configurationFactory");
            this.configuration = configuration;
            this.configurationFactory = configurationFactory;
            EnsureTopic(configuration.TopicName);
        }

        protected void EnsureTopic(string topicName) {
            Guard.ArgumentNotNull(topicName, "topicName");
            bool createNew = false;

            try {
                logger.Info("EnsureTopic Try {0} ", topicName);
                // First, let's see if a topic with the specified name already exists.
                topic = verifyRetryPolicy.ExecuteAction<TopicDescription>(() => {
                    return configurationFactory.NamespaceManager.GetTopic(topicName);
                });

                createNew = (topic == null);
            }
            catch (MessagingEntityNotFoundException) {
                logger.Info("EnsureTopic Does Not Exist {0} ", topicName);
                // Looks like the topic does not exist. We should create a new one.
                createNew = true;
            }

            // If a topic with the specified name doesn't exist, it will be auto-created.
            if (createNew) {
                try {
                    logger.Info("EnsureTopic CreateTopic {0} ", topicName);
                    var newTopic = new TopicDescription(topicName);

                    topic = retryPolicy.ExecuteAction<TopicDescription>(() => {
                        return configurationFactory.NamespaceManager.CreateTopic(newTopic);
                    });
                }
                catch (MessagingEntityAlreadyExistsException) {
                    logger.Info("EnsureTopic GetTopic {0} ", topicName);
                    // A topic under the same name was already created by someone else, perhaps by another instance. Let's just use it.
                    topic = retryPolicy.ExecuteAction<TopicDescription>(() => {
                        return configurationFactory.NamespaceManager.GetTopic(topicName);
                    });
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            configurationFactory.MessageFactory.Close();
        }

        public abstract void Dispose(bool disposing);
    }
}

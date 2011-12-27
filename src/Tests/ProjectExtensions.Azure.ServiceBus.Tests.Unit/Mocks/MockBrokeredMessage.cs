using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using Microsoft.Practices.TransientFaultHandling;
using System.IO;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockBrokeredMessage : IBrokeredMessage {

        IDictionary<string, object> properties = new Dictionary<string, object>();
        object body;
        IMockServiceBus serviceBus;

        public MockBrokeredMessage(IMockServiceBus serviceBus) {
            Guard.ArgumentNotNull(serviceBus, "serviceBus");
            this.serviceBus = serviceBus;
        }

        public string ContentType {
            get;
            set;
        }

        public string CorrelationId {
            get;
            set;
        }

        public int DeliveryCount {
            get;
            set;
        }

        public DateTime EnqueuedTimeUtc {
            get;
            set;
        }

        public DateTime ExpiresAtUtc {
            get;
            set;
        }

        public string Label {
            get;
            set;
        }

        public DateTime LockedUntilUtc {
            get;
            set;
        }

        public Guid LockToken {
            get;
            set;
        }

        public string MessageId {
            get;
            set;
        }

        public IDictionary<string, object> Properties {
            get {
                return properties;
            }
        }

        public string ReplyTo {
            get;
            set;
        }

        public string ReplyToSessionId {
            get;
            set;
        }

        public DateTime ScheduledEnqueueTimeUtc {
            get;
            set;
        }

        public long SequenceNumber {
            get;
            set;
        }

        public string SessionId {
            get;
            set;
        }

        public long Size {
            get;
            set;
        }

        public TimeSpan TimeToLive {
            get;
            set;
        }

        public string To {
            get;
            set;
        }

        public void Abandon() {
            serviceBus.MessageAbandon();
        }

        public void Complete() {
            serviceBus.MessageComplete();
        }

        public void DeadLetter(string deadLetterReason, string deadLetterErrorDescription) {
            serviceBus.MessageDeadLetter(deadLetterReason, deadLetterErrorDescription);
        }

        public void Dispose() {
            if (body != null && body is IDisposable) {
                (body as IDisposable).Dispose();
                body = null;
            }
        }

        public T GetBody<T>() {
            return (T)body;
        }

        public void SetBody(Stream stream) {
            body = stream;
        }
    }
}

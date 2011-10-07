using System;
namespace ProjectExtensions.Azure.ServiceBus {

    public interface IMessageSentResult<T> {
        bool IsSuccess {
            get;
            set;
        }
        Exception ThrownException {
            get;
            set;
        }
        TimeSpan TimeSpent {
            get;
            set;
        }
        object State {
            get;
            set;
        }
    }
}

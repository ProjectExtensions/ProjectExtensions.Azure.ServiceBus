=============================================
ProjectExtensions.Azure.ServiceBus
=============================================
Use ClickToBuild.bat to build.

==Nuget==
The Nuget package is ProjectExtensions.Azure.ServiceBus

==Getting Started==

1. Create a console application
2. Add a reference to ProjectExtensions.Azure.ServiceBus
    Using NuGet, install the package ProjectExtensions.Azure.ServiceBus
3. Optionally Add a reference to NLog
4. Create a Message Class that you wish to handle:

public class TestMessage {
  
    public string MessageId {
        get;
        set;
    }

    public int Value {
        get;
        set;
    }
}

5. Create a Handler that will receive notifications when the message is placed on the bus:

public class TestMessageSubscriber : IHandleMessages<TestMessage> {

    static Logger logger = LogManager.GetCurrentClassLogger();

    public void Handle(IReceivedMessage<TestMessage> message) {
        logger.Log(LogLevel.Info, "Message received: {0} {1}", message.Message.Value, message.Message.MessageId);
    }
}


6. Place this at the beginning of your method or in your BootStrapper

If you are going to use a config file, then set these properties

<add key="ServiceBusIssuerKey" value="base64hash" />
<add key="ServiceBusIssuerName" value="owner" />
//https://addresshere.servicebus.windows.net/
<add key="ServiceBusNamespace" value="namespace set up in service bus (addresshere) portion" />

ProjectExtensions.Azure.ServiceBus.BusConfiguration.WithSettings()
    .ReadFromConfigFile()
    .ServiceBusApplicationId("AppName")
    .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
    .Configure();

Otherwise, you can configure everything in code:

ProjectExtensions.Azure.ServiceBus.BusConfiguration.WithSettings()
    .ServiceBusApplicationId("AppName")
    .ServiceBusIssuerKey("[sb password]")
    .ServiceBusIssuerName("owner")
    .ServiceBusNamespace("[addresshere]")
    .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
    .Configure();

7. Put some messages on the Bus:

for (int i = 0; i < 20; i++) {
    var message1 = new TestMessage() {
        Value = i,
        MessageId = DateTime.Now.ToString()
    };
    BusConfiguration.Instance.Bus.Publish(message1, null);
}

Watch your method get called.

Welcome to Azure Service Bus.

@joefeser
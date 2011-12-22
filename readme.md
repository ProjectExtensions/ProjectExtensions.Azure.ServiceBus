#ProjectExtensions.Azure.ServiceBus

An easier way to work with the Azure service bus.

Follow me or tweet at me on Twitter: @joefeser.

##Building 

Use ClickToBuild.bat to build.

##Getting started

The Nuget package is ProjectExtensions.Azure.ServiceBus

1. Create a console application
2. Using NuGet, install the package ProjectExtensions.Azure.ServiceBus.
3. Optionally Add a reference to NLog
4. Create a Message Class that you wish to handle:

```csharp
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
```

5\. Create a Handler that will receive notifications when the message is placed on the bus:

```csharp
public class TestMessageSubscriber : IHandleMessages<TestMessage> {

    static Logger logger = LogManager.GetCurrentClassLogger();

    public void Handle(IReceivedMessage<TestMessage> message) {
        logger.Log(LogLevel.Info, "Message received: {0} {1}", message.Message.Value, message.Message.MessageId);
    }
}
```


6\. Place initialization code at the beginning of your method or in your BootStrapper.  You will need a couple of using declarations:

```csharp
using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.Autofac.Container;
```

Basic setup code (assuming you want to put Azure configuration information in your application configuration file):

```csharp
ProjectExtensions.Azure.ServiceBus.BusConfiguration.WithSettings()
    .UseAutofacContainer()
    .ReadFromConfigFile()
    .ServiceBusApplicationId("AppName")
    .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
    .Configure();
```

And configuration:

```xml
<add key="ServiceBusIssuerKey" value="base64hash" />
<add key="ServiceBusIssuerName" value="owner" />
//https://addresshere.servicebus.windows.net/
<add key="ServiceBusNamespace" value="namespace set up in service bus (addresshere) portion" />
```

Otherwise, you can configure everything in code:

```csharp
ProjectExtensions.Azure.ServiceBus.BusConfiguration.WithSettings()
	.UseAutofacContainer()
    .ServiceBusApplicationId("AppName")
    .ServiceBusIssuerKey("[sb password]")
    .ServiceBusIssuerName("owner")
    .ServiceBusNamespace("[addresshere]")
    .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
    .Configure();
```

7\. Put some messages on the Bus:

```csharp
for (int i = 0; i < 20; i++) {
    var message1 = new TestMessage() {
        Value = i,
        MessageId = DateTime.Now.ToString()
    };
    BusConfiguration.Instance.Bus.Publish(message1, null);
}
```

Watch your method get called.

Welcome to Azure Service Bus.

##Release Notes

###Coming Soon

* Support for Castle Windsor IOC container

###Version 0.8.4

* Allow support for other IOC containers to be added
* BREAKING CHANGE.  Move Autofac support into seperate DLL.  Existing implementations need to add a reference to ProjectExtensions.Azure.ServiceBus.Autofac and change initialization code as shown in the getting started example.
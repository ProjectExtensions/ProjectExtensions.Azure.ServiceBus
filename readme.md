# ProjectExtensions.Azure.ServiceBus #

An easier way to work with the Azure service bus.

Follow me or tweet at me on Twitter: @joefeser.

## Building ##

Use ClickToBuild.bat to build.  You will need to have the Azure SDK 1.5 installed in order to build.

## Nuget Packages ##

If you don't use an IoC container in your application or you are happy to use Autofac, download the default Nuget package:

* ProjectExtensions.Azure.ServiceBus

If you want to use a specific IoC container, grab our core package:

* ProjectExtensions.Azure.ServiceBus.Core

You can then either implement IAzureBusContainer for your IoC of choice or grab one of the pre-built options:

* Autofac in ProjectExtensions.Azure.ServiceBus.IOC.Autofac
* Castle Windsor in ProjectExtensions.Azure.ServiceBus.IOC.CastleWindsor
* Ninject in ProjectExtensions.Azure.ServiceBus.IOC.Ninject
* StructureMap in ProjectExtensions.Azure.ServiceBus.IOC.StructureMap
* Unity in ProjectExtensions.Azure.ServiceBus.IOC.Unity

If you have a favorite IoC container we don't support, let us know, or, better yet, contribute an implementation.

## Setting up a Windows Azure Service Bus ##

You may find step be step instructions [here](https://github.com/ProjectExtensions/ProjectExtensions.Azure.ServiceBus/wiki/Setting-Up-Windows-Azure-Service-Bus) on how to create the namespace needed to send messages.

## Getting started ##

Read more in the Wiki [here](https://github.com/ProjectExtensions/ProjectExtensions.Azure.ServiceBus/wiki/)

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
5\. Create a Handler that will receive notifications when the message is placed on the bus. The custom attribute is optional but configures the Service Bus Subscription:

Note: If your constructor for your message takes in parameters from your DI container that implement IDisposable, you must set Singleton = true or you will leak memory.

Please see [this](https://github.com/ProjectExtensions/ProjectExtensions.Azure.ServiceBus/wiki/Configuration-%22MessageHandlerConfiguration%22) for more information on the MessageHandlerConfiguration custom attribute
```csharp
[MessageHandlerConfiguration(
    DefaultMessageTimeToLive = 240, //Time in minutes before your message is deleted from the subscription if you don't receive it.
    LockDuration = 120, //Time that you wish to lock the message before it is marked to be received by another subscriber.
    MaxConcurrentCalls = 4, //Tells the library to spin up as many as 4 instances of your receiver at a time.
    MaxRetries = 2, //Number of times to retry calling your handler before the message is deleted or placed in the DeadLetterAfterMaxRetries if configured.
    PrefetchCount = 10, //Number of messages to pre-fetch. Used for high throughput 
    ReceiveMode = ReceiveMode.PeekLock, //PeekLock or Receive and Delete
    Singleton = true)] //If it is a singleton, the instance will be created once, otherwise it will be created for each message received. Recommended to set to true.
public class TestMessageSubscriber : IHandleMessages<TestMessage> {

    static Logger logger = LogManager.GetCurrentClassLogger();

    //You may optionally create a constructor that takes in parameters that can be resolved from your DI container.

    public void Handle(IReceivedMessage<TestMessage> message) {
        logger.Log(LogLevel.Info, "Message received: {0} {1}", message.Message.Value, message.Message.MessageId);
        //If you can't deal with the message and you would like to be called back, throw an exception.
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
    .ServiceBusApplicationId("AppName") //Multiple applications can be used in the same service bus namespace. It is converted to lower case.
    .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
    .Configure();
```

And configuration:

```xml
<add key="ServiceBusIssuerKey" value="base64hash" />
<add key="ServiceBusIssuerName" value="owner" />
<!--https://addresshere.servicebus.windows.net/-->
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
//Blocking (Synchronous calls)
for (int i = 0; i < 20; i++) {
    var message1 = new TestMessage() {
        Value = i,
        MessageId = DateTime.Now.ToString()
    };
    BusConfiguration.Instance.Bus.Publish(message1, null); //Optional Dictionary of name value pairs to pass with the massage. Can be used for filtering
}

//Async calls
for (int i = 0; i < 20; i++) {
    var message1 = new TestMessage() {
        Value = i,
        MessageId = DateTime.Now.ToString()
    };
    BusConfiguration.Instance.Bus.PublishAsync(message2, (result) => {
        if (!result.IsSuccess) { 
            //message failed. Handle it
        }
        Debug.WriteLine("async:" + result.TimeSpent);
    }, null); //Optional Dictionary of name value pairs to pass with the massage. Can be used for filtering
}

```

Watch your method get called.

Welcome to Azure Service Bus.

## Using an IoC Conatiner Other Than Autofac ##

Unless otherwise noted, everything works as shown in the getting starting section above.  This section outlines the things you will need to do differently.

1. Install the Nuget packages ProjectExtensions.Azure.ServiceBus.Core and your specific IoC container (e.g. ProjectExtensions.Azure.ServiceBus.IOC.CastleWindsor for Castle Windsor) 
instead of ProjectExtensions.Azure.ServiceBus
2. Use the following initialization code at the beginning of your method or in your BootStrapper.  _The code examples shown below are for Castle Windsor.  Code for other containers follows the same pattern._

You will need a couple of using declarations:

```csharp
using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.CastleWindsor.Container;
```
Basic setup code (assuming you want to put Azure configuration information in your application configuration file):

```csharp
ProjectExtensions.Azure.ServiceBus.BusConfiguration.WithSettings()
    .UseCastleWindsorContainer()
    .ReadFromConfigFile()
    .ServiceBusApplicationId("AppName")
    .EnablePartitioning(true)
    .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
    .Configure();
```

Otherwise, you can configure everything in code:

```csharp
ProjectExtensions.Azure.ServiceBus.BusConfiguration.WithSettings()
    .UseCastleWindsorContainer()
    .ServiceBusApplicationId("AppName")
    .ServiceBusIssuerKey("[sb password]")
    .ServiceBusIssuerName("owner")
    .ServiceBusNamespace("[addresshere]")
    .EnablePartitioning(true)
    .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
    .Configure();
```
Or if you prefer to configure everything and pass it in all at once you can use this method:
```csharp

//You can easily read your settings from Azure or a database and then pass them in.
//The setup class can be configured anywhere. 
//If you do not like the default implementation, just implement the interface.

var setup = new ServiceBusSetupConfiguration() {
    DefaultSerializer = new GZipXmlSerializer(),
    ServiceBusIssuerKey = ConfigurationManager.AppSettings["ServiceBusIssuerKey"],
    ServiceBusIssuerName = ConfigurationManager.AppSettings["ServiceBusIssuerName"],
    ServiceBusNamespace = ConfigurationManager.AppSettings["ServiceBusNamespace"],
    ServiceBusApplicationId = "AppName"
};

setup.AssembliesToRegister.Add(typeof(TestMessageSubscriber).Assembly);

BusConfiguration.WithSettings()
    .UseAutofacContainer()
    .ReadFromConfigurationSettings(setup)
    .Configure();
```
You may also download the repository and check out the Samples in the /src/samples folder.

The Sample used to build this document can be found in the PubSubUsingConfiguration example.

Click on the "Zip" Icon at the top of the page to download the latest source code.

## Release Notes ##

### Version 0.9.0 ###

* Allow support for other IoC containers to be added. Continue to support Autofac.
* Support for Castle Windsor IoC.
* Support for Ninject IoC.
* Support for StructureMap IoC.
* Support for Unity IoC.
* BREAKING CHANGE. Move Autofac support into seperate DLL. Existing implementations need to add a reference to ProjectExtensions.Azure.ServiceBus.Autofac and change initialization code as shown in the getting started example.
* BREAKING CHANGE. WithSettings No longer accepts the AutoFac Container as a parameter. This change was made to support the other containers.
* BREAKING CHANGE. You must add .UseAutofacContainer() after WithSettings(). If you wich to use your existing container, You would pass it into this method call.

### Version 0.9.1 ###

* Fixed bug in AutoFac registration of a Default Serializer.
* Fixed bug in AutoFac registration of a any items internally registered on the default container.
* Fixed bug in Publish method that ignored the serializer passed in and defaulted back to default serializer.

### Version 0.9.2 ###

* Added self healing of deleted topic during application execution. Error is still thrown since no subscribers will exist.
* Added self healing of deleted subscriptions during application execution. Any messages sent to the topic while your client subscription is deleted will not be received. The sender does not understand how many receivers exist and therefor does not know that the message needs to be resent.

### Version 0.9.3 ###

* Added the ability to pass in a Settings Provider instead of reading from the app/web.config file.


###Version 0.9.4 ###

* Updated references and set the Service Bus Requirement to v1.8. This is the last release that supports the v1.0 Service Bus Assembly.

###Version 0.10.0 ###

* Updated to use ServiceBus v2.2.2

###Version 0.10.1 ###

* Added EnablePartitioning Support to the Fluent Configuration.

###Version 0.10.2 ###

* Added MaxConcurrentCalls Support. This will spin up multiple instances of your receiver, increasing performance.
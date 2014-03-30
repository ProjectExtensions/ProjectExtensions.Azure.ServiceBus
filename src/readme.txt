Thanks for downloading this Windows Azure Service Bus Message Wrapper Project.



**Breaking change if you are upgrading from a version < 0.9.0 **

If you are upgrading from a version that is < 0.9.0 and You are using ProjectExtensions.Azure.ServiceBus there are two References now. 
If your project does not compile, make sure you have a reference to both ProjectExtensions.Azure.ServiceBus.dll and ProjectExtensions.Azure.ServiceBus.
Nuget will update these references for you but if you have a project you are manually editing, you must add both references.

Note: You must add this namespace to the files that perform the configuration.

using ProjectExtensions.Azure.ServiceBus.Autofac.Container;

WithSettings no longer takes an overload

You must add .UseAutofacContainer() passing in the optional container that used to be passed into WithSettings. This may also be left blank.



**Information**

Getting started can be found here

https://github.com/ProjectExtensions/ProjectExtensions.Azure.ServiceBus

And our Wiki is here:

https://github.com/ProjectExtensions/ProjectExtensions.Azure.ServiceBus/wiki/

You may also download the repository and check out the Samples in the /src/samples folder.

The Sample used to build this document can be found in the PubSubUsingConfiguration example.

https://github.com/ProjectExtensions/ProjectExtensions.Azure.ServiceBus/archive/master.zip

The ServiceBusNamespace Is the namespace that you configure in the Azure Portal.

You can read about how to set up the Windows Azure Namespace here:

https://github.com/ProjectExtensions/ProjectExtensions.Azure.ServiceBus/wiki/Setting-Up-Windows-Azure-Service-Bus

It is the test1234 portion of sb://test1234.servicebus.windows.net

Once you create the namespace, if you click on the "Access Key" button on the bottom of the screen, the owner and sb password will be provided

BusConfiguration.WithSettings()
    .UseAutofacContainer()
    .ReadFromConfigFile()
    .ServiceBusApplicationId("AppName") //Multiple applications can be used in the same service bus namespace. It is converted to lower case.
    .DefaultSerializer(new GZipXmlSerializer())
    //.ServiceBusIssuerKey("[sb password]")
    //.ServiceBusIssuerName("owner")
    //.ServiceBusNamespace("[addresshere]")
    .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
    .Configure();

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


**Warning if you are using IDisposable objects as parameters to your Handlers**

We do not support Releasing of Transient objects from your container. This may cause a memory leak. 

Set this property using this custom attribute on your message handler.

For a complete set of options, please see the github page.

[MessageHandlerConfiguration(
    LockDuration = 120, //Gets or sets the lock duration time span for the subscription. (in seconds)
    MaxConcurrentCalls = 4, //Gets or sets the maximum number of concurrent calls to the callback the message pump should initiate.
    MaxRetries = 2, //Gets or sets the number of maximum calls to your handler.
    PrefetchCount = 20, //Gets or sets the number of messages that the message receiver can simultaneously request.
    ReceiveMode = ReceiveMode.PeekLock, //This mode receives the message but keeps it peek-locked until the receiver abandons the message.
    Singleton=true)] //If it is a singleton, the instance will be created once, otherwise it will be created for each message received. Recommended to set to true.

**New Features**

We now have support for addtional containers. If you wish to use AutoFac, Castle Windsor, NInject, StructureMap or Unity, 
You will want to Install the Package ProjectExtensions.Azure.ServiceBus.Core along with the correct container.
Added MaxConcurrentCalls Support. This will spin up multiple instances of your receiver, increasing performance.


**Release Notes**

###Version 0.9.0

* Allow support for other IoC containers to be added. Continue to support Autofac.
* Support for Castle Windsor IoC.
* Support for Ninject IoC.
* Support for StructureMap IoC.
* Support for Unity IoC.
* BREAKING CHANGE. Move Autofac support into seperate DLL. Existing implementations need to add a reference to ProjectExtensions.Azure.ServiceBus.Autofac and change initialization code as shown in the getting started example.
* BREAKING CHANGE. WithSettings No longer accepts the AutoFac Container as a parameter. This change was made to support the other containers.
* BREAKING CHANGE. You must add .UseAutofacContainer() after WithSettings(). If you wich to use your existing container, You would pass it into this method call.

###Version 0.9.1

* Fixed bug in AutoFac registration of a Default Serializer.
* Fixed bug in AutoFac registration of a any items internally registered on the default container.
* Fixed bug in Publish method that ignored the serializer passed in and defaulted back to default serializer.

###Version 0.9.2

* Added self healing of deleted topic during application execution. Error is still thrown since no subscribers will exist.
* Added self healing of deleted subscriptions during application execution. Any messages sent to the topic while your client subscription is deleted will not be received. The sender does not understand how many receivers exist and therefor does not know that the message needs to be resent.

###Version 0.9.3

* Added the ability to pass in a Settings Provider instead of reading from the app/web.config file.

###Version 0.9.4

* Updated references and set the Service Bus Requirement to v1.8. This is the last release that supports the v1.0 Service Bus Assembly.

###Version 0.10.0

* Updated to use ServiceBus v2.2.2

###Version 0.10.1

* Added EnablePartitioning Support to the Fluent Configuration.

###Version 0.10.2

* Added MaxConcurrentCalls Support. This will spin up multiple instances of your receiver, increasing performance.

###Version 0.10.3

* Fixed a bug on how we reset the connection when a bad error happens. We now correctly pause when PauseTimeIfErrorWasThrown is set

###Version 0.10.4 ###

* Added the ability to return the number of messages for a Topic (subscription) by passing in the type of the receiver
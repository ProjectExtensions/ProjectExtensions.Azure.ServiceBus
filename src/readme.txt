Thanks for downloading this Windows Azure Service Bus Message Wrapper Project.

*****Breaking change if you are upgrading from a version < 0.9.0 *****

If you are upgrading from a version that is < 0.9.0 and You are using ProjectExtensions.Azure.ServiceBus there are two References now. 
If your project does not compile, make sure you have a reference to both ProjectExtensions.Azure.ServiceBus.dll and ProjectExtensions.Azure.ServiceBus.
Nuget will update these references for you but if you have a project you are manually editing, you must add both references.

Note You must add this namespace to the files that perform the configuration.

using ProjectExtensions.Azure.ServiceBus.Autofac.Container;

WithSettings no longer takes an overload

You must add .UseAutofacContainer() passing in the optional container that used to be passed into WithSettings. This may also be left blank.

The ServiceBusNamespace Is the namespace that you configure in the Azure Portal.

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

**New Features**

We now have support for addtional containers. If you wish to use AutoFac, Castle Windsor, NInject, StructureMap or Unity, 
You will want to Install the Package ProjectExtensions.Azure.ServiceBus.Core along with the correct container.

Getting started can be found here (readme.md)

https://github.com/ProjectExtensions/ProjectExtensions.Azure.ServiceBus
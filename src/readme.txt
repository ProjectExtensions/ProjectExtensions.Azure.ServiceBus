Thanks for downloading this Windows Azure Service Bus Message Wrapper Project.

If you are upgrading from a version that is < 0.9.0 and You are using ProjectExtensions.Azure.ServiceBus there are two References now. 
If your project does not compile, make sure you have a reference to both ProjectExtensions.Azure.ServiceBus.dll and ProjectExtensions.Azure.ServiceBus.

We now have support for addtional containers. If you wish to use AutoFac, Castle Windsor, NInject, StructureMap or Unity, 
You will want to Install the Package ProjectExtensions.Azure.ServiceBus.Core along with the correct container.
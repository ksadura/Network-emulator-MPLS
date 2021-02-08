# Network emulator - MPLS
This application has been created as a part of TSST course.

This project aimed at showing how Multiprotocol Label Switching Architecture technique works. 
We put much emphasis on the vital aspect of MPLS i.e. label:swapping, pushing and popping. 
Application is written in C# language with the use of asynchronous and synchronous sockets from *System.Net.Sockets* library. 
 

| ![Topology](./Resources/tp.png) |
|:--:|
|*Network topology*|

**To run application use ```run.bat```**  

Application consists of:
- [x] Cable cloud for packet forwarding
- [x] Management system for management and configuration with GUI
- [ ] Nodes (routers)
- [ ] Hosts

*I was responsible for these two modules checked above*

References:
* [link1](https://docs.microsoft.com/pl-pl/dotnet/framework/network-programming/asynchronous-client-socket-example)
* [link2](https://tools.ietf.org/html/rfc3031)
 







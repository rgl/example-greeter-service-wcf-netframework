# Example WCF service and client

The service is at [GreeterService/Program.cs](GreeterService/Program.cs) and listens on the addresses:

* http://localhost:8000/example/greeter (Metadata)
* http://localhost:8000/example/greeter/mex (Metadata)
* http://localhost:8000/example/greeter/web (Web)
* http://localhost:8000/example/greeter/soap (SOAP)
* net.pipe://localhost/8000/example/greeter/pipe (Named Pipe)

The endpoints can be accessed as:

* `curl http://localhost:8000/example/greeter?singleWsdl`
* `curl http://localhost:8000/example/greeter/web/greet?name=Rui%20Lopes`
* `curl --header 'Content-Type: text/xml' --header 'SOAPAction: "http://example.net/IGreeter/Greet"' --data '<Envelope xmlns="http://schemas.xmlsoap.org/soap/envelope/"><Body><Greet xmlns="http://example.net"><name>Rui Lopes</name></Greet></Body></Envelope>' http://localhost:8000/example/greeter/soap`
* Named Pipe with [GreeterPipeClient/Program.cs](GreeterPipeClient/Program.cs)

## Permissions

To be able to run GreeterService as a normal user account you need to grant it privileges with:

```powershell
netsh http add urlacl url=http://+:8000/example/greeter/ user=alice
netsh http show urlacl
```

If you do not do this, the following error is raised:

```plain
Unhandled Exception: System.ServiceModel.AddressAccessDeniedException: HTTP could not register URL http://+:8000/example/greeter/. Your process does not have access rights to this namespace (see http://go.microsoft.com/fwlink/?LinkId=70353 for details). ---> System.Net.HttpListenerException: Access is denied
```

## Named Pipes

See:

* https://blogs.msdn.microsoft.com/rodneyviana/2011/03/22/named-pipes-in-wcf-are-named-but-not-by-you-and-how-to-find-the-actual-windows-object-name/
* [PipeConnectionListener.Listen](https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/System.ServiceModel/System/ServiceModel/Channels/PipeConnection.cs#L2153)
* [PipeConnectionListener.CreatePipe](https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/System.ServiceModel/System/ServiceModel/Channels/PipeConnection.cs#L2048)
* [PipeUri.BuildSharedMemoryName](https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/System.ServiceModel/System/ServiceModel/Channels/PipeConnection.cs#L2753)

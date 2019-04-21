using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Threading;

namespace Example
{
    [ServiceContract(Namespace = "http://example.net")]
    public interface IGreeter
    {
        [OperationContract]
        [WebGet]
        string Greet(string name);
    }

    public class GreeterService : IGreeter
    {
        public string Greet(string name)
        {
            return $"Hello {name}!";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--wait-for-debugger"))
            {
                Console.WriteLine("Waiting for debugger...");
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(500);
                }
                Console.WriteLine("Debugger attached!");
                Debugger.Break();
            }

            var endpoints = new[] { "soap", "web", "pipe" };

            for (var i = 0; i < args.Length; ++i)
            {
                var name = args[i];

                if (name == "--endpoints")
                {
                    endpoints = args[i + 1].Split(',').ToArray();
                }
            }

            var quitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                quitEvent.Set();
            };

            var baseAddresses = new List<Uri>();

            if (endpoints.Contains("soap") || endpoints.Contains("web"))
            {
                baseAddresses.Add(new Uri("http://localhost:8000/example/greeter"));
            }

            if (endpoints.Contains("pipe"))
            {
                // NB net.pipe cannot contain a port number; so this emulates it in the first segment.
                // NB net.pipe shares the namespace with all other applications running on the machine,
                //    so make sure the segments are unique (like done here with the 8000 segment).
                //    see https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/System.ServiceModel/System/ServiceModel/Channels/PipeConnection.cs#L1680
                //    see https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/System.ServiceModel/System/ServiceModel/Channels/PipeConnection.cs#L2753
                // NB under the cover wcf will create a global memory-mapped file, which means
                //    the process user/account must have the SeCreateGlobalPrivilege privilege.
                baseAddresses.Add(new Uri("net.pipe://localhost/8000/example/greeter"));
            }                

            var host = new ServiceHost(typeof(GreeterService), baseAddresses.ToArray());

            if (endpoints.Contains("soap"))
            {
                // e.g. curl http://localhost:8000/example/greeter?singleWsdl
                var smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);
                var mexEndpoint = host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

                // e.g. curl --header 'Content-Type: text/xml' --header 'SOAPAction: "http://example.net/IGreeter/Greet"' --data '<Envelope xmlns="http://schemas.xmlsoap.org/soap/envelope/"><Body><Greet xmlns="http://example.net"><name>Rui Lopes</name></Greet></Body></Envelope>' http://localhost:8000/example/greeter/soap
                var soapEndpoint = host.AddServiceEndpoint(typeof(IGreeter), new BasicHttpBinding(), "soap");
            }

            if (endpoints.Contains("web"))
            {
                // e.g. curl http://localhost:8000/example/greeter/web/greet?name=Rui%20Lopes
                var webEndpoint = host.AddServiceEndpoint(typeof(IGreeter), new WebHttpBinding(), "web");
                webEndpoint.Behaviors.Add(new WebHttpBehavior());
            }

            if (endpoints.Contains("pipe"))
            {
                // see the GreeterPipeClient project.
                // see https://blogs.msdn.microsoft.com/rodneyviana/2011/03/22/named-pipes-in-wcf-are-named-but-not-by-you-and-how-to-find-the-actual-windows-object-name/
                var pipeEndpoint = host.AddServiceEndpoint(typeof(IGreeter), new NetNamedPipeBinding(), "pipe");
            }

            host.Open();

            foreach (var endpoint in host.Description.Endpoints)
            {
                Console.WriteLine($"Listening at {endpoint.ListenUri}");
            }

            Console.WriteLine("Press Ctrl+C to quit");
            quitEvent.WaitOne();

            host.Close();
            Console.WriteLine("Bye bye!");
        }
    }
}

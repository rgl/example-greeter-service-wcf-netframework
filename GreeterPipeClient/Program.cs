using System;
using System.ServiceModel;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var pipeFactory = new ChannelFactory<IGreeter>(
                new NetNamedPipeBinding(),
                new EndpointAddress("net.pipe://localhost/8000/example/greeter/pipe")
            );

            pipeFactory.Open();

            var greeter = pipeFactory.CreateChannel();

            Console.WriteLine($"Greeting: {greeter.Greet("Rui Lopes")}");

            pipeFactory.Close();
        }
    }
}

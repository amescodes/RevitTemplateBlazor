using System;
using Grpc.Core;
using GrpcRevitRunner;
using Newtonsoft.Json;
using RevitTemplateWeb.Core;

namespace gRPC.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:7287", ChannelCredentials.Insecure);

            var client = new RevitRunner.RevitRunnerClient(channel);

            string testMessage = "howdy pardner!";
            bool testBool = false;
            object[] inputArray = new object[] { testMessage, testBool };
            string inputAsJsonString = JsonConvert.SerializeObject(inputArray);
            var reply = client.RunCommand(new CommandRequest() { CommandEnum = Commands.ShowTaskDialog.ToString(), CommandInputJson = inputAsJsonString});
            Console.WriteLine("Greeting: " + reply.CommandOutputJson);

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();        }
    }
}

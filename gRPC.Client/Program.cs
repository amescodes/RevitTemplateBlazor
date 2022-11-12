using System;

using RevitTemplateWeb.Core;

using Synapse;

using Newtonsoft.Json;

namespace gRPC.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // port number is passed here in the command line argument
            int port = int.Parse(args[0]);

            SynapseClient synapseClient = SynapseClient.StartSynapseClient(port);

            string testMessage = "howdy pardner!";
            bool testBool = false;
            object[] inputArray = new object[] { testMessage, testBool };
            string inputAsJsonString = JsonConvert.SerializeObject(inputArray);

            //! synapse revit command!
            var reply = synapseClient.DoRevit(new SynapseRequest() { MethodId = (int)Commands.ShowTaskDialogTest, MethodInputJson = inputAsJsonString });

            Console.WriteLine("Greeting: " + reply.MethodOutputJson);

            synapseClient.Shutdown();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

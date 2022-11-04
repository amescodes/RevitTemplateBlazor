using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Grpc.Core;
using GrpcGreeter;
using RevitTemplateWeb.Services;

namespace RevitTemplateWeb
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenUIExtCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //TaskDialog.Show("tada!", "bada bing bada boom");

            //// if the browser window/process is already open, activate it instead of opening a new process 
            //if (MessageHandler.browser_pid != 0)
            //{
            //    // the following line could be enough, but rather activate the window thru the process
            //    //SetForegroundWindow(MessageHandler.browser_hwnd);

            //    // try to activate the existing instance
            //    Process[] processes = Process.GetProcesses();
                
            //    foreach (Process p in processes)
            //    {
            //        if (p.Id == MessageHandler.browser_pid)
            //        {
            //            IntPtr windowHandle = p.MainWindowHandle;
            //            SetForegroundWindow(windowHandle);
            //            return Result.Succeeded;
            //        }
            //    }
            //}

            // execute the browser window process
            Process process = new Process();
            process.StartInfo.FileName = "TestgRPC.Client.exe";
            //process.StartInfo.Arguments = ApplicationClass.messagehandler.Handle.ToString(); // pass the MessageHandler's window handle the the process as a command line argument
            process.Start();

            //MessageHandler.browser_pid = process.Id; // grab the PID so we can kill the process if required;
            
            Server server = new Server
            {
                Services = { Greeter.BindService(new GreeterService(null)) },
                Ports = { new ServerPort("localhost", 7287, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + 7287);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        
            return Result.Succeeded;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
            
            Server server = new Server
            {
                Services = { Greeter.BindService(new GreeterService(null)) },
                Ports = { new ServerPort("localhost", 7287, ServerCredentials.Insecure) }
            };
            server.Start();

            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyDllPath = assembly.Location.Replace($"{assembly.ManifestModule.Name}","UI");
//#if RELEASE
//            string uiProcessPath = Path.Combine(Assembly.GetAssembly(typeof(OpenUIExtCommand)).Location, "UI\\RevitTemplateWeb.UI.exe");
//#else
//            string uiProcessPath = "C:\\Users\\amesh\\source\\repos\\RevitTemplate-amescodes\\RevitTemplateWeb.UI\\bin\\Debug\\net6.0\\RevitTemplateWeb.UI.exe";
//#endif

            string uiProcessPath = Path.Combine(assemblyDllPath,"gRPC.Client.exe");
            // execute the browser window process
            Process process = new Process();
            process.StartInfo.FileName = uiProcessPath;
            //process.StartInfo.Arguments = ApplicationClass.messagehandler.Handle.ToString(); // pass the MessageHandler's window handle the the process as a command line argument
            process.Start();

            //MessageHandler.browser_pid = process.Id; // grab the PID so we can kill the process if required;

            //server.ShutdownAsync().Wait();
        
            return Result.Succeeded;
        }
    }
}

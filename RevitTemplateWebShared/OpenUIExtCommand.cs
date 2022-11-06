using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Grpc.Core;
using GrpcRevitRunner;
using Newtonsoft.Json.Linq;
using RevitTemplateWeb.Core;
using RevitTemplateWeb.Services;
using RevitTemplateWebShared.Services;

namespace RevitTemplateWeb
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenUIExtCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Revit.Async.RevitTask.Initialize(commandData.Application);

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

            Assembly assembly = Assembly.GetExecutingAssembly();

            // make command runner dictionary for translating grpc messages to revit actions
            RevitCommandRunnerService revitCommandRunnerService = new RevitCommandRunnerService(new RevitCommandRunner());
            Type[] exportedTypes = assembly.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                MethodInfo[] methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (MethodInfo method in methods)
                {
                    if (method.GetCustomAttribute<RevitCommandAttribute>() is not RevitCommandAttribute revitCommandAttribute)
                    {
                        continue;
                    }

                    revitCommandRunnerService.RevitRunnerCommandDictionary.Add(revitCommandAttribute.CommandToRun, method);
                }
            }

            // start grpc server
            Server server = new Server
            {
                Services = { RevitRunner.BindService(revitCommandRunnerService) },
                Ports = { new ServerPort("localhost", 7287, ServerCredentials.Insecure) }
            };
            server.Start();

            string assemblyDllPath = assembly.Location.Replace($"{assembly.ManifestModule.Name}", "UI");
            string uiProcessPath = Path.Combine(assemblyDllPath, "gRPC.Client.exe");

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

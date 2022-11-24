using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using RevitTemplateWebShared.Services;

using Synapse.Revit;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTemplateWeb
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenUIExtCommand : IExternalCommand
    {
        private static SynapseProcess process;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Revit.Async.RevitTask.Initialize(commandData.Application);
            SynapseRevitService.Initialize();

            if (process != null &&
                process.IsOpen())
            {
                process.ActivateProcess();
                return Result.Succeeded;
            }

            // make command runner dictionary for translating grpc messages to revit actions
            process = SynapseRevitService.RegisterSynapse(new RevitTemplateWebSynapse(commandData.Application));
            process.Start();

            commandData.Application.ApplicationClosing += Application_ApplicationClosing;

            return Result.Succeeded;
        }

        private void Application_ApplicationClosing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            process?.Close();

            UIControlledApplication uiapp = sender as UIControlledApplication;
            if (uiapp == null)
            {
                return;
            }

            uiapp.ApplicationClosing -= Application_ApplicationClosing;
        }
    }
}

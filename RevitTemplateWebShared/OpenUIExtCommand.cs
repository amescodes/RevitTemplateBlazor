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
        private SynapseRevitService synapseRevitService;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Revit.Async.RevitTask.Initialize(commandData.Application);

            // make command runner dictionary for translating grpc messages to revit actions
            synapseRevitService = SynapseRevitService.StartSynapseRevitService(new RevitTemplateWebSynapse());
            synapseRevitService.StartProcess();

            commandData.Application.ApplicationClosing += Application_ApplicationClosing;

            return Result.Succeeded;
        }

        private void Application_ApplicationClosing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            synapseRevitService.ShutdownSynapseRevitService();

            UIApplication uiapp = sender as UIApplication;
            if (uiapp == null)
            {
                return;
            }

            uiapp.ApplicationClosing -= Application_ApplicationClosing;
        }
    }
}

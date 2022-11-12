using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using RevitTemplateWeb.Core;

using Synapse.Revit;

using Autodesk.Revit.UI;

namespace RevitTemplateWebShared.Services
{
    public class RevitTemplateWebSynapse : IRevitSynapse
    {
        public string ProcessPath => Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)!,"UI", "gRPC.Client.exe");

        [SynapseRevitMethod((int)Commands.ShowTaskDialogTest, typeof(string), typeof(string), typeof(bool))]
        public string ShowTaskDialogTest(string message, bool testBool)
        {
            return Revit.Async.RevitTask.RunAsync(() =>
            {
                TaskDialog.Show("received bool: " + testBool, message);

                return "received!";
            }).Result;

        }
    }
}

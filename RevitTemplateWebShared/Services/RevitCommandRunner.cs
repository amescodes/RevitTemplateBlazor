using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RevitTemplateWeb.Core;
using Autodesk.Revit.UI;

namespace RevitTemplateWebShared.Services
{
    public class RevitCommandRunner
    {
        [RevitCommand(Commands.ShowTaskDialog, typeof(string), typeof(string), typeof(bool))]
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

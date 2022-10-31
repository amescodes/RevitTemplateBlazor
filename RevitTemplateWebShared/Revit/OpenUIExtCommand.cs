using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Revit.Async;

namespace RevitTemplateWeb
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenUIExtCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            RevitTask.Initialize(uiapp);

            // execute the browser window process
            Process process = new Process();
#if RELEASE
            string uiProcessPath = Path.Combine(Assembly.GetAssembly(typeof(OpenUIExtCommand)).Location, "UI\\RevitTemplateWeb.UI.exe");
#else
            string uiProcessPath = "C:\\Users\\amesh\\source\\repos\\RevitTemplate-amescodes\\RevitTemplateWeb.UI\\bin\\Debug\\net6.0\\RevitTemplateWeb.UI.exe";
#endif
            process.StartInfo.FileName = uiProcessPath;
            process.StartInfo.Arguments = uiapp.MainWindowHandle.ToString();

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                using (TaskDialog td = new TaskDialog("error")
                       {
                           MainContent = e.Message,
                           ExpandedContent = $"File not found: {uiProcessPath}",
                       })
                {
                    td.Show();
                }
            }
            
            return Result.Succeeded;
        }
    }
}

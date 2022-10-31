using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Windows;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;


namespace RevitTemplateWeb
{
    public class ExtApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel debuggerPanel = application.CreateRibbonPanel("Debugger");
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string assemblyPath = executingAssembly.Location;
            string assemblyNamespace = executingAssembly.GetName().Name;

            RibbonItemData btnData = new PushButtonData("debugBtn", "Debug",assemblyPath,$@"{assemblyNamespace}.{nameof(OpenUIExtCommand)}");
            debuggerPanel.AddItem(btnData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}

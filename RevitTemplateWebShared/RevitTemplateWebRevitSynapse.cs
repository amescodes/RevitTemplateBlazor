using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using RevitTemplateWeb.Core;

using Synapse.Revit;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTemplateWebShared.Services
{
    public class RevitTemplateWebSynapse : IRevitSynapse
    {
        private UIApplication uiapp;

        public string Id => "394D4D5F-025E-4A08-B564-86E939586017";
        public string ProcessPath => Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)!,"UI", "RevitTemplateWeb.UI.exe");

        public RevitTemplateWebSynapse(UIApplication uiapp)
        {
            this.uiapp = uiapp;
        }

        [SynapseRevitMethod(nameof(Commands.GetCurrentDocSiteLocation))]
        public object GetCurrentDocSiteLocation(string message, bool testBool)
        {
            return Revit.Async.RevitTask.RunAsync(() =>
            {
                //TaskDialog.Show("received bool: " + testBool, message);
                SiteLocation siteLocation = uiapp.ActiveUIDocument.Document.SiteLocation;

                return new RevitElementModel(siteLocation.Id.IntegerValue,siteLocation.PlaceName);
            }).Result;

        }
    }
}

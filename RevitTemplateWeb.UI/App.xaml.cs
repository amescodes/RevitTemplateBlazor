using Synapse;
using System.Windows;

namespace RevitTemplateWeb.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SynapseClient? SynapseClient { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            string eArg = e.Args[0];
            if (string.IsNullOrEmpty(eArg))
            {
                //todo show message about revit needing to be open
                Shutdown();
            }

            // port number is passed here in the command line argument
            int port = int.Parse(eArg);

            SynapseClient = SynapseClient.StartSynapseClient(port);
            
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SynapseClient?.Shutdown();
            base.OnExit(e);
        }
    }
}

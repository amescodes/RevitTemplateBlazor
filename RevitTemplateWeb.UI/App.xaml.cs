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
            SynapseClient = SynapseClient.StartSynapseClient();
            
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SynapseClient?.Shutdown();
            base.OnExit(e);
        }
    }
}

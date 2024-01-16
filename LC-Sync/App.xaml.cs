using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using LC_Sync.Core;
namespace LC_Sync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitCore.currentlyLoadedVersion = Assembly.GetExecutingAssembly().GetName().Version;
            await InitCore.SetupCoreAsync();
        }
    }
}

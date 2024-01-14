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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitCore.SetupCore();
        }
    }
}

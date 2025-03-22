using System.Configuration;
using System.Data;
using System.Windows;

namespace PerfumeManagement_SE172279
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Start with Login Window as default UI
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}

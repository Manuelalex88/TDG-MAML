using System.Configuration;
using System.Data;
using System.Windows;
using Prueba.data;
using Prueba.view;
namespace Prueba
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            var loginView = new Login(); 
            loginView.Show();
        }
    }

}

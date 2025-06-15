using System.Configuration;
using System.Data;
using System.Windows;
using GestionReparaciones.data;
using GestionReparaciones.view;
namespace GestionReparaciones
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

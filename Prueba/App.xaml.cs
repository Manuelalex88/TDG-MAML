using System.Configuration;
using System.Data;
using System.Windows;
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
            var Login = new Login();
            Login.Show();
            Login.IsVisibleChanged += (s, ev) =>
            {
                if (Login.IsVisible == false && Login.IsLoaded)
                {
                    var VentanaPrincipal = new VentanaPrincipal();
                    VentanaPrincipal.Show();
                    Login.Close();
                }
            };
        }
    }

}

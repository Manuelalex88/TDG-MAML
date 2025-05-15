using Npgsql;
using Prueba.viewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prueba.view
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public bool LoginCorrecto { get; set; } = false;
        public Login()
        {
            InitializeComponent();
            ConectarBaseDeDatos();
            // Enlazamos la acción de cierre para MVVM
            if (DataContext is loginViewModel vm)
            {
                vm.CerrarVentanaAction = () =>
                {
                    LoginCorrecto = true;
                     
                };
            }
        }
        /*Usamos esta función para poder mover la ventana con hacer click en ella en cualquier lugar -MAML*/
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton==MouseButtonState.Pressed) DragMove();
            
        }

        private void btnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            
        }


        /// <summary>
        /// FUNCIONES
        /// </summary>
        /*Prueba para conexión de BD -MAML*/
        private void ConectarBaseDeDatos()
        {
            try
            {
                // Obtén la cadena de conexión desde el archivo de configuración
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString;

                using (var conexion = new NpgsqlConnection(connectionString))
                {
                    conexion.Open();
                    MessageBox.Show("Conexión exitosa a la base de datos.");
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la base de datos: {ex.Message}");
            }
        }
        
    }
}

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
        
        public Login()
        {
            this.StateChanged += LoginView_StateChanged;
            InitializeComponent();
            
        }

        

        /*Usamos esta función para poder mover la ventana con hacer click en ella en cualquier lugar -MAML*/
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton==MouseButtonState.Pressed) DragMove();
            
        }



        #region Metodos
        /*Prueba para conexión de BD -MAML*/
        private void LoginView_StateChanged(object? sender, EventArgs e)
        { //Esta funcion para desacer la animacion de minimizar al volver a abrirlo
            if (this.WindowState == WindowState.Normal && this.Opacity < 1)
            {
                // Restaurar visibilidad suavemente
                var anim = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                this.BeginAnimation(Window.OpacityProperty, anim);
            }
        }

        public void MinimizarConAnimacion()
        {
            //Esta funcion para hacer una animacion de minimizar decente. Por que de base es muy tosco
            var anim = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            anim.Completed += (s, e) => this.WindowState = WindowState.Minimized;

            this.BeginAnimation(Window.OpacityProperty, anim);
        }
        #endregion
    }
}

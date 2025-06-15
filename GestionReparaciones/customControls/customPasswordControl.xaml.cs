using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestionReparaciones.customControls
{
    /// <summary>
    /// Logica de interaccion para customPasswordControl.xaml
    /// </summary>
    public partial class customPasswordControl : UserControl
    {
        // Define una DependencyProperty llamada "Password" que permite el enlace (binding) de datos seguros.
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(SecureString), typeof(customPasswordControl));

        // Propiedad CLR que encapsula la DependencyProperty. Permite acceso a la contraseña como SecureString.
        public SecureString Password
        {
            get { return (SecureString)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        // Constructor del control. Inicializa el componente y asocia el evento de cambio de contraseña.
        public customPasswordControl()
        {
            InitializeComponent();

            // Suscribe el evento PasswordChanged del control PasswordBox llamado "txtPass".
            txtPass.PasswordChanged += OnPasswordChanged;
        }

        // Evento que se dispara cuando el contenido del PasswordBox cambia.
        // Actualiza la propiedad Password con la contraseña segura del usuario.
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = txtPass.SecurePassword;
        }
    }

}

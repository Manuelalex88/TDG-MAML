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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Prueba.model;
using Prueba.viewModel;

namespace Prueba.view
{
    /// <summary>
    /// Lógica de interacción para VentanaPrincipal.xaml
    /// </summary>
    public partial class VentanaPrincipal : Window
    {
        public VentanaPrincipal()
        {
            InitializeComponent();

        }
        /*Requerimos usar la libreria de 32bits para manejar los eventos del propio sistema -MAML*/
        [DllImport("user32.dll")]
        /*Capturamos las senales del mouse y enviar mensajes para mover y arrastrar la ventana -MAML*/
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*Creamos y enviamos el identificador de la ventana y como parametro de 16 bits enviamos 2 y 0 -MAML*/
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlBarraControl_MouseEnter(object sender, MouseEventArgs e)
        {
            /*Para que al maximixar, esta tenga el tamano de la pantalla en la cual se ejecuta la app -MAML*/
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnminimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowState==WindowState.Normal)
            this.WindowState = WindowState.Maximized;
            else this.WindowState=WindowState.Normal;
        }

    }
}

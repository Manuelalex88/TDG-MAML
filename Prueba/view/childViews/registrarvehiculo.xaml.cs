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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prueba.view
{
    /// <summary>
    /// Lógica de interacción para registrarvehiculo.xaml
    /// </summary>
    public partial class Registrarvehiculo : UserControl
    {
        public Registrarvehiculo()
        {
            InitializeComponent();
        }


        private void cmbMotivoIngreso_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Comprobar si la opción seleccionada es "Diagnóstico sin identificar"
            if (cmbMotivoIngreso.SelectedItem != null &&
                ((ComboBoxItem)cmbMotivoIngreso.SelectedItem).Content.ToString() == "Diagnóstico sin identificar")
            {
                // Hacer visibles los controles
                txtDiagSinIdentificar.Visibility = Visibility.Visible;
                tbDiagSinIdentificar.Visibility = Visibility.Visible;
            }
            else
            {
                // Ocultar los controles
                txtDiagSinIdentificar.Visibility = Visibility.Collapsed;
                tbDiagSinIdentificar.Visibility = Visibility.Collapsed;
            }
        }
    }
}

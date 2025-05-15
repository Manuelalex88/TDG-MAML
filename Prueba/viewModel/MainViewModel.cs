using FontAwesome.Sharp;
using Prueba.model;
using Prueba.view.childViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Prueba.viewModel
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentChildView;
        private string _caption;
        private IconChar _icon;

        private string _nombreUsuario;

        //Propiedades
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                if (_nombreUsuario != value)
                {
                    _nombreUsuario = value;
                    cambioPropiedad(nameof(NombreUsuario));  // Notifica la actualización
                }
            }
        }
        public BaseViewModel CurrentChildView { get => _currentChildView; 
            set {
                _currentChildView = value;
                cambioPropiedad(nameof(CurrentChildView));
            }
        }
        public string Caption { get => _caption; 
            set { _caption = value;
                cambioPropiedad(nameof(Caption));
            }
        }
        public IconChar Icon
        {
            get  => _icon; 
            set
            {
                _icon = value;
                cambioPropiedad(nameof(Icon));
            }
        }

        //Comandos
        public ICommand showPrimerachildView {  get;}
        public ICommand showSegundachildView {  get;}
        public ICommand showReparacionesChildView {  get;}
        public ICommand showFacturasChildView{  get;}
        public ICommand ShowVehiculosEnTallerChildView {  get;}

        public MainViewModel()
        {
            showPrimerachildView = new comandoViewModel(ExecuteShowCommand);
            showSegundachildView = new comandoViewModel(ExecuteShowCommand2);
            showReparacionesChildView = new comandoViewModel(ExecuteShowCommand3);
            showFacturasChildView = new comandoViewModel(ExecuteShowCommand4);
            ShowVehiculosEnTallerChildView = new comandoViewModel(ExecuteShowCommand5);

            // Cargar el nombre del usuario desde UserData
            _nombreUsuario = UserData.Nombre;  
            cambioPropiedad(nameof(NombreUsuario));  

            //Deafult View
            ExecuteShowCommand(null);
        }

        private void ExecuteShowCommand5(object obj)
        {
            CurrentChildView = new VehiculosEnTallerViewModel();
            Caption = "Vehiculos En Taller";
            Icon = IconChar.Car;
        }

        private void ExecuteShowCommand4(object obj)
        {
            CurrentChildView = new FacturaViewModel();
            Caption = "Facturas";
            Icon = IconChar.MoneyBill;
        }

        private void ExecuteShowCommand3(object obj)
        {
            CurrentChildView = new ReparacionesViewModel();
            Caption = "Reparaciones";
            Icon = IconChar.Wrench;
        }

        private void ExecuteShowCommand2(object obj)
        {
            CurrentChildView = new registrarVehiculosViewModel();
            Caption = "Registrar Vehiculo";
            Icon = IconChar.Database;
        }

        private void ExecuteShowCommand(object obj)
        {
            CurrentChildView = new principalViewModel();
            Caption = "Dashboard";
            Icon = IconChar.House;
        }

        
    }
    
}

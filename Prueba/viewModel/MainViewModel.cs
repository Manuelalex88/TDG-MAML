using FontAwesome.Sharp;
using Prueba.model;
using Prueba.view.childViews;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Prueba.viewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Campos
        private BaseViewModel? _currentChildView;
        private string _caption = String.Empty;
        private IconChar _icon;
        private string _nombreUsuario = String.Empty;
        
        #endregion
        #region Propiedades
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                if (_nombreUsuario != value)
                {
                    _nombreUsuario = value;
                    OnPropertyChanged(nameof(NombreUsuario));  // Notifica la actualización
                }
            }
        }
        public BaseViewModel? CurrentChildView { 
            get => _currentChildView; 
            set => SetProperty(ref _currentChildView, value);
        }
        public string Caption { 
            get => _caption; 
            set => SetProperty(ref _caption, value);
        }
        public IconChar Icon
        {
            get  => _icon; 
            set => SetProperty(ref _icon, value);
        }
        #endregion

        #region Comandos
        public ICommand showPrimerachildView {  get;}
        public ICommand showSegundachildView {  get;}
        public ICommand showReparacionesChildView {  get;}
        public ICommand showFacturasChildView{  get;}
        public ICommand ShowVehiculosEnTallerChildView {  get;}
        #endregion
        //Constructor
        public MainViewModel()
        {   // Obtener el ID del mecánico desde el hilo actual
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name;

            showPrimerachildView = new comandoViewModel(ExecuteShowCommand);
            showSegundachildView = new comandoViewModel(ExecuteShowCommand2);
            showReparacionesChildView = new comandoViewModel(ExecuteShowCommand3);
            showFacturasChildView = new comandoViewModel(ExecuteShowCommand4);
            ShowVehiculosEnTallerChildView = new comandoViewModel(ExecuteShowCommand5);
            // Cargar el nombre del usuario desde UserData
            _nombreUsuario = identity?.NombreCompleto ?? "Desconocido"; 
            OnPropertyChanged(nameof(NombreUsuario));  

            //Deafult View
            ExecuteShowCommand(null);
        }
        #region Metodos
        //Metodo para no poner set { _loquesea = value; OnPropertyChanged(loquesea) y poner simplemente SetProperty(ref Loquesea, value)
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;

            if (propertyName != null)
                OnPropertyChanged(propertyName);

            return true;
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
            CurrentChildView = new RegistrarVehiculosViewModel();
            Caption = "Registrar Vehiculo";
            Icon = IconChar.Database;
        }

        private void ExecuteShowCommand(object? obj)
        {
            CurrentChildView = new PrincipalViewModel();
            Caption = "Dashboard";
            Icon = IconChar.House;
        }

        #endregion
    }

}

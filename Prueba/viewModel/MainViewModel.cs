using FontAwesome.Sharp;
using Prueba.model;
using Prueba.view;
using Prueba.view.childViews;
using Prueba.viewModel.viewModelAdmin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public bool EsAdmin => Thread.CurrentPrincipal?.IsInRole("admin") == true;
        public bool IsCheckedMecanicos => EsAdmin;
        public bool IsCheckedPrincipal => !EsAdmin;

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
        public ICommand showPrincipalChildViewCommand {  get;}
        public ICommand showRegistrarVehiculoChildViewCommand {  get;}
        public ICommand showReparacionesChildViewCommand {  get;}
        public ICommand showFacturasChildViewCommand{  get;}
        public ICommand showVehiculosEnTallerChildViewCommand {  get;}
        public ICommand showMecanicosChildViewCommand { get;}
        public ICommand CerrarSesionCommand { get; }
        public ICommand MostrarAyudaCommand { get; }
        #endregion
        //Constructor
        public MainViewModel()
        {   // Obtener el ID del mecánico desde el hilo actual
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name;

            //Instanciaar
            showPrincipalChildViewCommand = new comandoViewModel(ShowPrincipalChildView);
            showRegistrarVehiculoChildViewCommand = new comandoViewModel(ShowRegistrarVehiculoChildView);
            showReparacionesChildViewCommand = new comandoViewModel(ExecuteShowCommand3);
            showFacturasChildViewCommand = new comandoViewModel(MostrarFacturasChildView);
            showVehiculosEnTallerChildViewCommand = new comandoViewModel(ShowMecanicosChildView);
            showMecanicosChildViewCommand = new comandoViewModel(MostrarMecanicoChildView, _ => EsAdmin);
            CerrarSesionCommand = new comandoViewModel(CerrarSesion);
            MostrarAyudaCommand = new comandoViewModel(MostrarAyuda);

            // Inicializar comandos solo si es admin
            if (EsAdmin)
            {
                
            }

            // Cargar el nombre del usuario desde UserData
            _nombreUsuario = identity?.NombreCompleto ?? "Desconocido"; 
            OnPropertyChanged(nameof(NombreUsuario));  

            //Deafult View
            if(EsAdmin){
               MostrarMecanicoChildView(null);
            }
            else
            {
                ShowPrincipalChildView(null);
            }
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
        public void MostrarAyuda(object obj)
        {
            string rutaPdf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "Ayuda_GestionTalleres.pdf");


            if (File.Exists(rutaPdf))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = rutaPdf,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("El archivo de ayuda no fue encontrado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CerrarSesion(object obj)
        {
            // Crear una nueva instancia de la ventana de login
            var loginWindow = new Login();
            loginWindow.Show();

            // Cerrar la ventana actual (MainView)
            // Buscamos la ventana activa que sea del tipo MainView y la cerramos
            foreach (Window window in Application.Current.Windows)
            {
                if (window is VentanaPrincipal)
                {
                    window.Close();
                    break;
                }
            }
        }
        private void MostrarMecanicoChildView(object? obj)
        {
            CurrentChildView = new MecanicosViewModel();
            Caption = "Mecanicos";
            Icon = IconChar.User;
        }
        private void ShowMecanicosChildView(object obj)
        {
            CurrentChildView = new VehiculosEnTallerViewModel();
            Caption = "Vehiculos En Taller";
            Icon = IconChar.Car;
        }

        private void MostrarFacturasChildView(object obj)
        {
            if (EsAdmin)
            {
                CurrentChildView = new FacturasAdminViewModel(); 
                Caption = "Facturas (Admin)";
                Icon = IconChar.MoneyBill;
            }
            else
            {
                CurrentChildView = new FacturaViewModel();
                Caption = "Facturas";
                Icon = IconChar.MoneyBill;
            }
        }

        private void ExecuteShowCommand3(object obj)
        {
            CurrentChildView = new ReparacionesViewModel();
            Caption = "Reparaciones";
            Icon = IconChar.Wrench;
        }

        private void ShowRegistrarVehiculoChildView(object obj)
        {
            CurrentChildView = new RegistrarVehiculosViewModel();
            Caption = "Registrar Vehiculo";
            Icon = IconChar.Database;
        }

        private void ShowPrincipalChildView(object? obj)
        {
            CurrentChildView = new PrincipalViewModel();
            Caption = "Dashboard";
            Icon = IconChar.House;
        }

        #endregion
    }

}

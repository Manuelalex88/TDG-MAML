using Prueba.view;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prueba.model;
using Prueba.view.childViews;
using System.Collections.ObjectModel;
using Prueba.data;
using System.Windows.Input;
using System.Windows;
using System.Runtime.CompilerServices;


namespace Prueba.viewModel
{
    public class PrincipalViewModel : BaseViewModel
    {


        #region Listas
        public ObservableCollection<VehiculoReparacionDTO> ReparacionesAsignadas { get; set; }
        private ObservableCollection<FacturaVehiculoClienteDTO> _misFacturas;
        #endregion
        #region Campos
        private readonly VehiculoRepository _vehiculoRepository;
        private readonly FacturaRepository _facturaRepository;
        public string _textoEntrada;
        #endregion
        public ObservableCollection<FacturaVehiculoClienteDTO> MisFacturas
        {
            get => _misFacturas;
            set => SetProperty(ref _misFacturas, value);
        }
        public string TextoEntrada
        {
            get => _textoEntrada;
            set => SetProperty(ref _textoEntrada, value);
        }
        public PrincipalViewModel()
        {
            //Identidad Mecanico
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name ?? "Desconocido";
            
            //Instanciar
            _textoEntrada = string.Empty;
            TextoEntrada = "Bienvenido, " + (identity?.NombreCompleto ?? "Desconocido");
            ReparacionesAsignadas = new ObservableCollection<VehiculoReparacionDTO>();
            _vehiculoRepository = new VehiculoRepository();
            _misFacturas = new ObservableCollection<FacturaVehiculoClienteDTO>();
            _facturaRepository = new FacturaRepository();
            //Metodos
            CargarReparacionesAsignadas(idMecanico);
            CargarFacturasFinalizas(idMecanico);
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
        private void CargarFacturasFinalizas(string idMecanico)
        {
            if(MisFacturas == null)
            {
                MisFacturas = new ObservableCollection<FacturaVehiculoClienteDTO>();
            }

            try
            {
                MisFacturas.Clear();


                var lista = _facturaRepository.ObtenerFacturasPagadasPorMecanico(idMecanico);
                foreach (var v in lista)
                {
                    MisFacturas.Add(v);
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Error al cargar los vehículos asignados:\n" +
                "Mensaje: " + ex.Message + "\n" +
                "Fuente: " + ex.Source + "\n" +
                "StackTrace: " + ex.StackTrace);
            }
        }
        private void CargarReparacionesAsignadas(string idMecanico)
        {

            try
            {
                var lista = _vehiculoRepository.ObtenerVehiculosAsignados(idMecanico);
                foreach (var v in lista)
                {
                    ReparacionesAsignadas.Add(v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los vehículos asignados: " + ex.Message);
            }
        }
        #endregion

    }
}

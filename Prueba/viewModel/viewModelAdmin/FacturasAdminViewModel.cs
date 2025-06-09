using Prueba.data;
using Prueba.model;
using Prueba.repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel.viewModelAdmin
{
    public class FacturasAdminViewModel : BaseViewModel
    {
        #region Lista
        public ObservableCollection<Factura> _facturasList;
        #endregion

        #region Campos
        private FacturaRepository _facturaRepository;
        private Factura _facturaSeleccionada;

        
        #endregion

        #region Propiedades
        public ObservableCollection<Factura> FacturaList
        {
            get => _facturasList;
            set => SetProperty(ref _facturasList, value);
        }
        public Factura FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set => SetProperty(ref _facturaSeleccionada, value);
        }

        
        #endregion

        #region Comandos
        public ICommand MostrarFacturasAdminCommand { get;}
        public ICommand BorrarFacturaCommand { get; }
        #endregion



        public FacturasAdminViewModel()
        {
            //Instanciar
            _facturasList = new ObservableCollection<Factura>();
            _facturaRepository = new FacturaRepository();
            _facturaSeleccionada = new Factura();

            //Comando
            BorrarFacturaCommand = new comandoViewModel(BorrarFactura);
            MostrarFacturasAdminCommand = new comandoViewModel(MostrarFacturasAdmin);

            //Mostrar la lista
            MostrarFacturasAdminCommand.Execute(null);
        }
        #region Metodos
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;

            if (propertyName != null)
                OnPropertyChanged(propertyName);

            return true;
        }
        public void MostrarFacturasAdmin (object obj)
        {
            try
            {
                FacturaList.Clear();

                var lista = _facturaRepository.MostrarFacturaAdmin();
                foreach (var v in lista)
                {
                    FacturaList.Add(v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar a las Facturas:\n" +
                    "Mensaje: " + ex.Message + "\n" +
                    "Fuente: " + ex.Source + "\n" +
                    "StackTrace: " + ex.StackTrace);
            }
        }
        public void BorrarFactura(Object obj)
        {
            MessageBox.Show("Factura borrada con exito");
        }
        #endregion
    }
}

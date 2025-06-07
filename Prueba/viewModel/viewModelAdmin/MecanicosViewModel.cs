using Prueba.data;
using Prueba.model;
using Prueba.repository;
using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel.viewModelAdmin
{
    public class MecanicosViewModel : BaseViewModel
    {
        #region Listas
        private ObservableCollection<Mecanico> _mecanicoList;
        #endregion

        #region Campos
        private Mecanico _mecanicoSeleccionado;
        private MecanicoRepository _mecanicoRepository;

        private string _nombreMecanico;
        private string _idMecanico;
        private string _contrasena;
        #endregion
        #region Propiedades
        public Mecanico MecanicoSeleccionado
        {
            get => _mecanicoSeleccionado;
            set
            {
                if (SetProperty(ref _mecanicoSeleccionado, value))
                {
                    if (_mecanicoSeleccionado != null)
                    {
                        NombreMecanico = _mecanicoSeleccionado.Nombre;
                        MecanicoID = _mecanicoSeleccionado.Id;
                        Contrasena = _mecanicoSeleccionado.Contrasena;
                    }
                    else
                    {
                        NombreMecanico = string.Empty;
                        MecanicoID = string.Empty;
                        Contrasena = string.Empty;
                    }
                }
            }
        }
        public ObservableCollection<Mecanico> MecanicoList
        {
            get => _mecanicoList;
            set => SetProperty(ref _mecanicoList, value);
        }
        public string NombreMecanico
        {
            get => _nombreMecanico;
            set => SetProperty(ref _nombreMecanico, value);
        }
        public string MecanicoID
        {
            get => _idMecanico;
            set => SetProperty(ref _idMecanico, value);
        }
        public string Contrasena
        {
            get => _contrasena;
            set => SetProperty(ref _contrasena, value);
        }
        #endregion

        #region Comandos
        public ICommand MostrarMecanicosCommand { get; }
        public ICommand GuardarMecanicoCommand { get; }
        public ICommand EliminarMecanicoCommand { get; }
        public ICommand NuevoMecanicoCommand { get; }
        #endregion

        public MecanicosViewModel()
        {
            //Instanciar
            MecanicoList = new ObservableCollection<Mecanico>();
            _mecanicoList = new ObservableCollection<Mecanico>();
            _mecanicoSeleccionado = null!; //  PON NULL PARA QUE FUNCIONE LA COMPROBACION DEL NULL
            _nombreMecanico = string.Empty;
            _idMecanico = string.Empty;
            _contrasena = string.Empty;
            _mecanicoRepository = new MecanicoRepository();
            //Comando
            MostrarMecanicosCommand = new comandoViewModel(MostrarMecanicos);
            GuardarMecanicoCommand = new comandoViewModel(GuardarMecanico);
            EliminarMecanicoCommand = new comandoViewModel(EliminarMecanico);
            NuevoMecanicoCommand = new comandoViewModel(NuevoMecanico);
   
            MostrarMecanicosCommand.Execute(null);
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
        private void MostrarMecanicos(object? obj)
        {
            try
            {
                MecanicoList.Clear();

                var lista = _mecanicoRepository.ObtenerMecanicos();
                foreach (var v in lista)
                {
                    MecanicoList.Add(v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar a los Mecanicos:\n" +
                    "Mensaje: " + ex.Message + "\n" +
                    "Fuente: " + ex.Source + "\n" +
                    "StackTrace: " + ex.StackTrace);
            }
        }
        private void GuardarMecanico(object obj)
        {
            
        }
        private void EliminarMecanico(object obj)
        {
            if (MecanicoSeleccionado == null) //ESTA COMPROBACIONN
            {

                MessageBox.Show("Selecciona un Mecanico para guardar cambios.");
                return;
            }
            MessageBox.Show("Eliminar Siu");
        }
        private void NuevoMecanico(object obj)
        {

        }
        #endregion
    }
}

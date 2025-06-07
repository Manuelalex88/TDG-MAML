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
        #endregion
        #region Propiedades
        public Mecanico MecanicoSeleccionado
        {
            get => _mecanicoSeleccionado;
            set => SetProperty(ref _mecanicoSeleccionado, value);
        }
        public ObservableCollection<Mecanico> MecanicoList
        {
            get => _mecanicoList;
            set => SetProperty(ref _mecanicoList, value);
        }
        #endregion

        #region Comandos
        public ICommand MostrarMecanicosCommand { get; }
        #endregion

        public MecanicosViewModel()
        {
            //Instanciar
            MecanicoList = new ObservableCollection<Mecanico>();
            _mecanicoList = new ObservableCollection<Mecanico>();
            _mecanicoSeleccionado = new Mecanico();
            _mecanicoRepository = new MecanicoRepository();
            //Comando
            MostrarMecanicosCommand = new comandoViewModel(MostrarMecanicos);
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
        private void MostrarMecanicos(object obj)
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
        #endregion
    }
}

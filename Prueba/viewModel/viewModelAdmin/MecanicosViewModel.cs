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
                        ContrasenaMecanico = _mecanicoSeleccionado.Contrasena;
                    }
                    else
                    {
                        NombreMecanico = string.Empty;
                        MecanicoID = string.Empty;
                        ContrasenaMecanico = string.Empty;
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
        public string ContrasenaMecanico
        {
            get => _contrasena;
            set => SetProperty(ref _contrasena, value);
        }
        #endregion

        #region Comandos
        public ICommand MostrarMecanicosCommand { get; }
        public ICommand ModificarMecanicoCommand { get; }
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
            ModificarMecanicoCommand = new comandoViewModel(GuardarMecanico);
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
            // Eliminar espacios en blanco al inicio y final
            MecanicoID = MecanicoID.Trim();
            NombreMecanico = NombreMecanico.Trim();
            ContrasenaMecanico = ContrasenaMecanico.Trim();

            if (MecanicoSeleccionado == null)
            {

                MessageBox.Show("Selecciona un Mecanico para editarlo");
                return;
            }
            else if (MecanicoSeleccionado.Id == "Admin")
            {
                MessageBox.Show("No se puede editar el admin");
                return;
            }

            try
            {

                // Validar si el ID no existe ya (Haz exepcion del seleccionado)
                var mecanicoExistente = _mecanicoRepository.ObtenerMecanicoPorId(MecanicoID);
                if (mecanicoExistente != null && mecanicoExistente.Id != MecanicoSeleccionado.Id)
                {
                    MessageBox.Show("Ya existe un mecánico con ese ID.");
                    return;
                }

                var nuevoMecanico = new Mecanico
                {
                    Id = MecanicoID,
                    Nombre = NombreMecanico,
                    Contrasena = ContrasenaMecanico
                };

                _mecanicoRepository.ModificarMecanico(nuevoMecanico, MecanicoSeleccionado.Id);
                MessageBox.Show("Mecanico Modificado con exito");
                LimpiarCampos(nuevoMecanico, 2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar al Mecanico:\n" +
                                    "Mensaje: " + ex.Message + "\n" +
                                    "Fuente: " + ex.Source + "\n" +
                                    "StackTrace: " + ex.StackTrace);
            }
        }
        private void EliminarMecanico(object obj)
        {
            // Eliminar espacios en blanco al inicio y final
            MecanicoID = MecanicoID.Trim();
            NombreMecanico = NombreMecanico.Trim();
            ContrasenaMecanico = ContrasenaMecanico.Trim();

            if (MecanicoSeleccionado == null)
            {

                MessageBox.Show("Selecciona un Mecanico para eliminarlo");
                return;
            }
            else if (MecanicoSeleccionado.Id == "Admin")
            {
                MessageBox.Show("No se puede eliminar el admin");
                return;
            }

            try
            {

                _mecanicoRepository.EliminarMecanico(MecanicoSeleccionado.Id);
                LimpiarCampos(MecanicoSeleccionado,1);

                MecanicoSeleccionado = null;

                MessageBox.Show("Mecanico eliminado correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar a los Mecanicos:\n" +
                                    "Mensaje: " + ex.Message + "\n" +
                                    "Fuente: " + ex.Source + "\n" +
                                    "StackTrace: " + ex.StackTrace);
            }
        }
        private void NuevoMecanico(object obj)
        {
            // Eliminar espacios en blanco al inicio y final
            MecanicoID = MecanicoID.Trim();
            NombreMecanico = NombreMecanico.Trim();
            ContrasenaMecanico = ContrasenaMecanico.Trim();

            if (MecanicoID == "Admin")
            {
                MessageBox.Show("No se puede crear otro admin");
                return;
            }

            try
            {

                // Validar si el ID no existe ya
                var mecanicoExistente = _mecanicoRepository.ObtenerMecanicoPorId(MecanicoID);
                if (mecanicoExistente != null)
                {
                    MessageBox.Show("Ya existe un mecánico con ese ID.");
                    return;
                }

                var nuevoMec = new Mecanico
                {
                    Id = MecanicoID,
                    Nombre = NombreMecanico,
                    Contrasena = ContrasenaMecanico
                };

                _mecanicoRepository.RegistrarMecanico(nuevoMec);
                LimpiarCampos(nuevoMec,0);

                MessageBox.Show("Mecanico creado correctamente");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar al nuevo Mecanico:\n" +
                                    "Mensaje: " + ex.Message + "\n" +
                                    "Fuente: " + ex.Source + "\n" +
                                    "StackTrace: " + ex.StackTrace);
            }
        }
        private void LimpiarCampos(Mecanico mec, int accion)
        {
            var mecanicoActual = MecanicoSeleccionado;

            MecanicoID = string.Empty;
            ContrasenaMecanico = string.Empty;
            NombreMecanico = string.Empty;
            MecanicoSeleccionado = null;

            switch (accion)
            {
                case 0: // Nuevo
                    if (mec != null)
                        MecanicoList.Add(mec);
                    break;

                case 1: // Eliminar
                    if (mec != null)
                        MecanicoList.Remove(mec);
                    break;

                case 2: // Modificar directamente de la lista sin recargar toda la lista
                    if (mec != null && mecanicoActual != null)
                    {
                        var index = MecanicoList.IndexOf(mecanicoActual);
                        if (index >= 0)
                        {
                            MecanicoList[index] = mec;
                        }
                        else
                        {
                            MostrarMecanicos(null);
                        }
                    }
                    break;
            }
        }
        #endregion
    }
}

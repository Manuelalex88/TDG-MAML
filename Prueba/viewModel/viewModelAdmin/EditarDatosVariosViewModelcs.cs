using Prueba.data;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prueba.viewModel.viewModelAdmin
{
    public class EditarDatosVariosViewModelcs : BaseViewModel, IDataErrorInfo
    {
        #region Campos
        private string _nuevoNombreTaller;
        private int _iva;
        private string _calle;
        private string _municipio;
        private string _ciudad;
        private string _telefono;
        private string _email;
        private string _cif;
        private decimal _manoObra;

        #endregion
        #region Propiedades
        public string NuevoNombreTaller
        {
            get => _nuevoNombreTaller;
            set => SetProperty(ref _nuevoNombreTaller, value);
        }
        public int IVA
        {
            get => _iva;
            set => SetProperty(ref _iva, value);
        }
        public string Calle
        {
            get => _calle;
            set => SetProperty(ref _calle, value);
        }
        public string Municipio
        {
            get => _municipio;
            set => SetProperty(ref _municipio, value);
        }
        public string Ciudad
        {
            get => _ciudad;
            set => SetProperty(ref _ciudad, value);
        }
        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        public string CIF
        {
            get => _cif;
            set => SetProperty(ref _cif, value);
        }
        public decimal ManoObra
        {
            get => _manoObra;
            set => SetProperty(ref _manoObra, value);
        }
        #region Validaciones (IDataErrorInfo)
        public string this[string propertyName]
        {
            get
            {
                string? result = null;
                switch (propertyName)
                {
                    case nameof(CIF):
                        if (!string.IsNullOrWhiteSpace(CIF) && !EsCifValido(CIF))
                            result = "El CIF no tiene un formato válido (por ejemplo: B12345678).";
                        break;

                    case nameof(Email):
                        if (!string.IsNullOrWhiteSpace(Email) && !EsEmailValido(Email))
                            result = "El correo electrónico no es válido.";
                        break;
                    case nameof(Telefono):
                        if (!Regex.IsMatch(Telefono ?? "", @"^\d{9}$"))
                            result = "El teléfono debe contener 9 dígitos.";
                        break;
                    case nameof(ManoObra):
                        if (ManoObra < 0)
                            result = "La mano de obra no puede ser negativa.";
                        break;

                    case nameof(IVA):
                        if (IVA < 0 || IVA > 100)
                            result = "El IVA debe estar entre 0 y 100.";
                        break;
                }

                return result ?? string.Empty;
            }
        }

        public string Error => null!;

        // Validacion de CIF 
        private bool EsCifValido(string cif)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(cif, @"^[A-HJNP-SUVW]\d{7}[0-9A-J]$");
        }

        // Validacion correo electronico
        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #endregion
        #region Comandos
        public ICommand GuardarNombreTallerCommand { get; }
        public ICommand GuardarManoObraIvaCommand { get; }
        public ICommand GuardarDatosFacturaCommand { get; }




        #endregion
        //Constructor
        public EditarDatosVariosViewModelcs()
        {
            //Instanciar
            _nuevoNombreTaller = string.Empty;
            _cif = string.Empty;
            _calle = string.Empty;
            _municipio = string.Empty;
            _ciudad = string.Empty;
            _telefono = string.Empty;
            _email = string.Empty;
            //Instanciar comandos
            GuardarNombreTallerCommand = new comandoViewModel(GuardarNombreTaller);
            GuardarManoObraIvaCommand = new comandoViewModel(GuardarManoObraIva, PuedeGuardarManoObraIva);
            GuardarDatosFacturaCommand = new comandoViewModel(GuardarDatosFactura, PuedeGuardarDatosFactura);
            //Cargar de base
            var config = GestorConfiguracion.CargarConfiguracion();
            if (config != null)
            {
                IVA = config.IVA;
                Calle = config.Calle;
                Municipio = config.Municipio;
                Ciudad = config.Ciudad;
                Telefono = config.Telefono;
                Email = config.Email;
                CIF = config.CIF;
                ManoObra = config.ManoObra;
                NuevoNombreTaller = config.NombreTaller;
            }

        }
        #region Metodos
        // Metodo auxiliar para simplificar el OnPropertyChanged (No agregar lo mismo en todas las propiedades)
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;

            if (propertyName != null)
                OnPropertyChanged(propertyName);

            return true;
        }
        public void GuardarNombreTaller(object obj)
        {
            if (string.IsNullOrWhiteSpace(NuevoNombreTaller))
            {
                MessageBox.Show("El nombre del taller no puede estar vacío.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Cargar configuracion actual
            var config = GestorConfiguracion.CargarConfiguracion() ?? new ConfiguracionApp();

            // Actualizar el nombre del taller 
            config.NombreTaller = NuevoNombreTaller;

            try
            {
                string ruta = DatosConstantes.rutaConfiguracion;
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, opciones);
                File.WriteAllText(ruta, json);

                DatosConstantes.NombreTaller = NuevoNombreTaller;

                MessageBox.Show("Nombre del taller guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                if (obj is Window ventana)
                    ventana.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la configuración: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void GuardarManoObraIva(object obj)
        {
            try
            {
                // Cargar configuracion existente
                var config = GestorConfiguracion.CargarConfiguracion() ?? new ConfiguracionApp();

                // Actualizar solo IVA y ManoObra
                config.IVA = this.IVA;
                config.ManoObra = this.ManoObra;

                // Guardar en archivo JSON
                string ruta = DatosConstantes.rutaConfiguracion;
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(ruta, JsonSerializer.Serialize(config, opciones));

                // Notificar al usuario
                MessageBox.Show("IVA y Mano de Obra guardados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar IVA y Mano de Obra:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GuardarDatosFactura(object obj)
        {
            // Validar datos básicos (puedes hacer validaciones específicas aquí)
            if (string.IsNullOrWhiteSpace(Calle) ||
                string.IsNullOrWhiteSpace(Municipio) ||
                string.IsNullOrWhiteSpace(Ciudad))
            {
                MessageBox.Show("Por favor, complete los campos Calle, Municipio y Ciudad.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Cargar configuración existente o crear nueva
                var config = GestorConfiguracion.CargarConfiguracion() ?? new ConfiguracionApp();

                // Actualizar campos de factura
                config.Calle = this.Calle;
                config.Municipio = this.Municipio;
                config.Ciudad = this.Ciudad;
                config.Telefono = this.Telefono;
                config.Email = this.Email;
                config.CIF = this.CIF;

                // Guardar en archivo JSON
                string ruta = DatosConstantes.rutaConfiguracion;
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, opciones);
                File.WriteAllText(ruta, json);


                MessageBox.Show("Datos de la factura guardados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                if (obj is Window ventana)
                    ventana.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar los datos de la factura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Metodos de Validacion

        private bool PuedeGuardarManoObraIva(object? obj)
        {
            // Validar que los valores estén en rango
            bool camposObligatorios = IVA >= 0 && IVA <= 100 && ManoObra >= 0;

            // Validar que no haya errores de validación
            bool sinErrores = string.IsNullOrEmpty(this[nameof(IVA)]) &&
                              string.IsNullOrEmpty(this[nameof(ManoObra)]);

            // Solo permitir guardar si todo está bien
            return camposObligatorios && sinErrores;
        }
        private bool PuedeGuardarDatosFactura(object? obj)
        {
            bool camposValidos =
                !string.IsNullOrWhiteSpace(Calle) &&
                !string.IsNullOrWhiteSpace(Municipio) &&
                !string.IsNullOrWhiteSpace(Ciudad);

            bool sinErrores = string.IsNullOrEmpty(this[nameof(CIF)]) &&
                              string.IsNullOrEmpty(this[nameof(Email)]) &&
                              string.IsNullOrEmpty(this[nameof(Telefono)]);

            return camposValidos && sinErrores;
        }

        #endregion
    }
}

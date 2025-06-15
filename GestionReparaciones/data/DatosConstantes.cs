using System;
using System.IO;

namespace GestionReparaciones.data
{
    public static class DatosConstantes
    {
        #region Constantes fijas
        public const decimal MantenimientoBasico = 120.00m;

        public const string Estado1 = "Problema sin identificar";
        public const string Estado2 = "Diagnosticando";
        public const string Estado3 = "Esperando Repuesto";
        public const string Estado4 = "En Reparacion";
        public const string Estado5 = "Pendiente de Factura";
        public const string Estado6 = "Finalizada";

        public const string NombreAPP = "Gestión Reparaciones";
        public static string rutaConfiguracion => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "configuracion.json");
        #endregion

        #region Campos cargados desde configuracion
        private static string _nombreTaller;
        private static int _iva;
        private static decimal _manoObra;

        private static string _calle;
        private static string _municipio;
        private static string _ciudad;
        private static string _telefono;
        private static string _email;
        private static string _cif;
        #endregion

        #region Propiedades publicas
        public static string NombreTaller
        {
            get => _nombreTaller;
            set => _nombreTaller = value;
        }

        public static int Iva
        {
            get => _iva;
            set => _iva = value;
        }

        public static decimal ManoObra
        {
            get => _manoObra;
            set => _manoObra = value;
        }

        public static string Calle
        {
            get => _calle;
            set => _calle = value;
        }

        public static string Municipio
        {
            get => _municipio;
            set => _municipio = value;
        }

        public static string Ciudad
        {
            get => _ciudad;
            set => _ciudad = value;
        }

        public static string Telefono
        {
            get => _telefono;
            set => _telefono = value;
        }

        public static string Email
        {
            get => _email;
            set => _email = value;
        }

        public static string CIF
        {
            get => _cif;
            set => _cif = value;
        }
        #endregion

        #region Inicializacion estatica
        static DatosConstantes()
        {
            var config = GestorConfiguracion.CargarConfiguracion();

            _nombreTaller = config?.NombreTaller ?? "Nombre por defecto";
            _iva = config?.IVA ?? 21;
            _manoObra = config?.ManoObra ?? 0;

            _calle = config?.Calle ?? string.Empty;
            _municipio = config?.Municipio ?? string.Empty;
            _ciudad = config?.Ciudad ?? string.Empty;
            _telefono = config?.Telefono ?? string.Empty;
            _email = config?.Email ?? string.Empty;
            _cif = config?.CIF ?? string.Empty;
        }
        #endregion
    }
}

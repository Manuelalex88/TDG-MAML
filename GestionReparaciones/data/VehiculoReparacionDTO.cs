﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionReparaciones.data
{
    //Los DTO (data transfer object) sirve para ensanblar datos combinados de dos tablas en mi caso
    public class VehiculoReparacionDTO
    {
        //Vehiculo
        public string Marca { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Descripcion {  get; set; } = string.Empty;
        //Reparacion 
        public int IdReparacion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string TrabajoARealizar { get; set; } = string.Empty;
        public DateTime FechaFin {  get; set; } = DateTime.Now;
        public DateTime Fecha_Inicio {  get; set; } = DateTime.Now;
    }

}

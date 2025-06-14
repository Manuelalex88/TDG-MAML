using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Prueba.viewModel
{
    public class comandoViewModel : ICommand
    {
        /// <summary>
        /// CAMPOS
        /// </summary>
        //Action para encapsular un metodo, es decir pasamos un metodo como parametro
        private readonly Action<object> action = _ => { };
        //Predicate para ver si el campo se puede ejecutar
        private readonly Predicate<object?>? predicate;
        //Constructores
        public comandoViewModel(Action<object> action) //No todos los eventos necesitan ser evaluados
        {
            this.action = action;
            this.predicate = null;
        }
        public comandoViewModel(Action<object> action, Predicate<object?> predicate)
        {
            this.action = action;
            this.predicate = predicate;
        }
        // Eventos

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; } //Si la condicion ha cambiado
            remove { CommandManager.RequerySuggested -= value; }
        }

        //Metodos
        public bool CanExecute(object? parameter)
        {
            return predicate == null || predicate(parameter); //Si se puede ejecutar
        }

        public void Execute(object? parameter)
        {
            if(parameter == null) parameter = "Error";

            action?.Invoke(parameter);
        }

    }
}

using Prueba.view.childViews;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.viewModel
{
    /*Lo colocamos en abstracto para que solo se use en herencia
     Implementamos la interfaz que notificara los cambios (INotifyPropertyChanged) */
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(String nombre)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
        }

    }
}

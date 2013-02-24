using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Data;

namespace MetroExplorer.Common
{
    /// <summary>
    /// Implémentation de <see cref="INotifyPropertyChanged"/> pour simplifier les modèles.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Événement multidiffusion pour les notifications de modification de propriétés.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Vérifie si une propriété correspond déjà à une valeur voulue. Définit la propriété et
        /// notifie les écouteurs uniquement lorsque cela est nécessaire.
        /// </summary>
        /// <typeparam name="T">Type de la propriété.</typeparam>
        /// <param name="storage">Référence à une propriété avec les accesseurs Get et Set.</param>
        /// <param name="value">Valeur voulue pour la propriété.</param>
        /// <param name="propertyName">Nom de la propriété utilisée pour notifier les écouteurs. Cette
        /// valeur est facultative et elle peut être fournie automatiquement lorsqu'appelée à partir de compilateurs
        /// prenant en charge CallerMemberName.</param>
        /// <returns>True si la valeur a changé, false si la valeur correspondait à la
        /// valeur voulue.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifie les écouteurs qu'une valeur de propriété a changé.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété utilisée pour notifier les écouteurs. Cette
        /// valeur est facultative et elle peut être fournie automatiquement lorsqu'appelée à partir de compilateurs
        /// prenant en charge <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

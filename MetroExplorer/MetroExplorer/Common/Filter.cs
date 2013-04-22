namespace MetroExplorer.Common
{
    using System;

    /// <summary>
    /// Modèle d'affichage décrivant l'un des filtres disponibles pour l'affichage des résultats de recherche.
    /// </summary>
    public sealed class Filter : BindableBase
    {
        private String _name;
        private int _count;
        private bool _active;

        public Filter(String name, int count, bool active = false)
        {
            Name = name;
            Count = count;
            Active = active;
        }

        public override String ToString()
        {
            return Description;
        }

        public String Name
        {
            get { return _name; }
            set { if (SetProperty(ref _name, value)) OnPropertyChanged("Description"); }
        }

        public int Count
        {
            get { return _count; }
            set { if (SetProperty(ref _count, value)) OnPropertyChanged("Description"); }
        }

        public bool Active
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }

        public String Description
        {
            get { return String.Format("{0} ({1})", _name, _count); }
        }
    }
}

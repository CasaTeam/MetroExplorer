using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace MetroExplorer.core.Objects
{
    public class ExplorerItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                NotifyPropertyChanged("ImagePath");
            }
        }

        private double width = 0;
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                NotifyPropertyChanged("Width");
            }
        }

        private double height = 0;
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                NotifyPropertyChanged("Height");
            }
        }

        private ExplorerItemType type;
        public ExplorerItemType Type
        {
            get { return type; }
            set
            {
                type = value;
                NotifyPropertyChanged("Type");
            }
        }

        [XmlIgnore]
        private StorageFolder storageFolder;
        [XmlIgnore]
        public StorageFolder StorageFolder
        {
            get { return storageFolder; }
            set
            {
                storageFolder = value;
                NotifyPropertyChanged("StorageFolder");
            }
        }

        [XmlIgnore]
        private StorageFile storageFile;
        [XmlIgnore]
        public StorageFile StorageFile
        {
            get { return storageFile; }
            set
            {
                storageFile = value;
                NotifyPropertyChanged("StorageFile");
            }
        }

        [XmlIgnore]
        private BitmapImage image;
        [XmlIgnore]
        public BitmapImage Image
        {
            get { return image; }
            set
            {
                image = value;
                NotifyPropertyChanged("Image");
            }
        }

        public void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }
    }
}

namespace MetroExplorer.core.Objects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using Windows.Storage;
    using Windows.UI.Xaml.Media.Imaging;

    public class ExplorerItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _path;
        private double _width;
        private double _height;
        private ExplorerItemType _type;
        private ulong _size;
        private DateTime _modifiedDateTime;
        [XmlIgnore]
        private StorageFolder _storageFolder;
        [XmlIgnore]
        private StorageFile _storageFile;
        [XmlIgnore]
        private BitmapImage _image;
        [XmlIgnore]
        private string _textWrap = "Wrap";
        [XmlIgnore]
        public string _imageStretch = "None";

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                NotifyPropertyChanged("ImagePath");
            }
        }
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                NotifyPropertyChanged("Width");
            }
        }
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                NotifyPropertyChanged("Height");
            }
        }
        public ExplorerItemType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                NotifyPropertyChanged("Type");
            }
        }
        public ulong Size
        {
            get { return _size; }
            set
            {
                _size = value;
                NotifyPropertyChanged("Size");
            }
        }
        public DateTime ModifiedDateTime
        {
            get { return _modifiedDateTime; }
            set
            {
                _modifiedDateTime = value;
                NotifyPropertyChanged("ModifiedDateTime");
            }
        }
        [XmlIgnore]
        public StorageFolder StorageFolder
        {
            get { return _storageFolder; }
            set
            {
                _storageFolder = value;
                NotifyPropertyChanged("StorageFolder");
            }
        }
        [XmlIgnore]
        public StorageFile StorageFile
        {
            get { return _storageFile; }
            set
            {
                _storageFile = value;
                NotifyPropertyChanged("StorageFile");
            }
        }

        [XmlIgnore]
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                NotifyPropertyChanged("Image");
            }
        }

        [XmlIgnore]
        public string TextWrap
        {
            get { return _textWrap; }
            set
            {
                _textWrap = value;
                NotifyPropertyChanged("TextWrap");
            }
        }

        [XmlIgnore]
        public string ImageStretch
        {
            get { return _imageStretch; }
            set
            {
                _imageStretch = value;
                NotifyPropertyChanged("ImageStretch");
            }
        }

        public int LastImageIndex = -1;
        public List<string> LastImageName = new List<string>();

        public void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }
    }
}

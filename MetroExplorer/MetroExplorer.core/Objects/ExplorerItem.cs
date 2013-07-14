namespace MetroExplorer.Core.Objects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using Windows.Storage;
    using Windows.UI.Xaml.Media.Imaging;

    /// <summary>
    /// 这个类适用PageExplorer部分的数据结构
    /// </summary>
    public class ExplorerItem : INotifyPropertyChanged, IResizable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _path;
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
        public string ImageStretch
        {
            get { return _imageStretch; }
            set
            {
                _imageStretch = value;
                NotifyPropertyChanged("ImageStretch");
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }
    }

    public class HomeItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _path;
        [XmlIgnore]
        private StorageFolder _storageFolder;
        [XmlIgnore]
        private BitmapImage _image;
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
        public string ImageStretch
        {
            get { return _imageStretch; }
            set
            {
                _imageStretch = value;
                NotifyPropertyChanged("ImageStretch");
            }
        }

        public string SubImageName = "";

        public void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }

        private string _ifImageChanged = "Visible";
        public string IfImageChanged
        {
            get { return _ifImageChanged; }
            set
            {
                _ifImageChanged = value;
                NotifyPropertyChanged("IfImageChanged");
            }
        }

        private BitmapImage _defautImage;
        public BitmapImage DefautImage
        {
            get { return _defautImage; }
            set
            {
                _defautImage = value;
                NotifyPropertyChanged("DefautImage");
            }
        }
    }
}

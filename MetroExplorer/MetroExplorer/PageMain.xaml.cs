using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MetroExplorer.Common;
using System.Collections.ObjectModel;
using MetroExplorer.core.Objects;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRTXamlToolkit.Controls.Extensions;

// Pour en savoir plus sur le modèle d'élément Page Éléments groupés, consultez la page http://go.microsoft.com/fwlink/?LinkId=234231

namespace MetroExplorer
{
    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>
    public sealed partial class PageMain : LayoutAwarePage, INotifyPropertyChanged
    {
        private ObservableCollection<GroupInfoList<ExplorerItem>> _explorerItems;
        public ObservableCollection<GroupInfoList<ExplorerItem>> ExplorerItems
        {
            get { return _explorerItems; }
            set
            {
                _explorerItems = value;
                NotifyPropertyChanged("ExplorerItems");
            }
        }

        public PageMain()
        {
            this.InitializeComponent();
            DataContext = this;
            ExplorerItems = new ObservableCollection<GroupInfoList<ExplorerItem>>();

            this.Loaded += PageMain_Loaded;
        }

        void PageMain_Loaded(object sender, RoutedEventArgs e)
        {
            GroupInfoList<ExplorerItem> groupInfoList = new GroupInfoList<ExplorerItem>();
            
            groupInfoList.Key = "系统磁盘及文件库";
            for (int i = 0; i < 50; i++)
            {
                groupInfoList.Add(new ExplorerItem()
                {
                    Name = KnownFolders.PicturesLibrary.Name,
                    Path = KnownFolders.PicturesLibrary.Path,
                    StorageFolder = KnownFolders.PicturesLibrary,
                    Type = ExplorerItemType.Folder
                });
                groupInfoList.Add(new ExplorerItem()
                {
                    Name = KnownFolders.MusicLibrary.Name,
                    Path = KnownFolders.MusicLibrary.Path,
                    StorageFolder = KnownFolders.MusicLibrary,
                    Type = ExplorerItemType.Folder
                });
                groupInfoList.Add(new ExplorerItem()
                {
                    Name = KnownFolders.DocumentsLibrary.Name,
                    Path = KnownFolders.DocumentsLibrary.Path,
                    StorageFolder = KnownFolders.DocumentsLibrary,
                    Type = ExplorerItemType.Folder
                });
                groupInfoList.Add(new ExplorerItem()
                {
                    Name = KnownFolders.VideosLibrary.Name,
                    Path = KnownFolders.VideosLibrary.Path,
                    StorageFolder = KnownFolders.VideosLibrary,
                    Type = ExplorerItemType.Folder
                });
            }
            ExplorerItems.Add(groupInfoList);

            ScrollViewer myScrollViewer = itemGridView.GetFirstDescendantOfType<ScrollViewer>();
            myScrollViewer.ViewChanged += myScrollViewer_ViewChanged;
        }

        void myScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            double offset = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).HorizontalOffset;
            double scroll = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).ScrollableWidth;
            double viewportwidth = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).ViewportWidth;
            var delta = offset * 90;
            //var delta = (offset / scroll) * (Image_Background.ActualWidth - viewportwidth);
            Image_Background.Margin = new Thickness(-delta, 0, 0, 0);

        }

        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="navigationParameter">Valeur de paramètre passée à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page.
        /// </param>
        /// <param name="pageState">Dictionnaire d'état conservé par cette page durant une session
        /// antérieure. Null lors de la première visite de la page.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: assignez une collection de groupes pouvant être liés à this.DefaultViewModel["Groups"]
        }

        private void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class GroupInfoList<T> : ObservableCollection<T>
    {
        public string Key { get; set; }

        public new IEnumerator<T> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<T>)base.GetEnumerator();
        }
    }
}

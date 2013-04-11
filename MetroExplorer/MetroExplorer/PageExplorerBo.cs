namespace MetroExplorer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.Storage.FileProperties;
    using Windows.Storage.Pickers;
    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media.Imaging;
    using core;
    using core.Objects;
    using core.Utils;

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class PageExplorer
    {
        DispatcherTimer _imageChangingDispatcher = new DispatcherTimer();

        private void InitializeChangingDispatcher()
        {
            _imageChangingDispatcher.Tick += ImageChangingDispatcher_Tick;
            _imageChangingDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _imageChangingDispatcher.Start();
        }

        int _loadingImageCount;
        int _loadingFolderImageCount;
        bool _imageDispatcherLock;
        async void ImageChangingDispatcher_Tick(object sender, object e)
        {
            if (_imageDispatcherLock == false && ExplorerGroups != null)
            {
                _imageDispatcherLock = true;
                if (ExplorerGroups != null && ExplorerGroups[1] != null && _loadingImageCount < ExplorerGroups[1].Count)
                {
                    for (int i = 1; i % 20 != 0 && ExplorerGroups != null && _loadingImageCount < ExplorerGroups[1].Count; i++)
                    {
                        await ThumbnailPhoto(ExplorerGroups[1][_loadingImageCount], ExplorerGroups[1][_loadingImageCount].StorageFile);
                        _loadingImageCount++;
                    }
                }
                else
                {
                    _imageChangingDispatcher.Interval = new TimeSpan(0, 0, 0, 2);
                }
                await ChangeFolderCover();
                _imageDispatcherLock = false;
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        private async Task ChangeFolderCover()
        {
            try
            {
                if (_loadingFolderImageCount % 7 == 0 && ExplorerGroups != null && ExplorerGroups[0] != null)
                {
                    for (int i = 0; ExplorerGroups != null && ExplorerGroups[0] != null && i < ExplorerGroups[0].Count; i++)
                    {
                        var sdf = (await ExplorerGroups[0][i].StorageFolder.GetFilesAsync()).Where(p => p.Name.ToUpper().EndsWith(".JPG") || p.Name.ToUpper().EndsWith(".JPEG")
                                        || p.Name.ToUpper().EndsWith(".PNG") || p.Name.ToUpper().EndsWith(".BMP")).ToList();
                        if (sdf != null && sdf.Any())
                        {
                            await ThumbnailPhoto(ExplorerGroups[0][i], sdf[(new Random()).Next(sdf.Count)]);
                        }
                    }
                }
                _loadingFolderImageCount = ++_loadingFolderImageCount % 7;
            }
            catch
            { }
        }

        private async void AddNewItem(GroupInfoList<ExplorerItem> itemList, IStorageItem retrievedItem)
        {
            ExplorerItem item = new ExplorerItem()
            {
                Name = retrievedItem.Name,
                Path = retrievedItem.Path
            };
            if (retrievedItem is StorageFolder)
            {
                item.StorageFolder = retrievedItem as StorageFolder;
                item.Type = ExplorerItemType.Folder;
            }
            else if (retrievedItem is StorageFile)
            {
                item.StorageFile = retrievedItem as StorageFile;
                item.Type = ExplorerItemType.File;
                item.Size = (await item.StorageFile.GetBasicPropertiesAsync()).Size;
                item.ModifiedDateTime = (await item.StorageFile.GetBasicPropertiesAsync()).DateModified.DateTime;
            }
            if (itemList.All(p => p.Name != item.Name))
                itemList.Add(item);
        }

        private async Task ThumbnailPhoto(ExplorerItem item, StorageFile sf)
        {
            if (item == null) return;
            StorageItemThumbnail fileThumbnail = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 250);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            item.Image = bitmapImage;
        }

        private void ExplorerItemImage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void Button_RemoveDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItems == null || itemGridView.SelectedItems.Count == 0) return;
            while (itemGridView.SelectedItems.Count > 0)
            {
                if (ExplorerGroups[0].Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                {
                    await (itemGridView.SelectedItems[0] as ExplorerItem).StorageFolder.DeleteAsync();
                    ExplorerGroups[0].Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                }
                else if (ExplorerGroups[1].Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                {
                    await (itemGridView.SelectedItems[0] as ExplorerItem).StorageFile.DeleteAsync();
                    ExplorerGroups[1].Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                }
            }
            await InitializeNavigator();
            BottomAppBar.IsOpen = false;
        }

        private void Button_RenameDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItems.Count == 1)
            {
                (itemGridView.SelectedItem as ExplorerItem).RenameBoxVisibility = "Visible";
            }
        }

        private void AppBar_BottomAppBar_Opened_1(object sender, object e)
        {
            Button_RenameDiskFolder.Visibility = itemGridView.SelectedItems.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
            Button_RemoveDiskFolder.Visibility = itemGridView.SelectedItems.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void ItemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ExplorerItem item = e.ClickedItem as ExplorerItem;
            if (item.Type == ExplorerItemType.Folder)
            {
                //this.Frame.Navigate(typeof(PageExplorer), item.StorageFolder);
                _navigatorStorageFolders.Add(item.StorageFolder);
                Frame.Navigate(typeof(PageExplorer), _navigatorStorageFolders);
            }
            else if (item.Type == ExplorerItemType.File)
            {
                if (item.StorageFile != null && item.StorageFile.IsImageFile())
                {
                    Frame.Navigate(typeof(PhotoGallery), new Object[] { _navigatorStorageFolders, item.StorageFile });
                }
                else
                {
                    var file = await StorageFile.GetFileFromPathAsync(item.Path);
                    await file.OpenAsync(FileAccessMode.Read);
                    EventLogger.onActionEvent(EventLogger.FILE_OPENED);
                    await Launcher.LaunchFileAsync(file, new LauncherOptions { DisplayApplicationPicker = true });

                }
            }
        }

        private  void Button_AddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            Popup_CreateNewFolder.IsOpen = true;
            Popup_CreateNewFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_CreateNewFolder.Margin = new Thickness(0, 0, 555, 222);
            Popup_CreateNewFolder.IsLightDismissEnabled = true;
            TextBox_CreateNewFolder.Focus(Windows.UI.Xaml.FocusState.Keyboard);
            TextBox_CreateNewFolder.SelectAll();
        }

        private async void Button_CreateNewFolder_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder sf = await _currentStorageFolder.CreateFolderAsync(StringResources.ResourceLoader.GetString("String_NewFolder"), CreationCollisionOption.GenerateUniqueName);
            ExplorerItem item = new ExplorerItem()
            {
                Name = sf.Name,
                Path = sf.Path,
                Type = ExplorerItemType.Folder,
                RenameBoxVisibility = "Collapsed",
                StorageFolder = sf
            };
            ExplorerGroups[0].Add(item);
            itemGridView.SelectedItem = item;
            Popup_CreateNewFolder.IsOpen = false;
            Popup_CreateNewFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BottomAppBar.IsOpen = false;
            await InitializeNavigator();
        }

        private void ItemGridView_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ItemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            foreach (var selectedItem in e.RemovedItems)
            {
                if ((selectedItem as ExplorerItem).RenameBoxVisibility == "Visible")
                    (selectedItem as ExplorerItem).RenameBoxVisibility = "Collapsed";
            }
            foreach (var selectedItem in itemGridView.SelectedItems)
            {
                if (itemGridView.SelectedItems.Count > 1 && (selectedItem as ExplorerItem).RenameBoxVisibility == "Visible")
                    (selectedItem as ExplorerItem).RenameBoxVisibility = "Collapsed";
            }
            if (itemGridView.SelectedItems.Count == 1 && (itemGridView.SelectedItems[0] as ExplorerItem).RenameBoxVisibility == "Visible")
                BottomAppBar.IsOpen = false;
            else if (itemGridView.SelectedItems.Count > 0)
                BottomAppBar.IsOpen = true;
        }

        private void Button_CancelRename_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItem != null)
            {
                (itemGridView.SelectedItem as ExplorerItem).RenameBoxVisibility = "Collapsed";
            }
        }

        private async void Button_RenameFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItem != null)
            {
                (itemGridView.SelectedItem as ExplorerItem).Name = (itemGridView.SelectedItem as ExplorerItem).RenamingName;
                (itemGridView.SelectedItem as ExplorerItem).RenameBoxVisibility = "Collapsed";
                if ((itemGridView.SelectedItem as ExplorerItem).Type == ExplorerItemType.Folder)
                    await (itemGridView.SelectedItem as ExplorerItem).StorageFolder.RenameAsync((itemGridView.SelectedItem as ExplorerItem).RenamingName, NameCollisionOption.GenerateUniqueName);
                else if ((itemGridView.SelectedItem as ExplorerItem).Type == ExplorerItemType.File)
                    await (itemGridView.SelectedItem as ExplorerItem).StorageFile.RenameAsync((itemGridView.SelectedItem as ExplorerItem).RenamingName, NameCollisionOption.GenerateUniqueName);
            }
            await InitializeNavigator();
        }

        private void ExplorerItemImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //(sender as Image).FadeOut();
            (sender as Image).FadeInCustom(new TimeSpan(0, 0, 0, 1, 500));
        }

        private Boolean IsImageFile(StorageFile file)
        {
            if (file.FileType.ToUpper().Equals(".JPG") ||
                file.FileType.ToUpper().Equals(".JPEG") ||
                file.FileType.ToUpper().Equals(".PNG") ||
                file.FileType.ToUpper().Equals(".BMP"))
                return true;
            return false;
        }
    }

    /// <summary>
    /// Bottom App bar right buttons
    /// </summary>
    public sealed partial class PageExplorer
    {
        #region cutcopypaste
        private void Button_CutPaste_Click(object sender, RoutedEventArgs e)
        {
            Popup_CopyCutPaste.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_CopyCutPaste.Margin = new Thickness(0, 0, 405, 183);
            Popup_CopyCutPaste.IsLightDismissEnabled = true;
            Popup_CopyCutPaste.IsOpen = true;

        }

        private async void ListBox_CopyCutPaste_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) return;
            Popup_CopyCutPaste.IsOpen = false;
            Popup_CopyCutPaste.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if (ListBox_CopyCutPaste.Items.IndexOf((sender as ListBox).SelectedItem) == 0)
                await CopyFile();
            else if (ListBox_CopyCutPaste.Items.IndexOf((sender as ListBox).SelectedItem) == 1)
                await CutFile();
            ListBox_CopyCutPaste.SelectedItem = null;
            RefreshAfterAddNewItem();
        }

        private async void RefreshAfterAddNewItem()
        {
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            await RefreshLocalFiles();
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async Task CopyFile()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.FileTypeFilter.Add("*");
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var sf = await filePicker.PickMultipleFilesAsync();
            for (int i = 0; i < sf.Count; i++)
            {
                await sf[i].CopyAsync(_currentStorageFolder, sf[i].Name, NameCollisionOption.GenerateUniqueName);
            }
        }

        private async Task CutFile()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.FileTypeFilter.Add("*");
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var sf = await filePicker.PickMultipleFilesAsync();
            for (int i = 0; i < sf.Count; i++)
            {
                await sf[i].CopyAsync(_currentStorageFolder, sf[i].Name, NameCollisionOption.GenerateUniqueName);
                await sf[i].DeleteAsync(StorageDeleteOption.Default);
            }
            GC.Collect();
        }
        #endregion

        private void Button_Detail_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.ItemTemplate == this.Resources["Standard300x80ItemTemplate"] as DataTemplate)
            {
                itemGridView.ItemTemplate = this.Resources["Standard300x180ItemTemplate"] as DataTemplate;
                PageExplorer.BigSquareMode = true;
            }
            else
            {
                itemGridView.ItemTemplate = this.Resources["Standard300x80ItemTemplate"] as DataTemplate;
                PageExplorer.BigSquareMode = false;
            }
        }

        #region sort
        public static SortType CurrentFileListSortType = SortType.None; 

        private void Button_Sort_Click(object sender, RoutedEventArgs e)
        {
            Popup_Sort.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_Sort.Margin = new Thickness(0, 0, 151, 270);
            Popup_Sort.IsLightDismissEnabled = true;
            Popup_Sort.IsOpen = true;
        }

        private void ListBox_Sorte_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) return;
            Popup_Sort.IsOpen = false;
            Popup_Sort.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if(ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 0)
                SortItems(SortType.Date);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 1)
                SortItems(SortType.Name);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 2)
                SortItems(SortType.Size);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 3)
                SortItems(SortType.Type);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 4)
                SortItems(SortType.None);
            ListBox_CopyCutPaste.SelectedItem = null;
        }

        private void SortItems(SortType sortType)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource = null;
            if (sortType == SortType.Date)
                sortedSource = SortByDate(ExplorerGroups[1] as IEnumerable<ExplorerItem>);
            else if (sortType == SortType.Name)
                sortedSource = SortByName(ExplorerGroups[1] as IEnumerable<ExplorerItem>);
            else if (sortType == SortType.Size)
                sortedSource = SortBySize(ExplorerGroups[1] as IEnumerable<ExplorerItem>);
            else if (sortType == SortType.Type)
                sortedSource = SortByType(ExplorerGroups[1] as IEnumerable<ExplorerItem>);
            else if (sortType == SortType.None)
                return;
            if (sortedSource != null)
                RerangeDataSource(sortedSource);
        }

        private IOrderedEnumerable<ExplorerItem> SortByDate(IEnumerable<ExplorerItem> items)
        {
            var sortedSource = items.OrderByDescending(p => p.ModifiedDateTime);
            PageExplorer.CurrentFileListSortType = SortType.Date;
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByName(IEnumerable<ExplorerItem> items)
        {
            var sortedSource = items.OrderByDescending(p => p.Name);
            PageExplorer.CurrentFileListSortType = SortType.Name;
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortBySize(IEnumerable<ExplorerItem> items)
        {
            var sortedSource = items.OrderByDescending(p => p.Size);
            PageExplorer.CurrentFileListSortType = SortType.Size;
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByType(IEnumerable<ExplorerItem> items)
        {
            var sortedSource = items.OrderByDescending(p => p.Name.Split(new string[] { "." }, StringSplitOptions.None).Last());
            PageExplorer.CurrentFileListSortType = SortType.Type;
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByNone(IEnumerable<ExplorerItem> items)
        {
            var sortedSource = items as IOrderedEnumerable<ExplorerItem>;
            PageExplorer.CurrentFileListSortType = SortType.None;
            return sortedSource;
        }

        private void RerangeDataSource(IOrderedEnumerable<ExplorerItem> sortedSource)
        {
            if (ExplorerGroups == null || ExplorerGroups[1] == null) return;
            List<ExplorerItem> sortedItems = new List<ExplorerItem>();
            foreach (var item in sortedSource)
            {
                sortedItems.Add(item);
            }
            ExplorerGroups[1].Clear();
            foreach (var item in sortedItems)
            {
                ExplorerGroups[1].Add(item);
            }
        }
        #endregion
    }

    public enum SortType
    { 
        Date,
        Name,
        Size,
        Type,
        None
    }
}
﻿namespace MetroExplorer
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
            _imageChangingDispatcher.Interval = new TimeSpan(0, 0, 0, 1, 500);
            _imageChangingDispatcher.Start();
        }

        int _loadingImageCount;
        //int _loadingFolderImageCount;
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
                        var file = ExplorerGroups[1][_loadingImageCount].StorageFile;
                        await ThumbnailPhoto(ExplorerGroups[1][_loadingImageCount], file, true);
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
        
        private async void ItemGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //if (ExplorerGroups[1].Count > 0)
            //{
            //    var item = ExplorerGroups[1].Single(p => p.Name == (((sender as Grid).Children[2] as Grid).Children[0] as TextBlock).Text);
            //    if (item != null && item.DefautImage == null)
            //        await ThumbnailPhoto(item, item.StorageFile, true);
            //}
        }

        int _lastChangedFolder = 0;
        private async Task ChangeFolderCover()
        {
            try
            {
                if (_lastChangedFolder == ExplorerGroups[0].Count)
                    _lastChangedFolder = 0;
                _lastChangedFolder++;
                var exploreItem = ExplorerGroups[0][_lastChangedFolder];
                if (exploreItem.StorageFolder == null) return;
                var files = await exploreItem.StorageFolder.GetFilesAsync();
                if (exploreItem.LastImageIndex == -2) return;
                if (exploreItem.LastImageIndex == -1)
                    foreach (var file in files)
                    {
                        exploreItem.LastImageIndex = -2;
                        if (file.Name.ToUpper().EndsWith(".PNG") || file.Name.ToUpper().EndsWith(".JPG") || file.Name.ToUpper().EndsWith(".JPEG") ||
                            file.Name.ToUpper().EndsWith(".BMP") || file.Name.ToUpper().EndsWith(".RMVB") || file.Name.ToUpper().EndsWith(".MP4") ||
                            file.Name.ToUpper().EndsWith(".MKV") || file.Name.ToUpper().EndsWith(".PNG"))
                        {
                            exploreItem.LastImageName.Add(file.Name);
                            exploreItem.LastImageIndex = 0;
                        }
                    }
                if (exploreItem.LastImageIndex == exploreItem.LastImageName.Count - 1)
                    exploreItem.LastImageIndex = 0;
                await ThumbnailPhoto(exploreItem, files[exploreItem.LastImageIndex]);
                exploreItem.LastImageIndex++;
            }
            catch
            { }
        }

        private async Task ThumbnailPhoto(ExplorerItem item, StorageFile sf, bool file = false)
        {
            if (item == null) return;
            
            StorageItemThumbnail fileThumbnail = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 250);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            if (file == false)
                item.Image = bitmapImage;
            else
                item.DefautImage = bitmapImage;
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
                _dataSource.NavigatorStorageFolders.Add(item.StorageFolder);
                Frame.Navigate(typeof(PageExplorer), null);
            }
            else if (item.Type == ExplorerItemType.File)
            {
                if (item.StorageFile != null && item.StorageFile.IsImageFile())
                {
                    var parameters = ExplorerGroups[1].Where(p=> p.StorageFile.FileType.ToUpper().Equals(".JPG") ||
                                                                 p.StorageFile.FileType.ToUpper().Equals(".JPEG") ||
                                                                 p.StorageFile.FileType.ToUpper().Equals(".PNG") ||
                                                                 p.StorageFile.FileType.ToUpper().Equals(".BMP")).ToList();
                    parameters.Remove(item);
                    parameters.Insert(0, item);
                    Frame.Navigate(typeof(PhotoGallery), parameters);
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

        private void Button_AddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            Popup_CreateNewFolder.IsOpen = true;
            Popup_CreateNewFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_CreateNewFolder.Margin = new Thickness(0, 0, 555, 222);
            TextBox_CreateNewFolder.Focus(Windows.UI.Xaml.FocusState.Keyboard);
            TextBox_CreateNewFolder.SelectAll();
        }

        private async void Button_CreateNewFolder_Click(object sender, RoutedEventArgs e)
        {
            //StorageFolder sf = await _currentStorageFolder.CreateFolderAsync(StringResources.ResourceLoader.GetString("String_NewFolder"), CreationCollisionOption.GenerateUniqueName);
            StorageFolder sf = await _dataSource.CurrentStorageFolder.CreateFolderAsync(TextBox_CreateNewFolder.Text, CreationCollisionOption.GenerateUniqueName);
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
                //await sf[i].CopyAsync(_currentStorageFolder, sf[i].Name, NameCollisionOption.GenerateUniqueName);
                await sf[i].CopyAsync(_dataSource.CurrentStorageFolder, sf[i].Name, NameCollisionOption.GenerateUniqueName);
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
                //await sf[i].CopyAsync(_currentStorageFolder, sf[i].Name, NameCollisionOption.GenerateUniqueName);
                await sf[i].CopyAsync(_dataSource.CurrentStorageFolder, sf[i].Name, NameCollisionOption.GenerateUniqueName);
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
        private Dictionary<SortType, SortOrderType> _sortedRecoder = new Dictionary<SortType, SortOrderType>();

        private void Button_Sort_Click(object sender, RoutedEventArgs e)
        {
            Popup_Sort.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_Sort.Margin = new Thickness(0, 0, 151, 270);
            Popup_Sort.IsOpen = true;
        }

        private void ListBox_Sorte_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) return;
            Popup_Sort.IsOpen = false;
            Popup_Sort.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 0)
                SortItems(SortType.Date);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 1)
                SortItems(SortType.Name);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 2)
                SortItems(SortType.Size);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 3)
                SortItems(SortType.Type);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 4)
                SortItems(SortType.None);
            ListBox_Sorte.SelectedItem = null;
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
            IOrderedEnumerable<ExplorerItem> sortedSource;
            if (_sortedRecoder.Keys.Contains(SortType.Date))
            {
                if (_sortedRecoder[SortType.Date] == SortOrderType.Ascend)
                {
                    sortedSource = items.OrderByDescending(p => p.ModifiedDateTime);
                    _sortedRecoder[SortType.Date] = SortOrderType.Descend;
                }
                else
                {
                    sortedSource = items.OrderBy(p => p.ModifiedDateTime);
                    _sortedRecoder[SortType.Date] = SortOrderType.Ascend;
                }
            }
            else
            {
                sortedSource = items.OrderBy(p => p.ModifiedDateTime);
                _sortedRecoder.Add(SortType.Date, SortOrderType.Ascend);
            }
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByName(IEnumerable<ExplorerItem> items)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource;
            if (_sortedRecoder.Keys.Contains(SortType.Name))
            {
                if (_sortedRecoder[SortType.Name] == SortOrderType.Ascend)
                {
                    sortedSource = items.OrderByDescending(p => p.Name);
                    _sortedRecoder[SortType.Name] = SortOrderType.Descend;
                }
                else
                {
                    sortedSource = items.OrderBy(p => p.Name);
                    _sortedRecoder[SortType.Name] = SortOrderType.Ascend;
                }
            }
            else
            {
                sortedSource = items.OrderBy(p => p.Name);
                _sortedRecoder.Add(SortType.Name, SortOrderType.Ascend);
            }
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortBySize(IEnumerable<ExplorerItem> items)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource;
            if (_sortedRecoder.Keys.Contains(SortType.Size))
            {
                if (_sortedRecoder[SortType.Size] == SortOrderType.Ascend)
                {
                    sortedSource = items.OrderByDescending(p => p.Size);
                    _sortedRecoder[SortType.Size] = SortOrderType.Descend;
                }
                else
                {
                    sortedSource = items.OrderBy(p => p.Size);
                    _sortedRecoder[SortType.Size] = SortOrderType.Ascend;
                }
            }
            else
            {
                sortedSource = items.OrderBy(p => p.Size);
                _sortedRecoder.Add(SortType.Size, SortOrderType.Ascend);
            }
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByType(IEnumerable<ExplorerItem> items)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource;
            if (_sortedRecoder.Keys.Contains(SortType.Type))
            {
                if (_sortedRecoder[SortType.Type] == SortOrderType.Ascend)
                {
                    sortedSource = items.OrderByDescending(p => p.Type);
                    _sortedRecoder[SortType.Type] = SortOrderType.Descend;
                }
                else
                {
                    sortedSource = items.OrderBy(p => p.Type);
                    _sortedRecoder[SortType.Type] = SortOrderType.Ascend;
                }
            }
            else
            {
                sortedSource = items.OrderBy(p => p.Type);
                _sortedRecoder.Add(SortType.Type, SortOrderType.Ascend);
            }
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByNone(IEnumerable<ExplorerItem> items)
        {
            var sortedSource = items as IOrderedEnumerable<ExplorerItem>;
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

    public enum SortOrderType
    {
        Ascend,
        Descend
    }
}
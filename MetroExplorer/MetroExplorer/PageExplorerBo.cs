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
    using MetroExplorer.Common;

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class PageExplorer
    {
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
                    var parameters = ExplorerGroups[1].Where(p => p.StorageFile.FileType.ToUpper().Equals(".JPG") ||
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
            Popup_CreateNewFolder.Margin = new Thickness(0, 0, 910, 222);
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
                StorageFolder = sf
            };
            ExplorerGroups[0].Add(item);
            itemGridView.SelectedItem = item;
            Popup_CreateNewFolder.IsOpen = false;
            Popup_CreateNewFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BottomAppBar.IsOpen = false;
            await InitializeNavigator();
        }

        private void ItemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
        }

        private void RenameDiskFolderButtonClick(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItem != null && itemGridView.SelectedItems.Count == 1 &&
                (itemGridView.SelectedItem as ExplorerItem).Type == ExplorerItemType.Folder)
            {
                Popup_RenameFolder.IsOpen = true;
                Popup_RenameFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Popup_RenameFolder.Margin = new Thickness(60, 0, 0, 222);
                TextBox_RenameFolder.Text = (itemGridView.SelectedItem as ExplorerItem).Name;
                TextBox_RenameFolder.Focus(Windows.UI.Xaml.FocusState.Keyboard);
                TextBox_RenameFolder.SelectAll();
            }
        }

        private async void ConfirmRenameFolderButtonClick(object sender, RoutedEventArgs e)
        {
            if ((itemGridView.SelectedItem as ExplorerItem).Name != TextBox_RenameFolder.Text)
            {
                (itemGridView.SelectedItem as ExplorerItem).Name = TextBox_RenameFolder.Text;
                await (itemGridView.SelectedItem as ExplorerItem).StorageFolder.RenameAsync(TextBox_RenameFolder.Text, NameCollisionOption.ReplaceExisting);
                await InitializeNavigator();
            }
            Popup_RenameFolder.IsOpen = false;
            Popup_RenameFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
        private void Button_Detail_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.ItemTemplate == this.Resources["Standard300x80ItemTemplate"] as DataTemplate)
            {
                itemGridView.ItemTemplate = this.Resources["Standard300x180ItemTemplate"] as DataTemplate;
                PageExplorer.BigSquareMode = true;
                _loadingImageCount = 0;
                UserPreferenceRecord.GetInstance().WriteUserPreferenceRecord("Square");
            }
            else
            {
                itemGridView.ItemTemplate = this.Resources["Standard300x80ItemTemplate"] as DataTemplate;
                PageExplorer.BigSquareMode = false;
                _loadingFileSizeCount = 0;
                UserPreferenceRecord.GetInstance().WriteUserPreferenceRecord("List");
            }
        }
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
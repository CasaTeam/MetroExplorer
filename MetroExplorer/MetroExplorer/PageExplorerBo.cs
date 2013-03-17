using MetroExplorer.core;
using MetroExplorer.core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MetroExplorer
{

    public sealed partial class PageExplorer : MetroExplorer.Common.LayoutAwarePage, INotifyPropertyChanged
    {
        DispatcherTimer imageChangingDispatcher = new DispatcherTimer();
        int folderInternalImageCount = 0;

        private void initializeChangingDispatcher()
        {
            imageChangingDispatcher.Tick += imageChangingDispatcher_Tick;
            imageChangingDispatcher.Interval = new TimeSpan(0, 0, 0, 2);
            imageChangingDispatcher.Start();
        }

        async void imageChangingDispatcher_Tick(object sender, object e)
        {
            for (int i = 0; i < ExplorerGroups[0].Count; i++)
            {
                if (ExplorerGroups[0][i].Image != null)
                {
                    int j = 0;
                    foreach (StorageFile st in await ExplorerGroups[0][i].StorageFolder.GetFilesAsync())
                    {
                        if (j < folderInternalImageCount)
                        {
                            j++;
                            continue;
                        }
                        folderInternalImageCount++;
                        if ((st.Name.ToUpper().EndsWith(".JPG") || st.Name.ToUpper().EndsWith(".JPEG") || st.Name.ToUpper().EndsWith(".PNG") ||
                             st.Name.ToUpper().EndsWith(".BMP")))
                        {
                            ExplorerGroups[0][i].Image = null;
                            await thumbnailPhoto(ExplorerGroups[0][i], st);
                            folderInternalImageCount = (folderInternalImageCount < 4) ? folderInternalImageCount++ : 0;          
                            break;
                        }
                    }
                }
            }
        }

        private void ExplorerItemImage_Loaded(object sender, RoutedEventArgs e)
        {
            //var sb = (Storyboard)ExplorerItemImageLoadingAnimationTemplate.LoadContent();
            //Storyboard.SetTarget(sb, sender as Image);
            //sb.Begin();
            (sender as Image).FadeIn();
            //(sender as Image).FadeIn(new TimeSpan(0, 0, 1));
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
            BottomAppBar.IsOpen = false;
        }

        private void Button_Detail_Click(object sender, RoutedEventArgs e)
        {

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
            if (itemGridView.SelectedItems.Count == 1)
            {
                Button_RenameDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                Button_RenameDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            if (itemGridView.SelectedItems.Count == 0)
            {
                Button_RemoveDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                Button_RemoveDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private async void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ExplorerItem item = e.ClickedItem as ExplorerItem;
            if(item.Type == ExplorerItemType.Folder)
                this.Frame.Navigate(typeof(PageExplorer), item.StorageFolder);
            else if (item.Type == ExplorerItemType.File)
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(item.Path);
                var targetStream = await file.OpenAsync(FileAccessMode.Read);
                await Launcher.LaunchFileAsync(file, new LauncherOptions { DisplayApplicationPicker = true });
            }
        }

        private async void Button_AddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder sf = await currentStorageFolder.CreateFolderAsync(StringResources.ResourceLoader.GetString("String_NewFolder"), CreationCollisionOption.GenerateUniqueName);
            ExplorerItem item = new ExplorerItem()
            {
                Name = sf.Name,
                RenamingName = sf.Name,
                Path = sf.Path,
                Type = ExplorerItemType.Folder,
                RenameBoxVisibility = "Visible",
                StorageFolder = sf
            };
            ExplorerGroups[0].Add(item);
            itemGridView.SelectedItem = item;
        }

        private void itemGridView_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void itemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
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
            else
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
        }

        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExplorerItemImage_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

        }

        private void ExplorerItemImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            (sender as Image).FadeIn(new TimeSpan(0,0,0,0,500));
        }

        private void ExplorerItemImage_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}

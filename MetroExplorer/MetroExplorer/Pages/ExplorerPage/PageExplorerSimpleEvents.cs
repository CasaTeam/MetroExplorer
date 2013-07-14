namespace MetroExplorer.Pages.ExplorerPage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Resources;
    using Windows.Storage;
    using Windows.Storage.FileProperties;
    using Windows.Storage.Pickers;
    using Windows.System;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media.Imaging;
    using Core;
    using Core.Objects;
    using Core.Utils;
    using UserPreferenceRecord;
    using MainPage;
    public sealed partial class PageExplorer
    {
        private void Button_PlayFolder_Click(object sender, RoutedEventArgs e)
        {
            PhotoGallery.ActualScreenHeight = this.ActualHeight;
            Frame.Navigate(typeof(PhotoGallery), ExplorerItems.ToList());
        }

        #region Home Button
        private void HomeButtonImage_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            HomeCircleBackgroundEllipse.Opacity = 0.6;
        }

        private void HomeButtonImage_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            HomeCircleBackgroundEllipse.Opacity = 0;
        }

        private void HomeButtonImage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            HomeCircleBackgroundEllipse.Opacity = 1;
        }

        private void HomeButtonImage_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            HomeCircleBackgroundEllipse.Opacity = 0;
            DataSource.NavigatorStorageFolders = new List<StorageFolder>();
            StopImageChangingDispatcher();
            CurrentItems = null;
            Frame.Navigate(typeof(PageMain));
        }
        #endregion

        private async void Button_RemoveDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog confrimMsg = new MessageDialog(StringResources.ResourceLoader.GetString("RemoveFolderConfirmationContentString"), StringResources.ResourceLoader.GetString("RemoveFolderConfirmationTitleString"));
            confrimMsg.Commands.Add(new UICommand(StringResources.ResourceLoader.GetString("RemoveFolderConfirmationYesButtonString"), async (UICommandInvokedHandler) =>
            {
                MessageDialog msg = null;
                try
                {
                    if (itemGridView.SelectedItems == null || itemGridView.SelectedItems.Count == 0) return;
                    while (itemGridView.SelectedItems.Count > 0)
                    {
                        if (ExplorerItems.Contains(itemGridView.SelectedItems[0] as ExplorerItem) && (itemGridView.SelectedItems[0] as ExplorerItem).StorageFolder != null)
                        {
                            await (itemGridView.SelectedItems[0] as ExplorerItem).StorageFolder.DeleteAsync();
                            ExplorerItems.Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                        }
                        else if (ExplorerItems.Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                        {
                            await (itemGridView.SelectedItems[0] as ExplorerItem).StorageFile.DeleteAsync();
                            ExplorerItems.Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                        }
                    }
                    await InitializeNavigator();
                    BottomAppBar.IsOpen = false;
                }
                catch (Exception exp)
                {
                    msg = new MessageDialog(exp.Message + " ; " + (exp.InnerException == null ? "" : exp.InnerException.Message)
                                             + " ; " + (exp.InnerException.InnerException == null ? "" : exp.InnerException.InnerException.Message), "Remove denied");
                }
                if (msg != null)
                    await msg.ShowAsync();
            }));
            confrimMsg.Commands.Add(new UICommand(StringResources.ResourceLoader.GetString("RemoveFolderConfirmationNoButtonString")));
            await confrimMsg.ShowAsync();
        }

        private void AppBar_BottomAppBar_Opened_1(object sender, object e)
        {
            Button_RenameDiskFolder.Visibility = itemGridView.SelectedItems.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
            Button_RemoveDiskFolder.Visibility = itemGridView.SelectedItems.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageExplorer), null);
        }

    }


    /// <summary>
    /// Add New Folder
    /// </summary>
    public sealed partial class PageExplorer
    {
        private void Button_AddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            Popup_CreateNewFolder.IsOpen = true;
            Popup_CreateNewFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_CreateNewFolder.Margin = new Thickness(0, 0, 1055, 237);
            TextBox_CreateNewFolder.Focus(Windows.UI.Xaml.FocusState.Keyboard);
            TextBox_CreateNewFolder.SelectAll();
        }

        private async void Button_CreateNewFolder_Click(object sender, RoutedEventArgs e)
        {
            //StorageFolder sf = await _currentStorageFolder.CreateFolderAsync(StringResources.ResourceLoader.GetString("String_NewFolder"), CreationCollisionOption.GenerateUniqueName);
            StorageFolder sf = await DataSource.CurrentStorageFolder.CreateFolderAsync(TextBox_CreateNewFolder.Text, CreationCollisionOption.GenerateUniqueName);
            ExplorerItem item = new ExplorerItem()
            {
                Name = sf.Name,
                Path = sf.Path,
                Type = ExplorerItemType.Folder,
                StorageFolder = sf
            };
            ExplorerItems.Insert(0, item);
            itemGridView.SelectedItem = item;
            Popup_CreateNewFolder.IsOpen = false;
            Popup_CreateNewFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BottomAppBar.IsOpen = false;
            await InitializeNavigator();
        }
    }


    /// <summary>
    /// Rename file and folder
    /// </summary>
    public sealed partial class PageExplorer
    {
        private void RenameDiskFolderButtonClick(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItem != null && itemGridView.SelectedItems.Count == 1)
            {
                Popup_RenameFolder.IsOpen = true;
                Popup_RenameFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Popup_RenameFolder.Margin = new Thickness(60, 0, 0, 237);
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
                if ((itemGridView.SelectedItem as ExplorerItem).StorageFolder != null)
                    await (itemGridView.SelectedItem as ExplorerItem).StorageFolder.RenameAsync(TextBox_RenameFolder.Text, NameCollisionOption.ReplaceExisting);
                else if ((itemGridView.SelectedItem as ExplorerItem).StorageFile != null)
                    await (itemGridView.SelectedItem as ExplorerItem).StorageFile.RenameAsync(TextBox_RenameFolder.Text, NameCollisionOption.ReplaceExisting);
                await InitializeNavigator();
            }
            Popup_RenameFolder.IsOpen = false;
            Popup_RenameFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
                UserPreferenceRecord.GetInstance().WriteUserPreferenceRecord("Square");
            }
            else
            {
                itemGridView.ItemTemplate = this.Resources["Standard300x80ItemTemplate"] as DataTemplate;
                PageExplorer.BigSquareMode = false;
                UserPreferenceRecord.GetInstance().WriteUserPreferenceRecord("List");
            }
            _counterForLoadUnloadedItems = 0;
        }
    }
}
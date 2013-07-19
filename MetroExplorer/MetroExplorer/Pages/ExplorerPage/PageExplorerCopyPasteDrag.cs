namespace MetroExplorer.Pages.ExplorerPage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Core;
    using Core.Objects;

    public sealed partial class PageExplorer
    {
        /// <summary>
        /// 为了防止不能理解的第二次相应点击事件而设计的变量
        /// </summary>
        bool _isPasting = false;

        private void PageExplorerCopyButtonClick(object sender, RoutedEventArgs e)
        {
            CopiedCuttedItems.GetInstance().Items.Clear();
            if (itemGridView.SelectedItems != null && itemGridView.SelectedItems.Count > 0)
            {
                foreach (var item in itemGridView.SelectedItems)
                {
                    CopiedCuttedItems.GetInstance().Items.Add((item as ExplorerItem));
                }
                CopiedCuttedItems.GetInstance().CutOrCopy = CopyCutState.Copy;
            }
        }

        private void PageExplorerCutButtonClick(object sender, RoutedEventArgs e)
        {
            CopiedCuttedItems.GetInstance().Items.Clear();
            if (itemGridView.SelectedItems != null && itemGridView.SelectedItems.Count > 0)
            {
                foreach (var item in itemGridView.SelectedItems)
                {
                    CopiedCuttedItems.GetInstance().Items.Add((item as ExplorerItem));
                }
                CopiedCuttedItems.GetInstance().CutOrCopy = CopyCutState.Cut;
            }
        }

        private async void PageExplorerPasteButtonClick(object sender, RoutedEventArgs e)
        {
            if (_isPasting == true) return;
            _isPasting = true;
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (CopiedCuttedItems.GetInstance().Items.Count > 0)
            {
                foreach (var item in CopiedCuttedItems.GetInstance().Items)
                {
                    if (item.Type == ExplorerItemType.File)
                    {
                        try
                        {
                            await item.StorageFile.CopyAsync(DataSource.CurrentStorageFolder, item.Name, NameCollisionOption.GenerateUniqueName);
                            if (CopiedCuttedItems.GetInstance().CutOrCopy == CopyCutState.Cut)
                            {
                                if (ExplorerItems.Contains(item))
                                    ExplorerItems.Remove(item);
                                await item.StorageFile.DeleteAsync(StorageDeleteOption.Default);
                            }
                        }
                        catch
                        { }
                    }
                }
                if (CopiedCuttedItems.GetInstance().CutOrCopy == CopyCutState.Cut)
                    CopiedCuttedItems.GetInstance().Items.Clear();
            }
            else if (DataSource.ShareStorageItems.Count > 0)
            {
                foreach (IStorageItem item in DataSource.ShareStorageItems)
                    if (item is StorageFile)
                        try
                        {
                            StorageFile file = (StorageFile)item;
                            await file.CopyAsync(DataSource.CurrentStorageFolder, item.Name, NameCollisionOption.GenerateUniqueName);
                        }
                        catch
                        { }
            }
            RefreshAfterAddNewItem();
            _isPasting = false;
        }

        #region new mode

        private void Button_CutPaste_Click(object sender, RoutedEventArgs e)
        {
            Popup_CopyCutPaste.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_CopyCutPaste.Margin = new Thickness(0, 0, 470, 198);
            Popup_CopyCutPaste.IsOpen = true;
        }

        private async void ListBox_CopyCutPaste_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || _isPasting == true) return;
            _isPasting = true;
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_CopyCutPaste.IsOpen = false;
            Popup_CopyCutPaste.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if (ListBox_CopyCutPaste.Items.IndexOf((sender as ListBox).SelectedItem) == 0)
                await CopyFile();
            else if (ListBox_CopyCutPaste.Items.IndexOf((sender as ListBox).SelectedItem) == 1)
                await CutFile();
            ListBox_CopyCutPaste.SelectedItem = null;
            RefreshAfterAddNewItem();
            _isPasting = false;
        }

        private async void RefreshAfterAddNewItem()
        {
            await RefreshLocalFiles();
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
                await sf[i].CopyAsync(DataSource.CurrentStorageFolder, sf[i].Name, NameCollisionOption.GenerateUniqueName);
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
                await sf[i].CopyAsync(DataSource.CurrentStorageFolder, sf[i].Name, NameCollisionOption.GenerateUniqueName);
                await sf[i].DeleteAsync(StorageDeleteOption.Default);
            }
            GC.Collect();
        }

        #endregion
    }

    public class CopiedCuttedItems
    {
        public List<ExplorerItem> Items = null;
        public CopyCutState CutOrCopy;

        private CopiedCuttedItems()
        {
            Items = new List<ExplorerItem>();
        }

        public static CopiedCuttedItems GetInstance()
        {
            return Singleton<CopiedCuttedItems>.Instance;
        }
    }

    public enum CopyCutState
    {
        Copy,
        Cut
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroExplorer
{
    /// <summary>
    /// This partial Class is used for fixing the conflict between Search Event and other keyboard typing Events.
    /// ToDo: We should think a better way to manage when should the ShowOnKeyboardInput disabled.
    /// </summary>
    public sealed partial class PageExplorer
    {
        private void Popup_CreateNewFolder_Closed(object sender, object e)
        {
            _searchPane.ShowOnKeyboardInput = true;
        }

        private void Popup_CreateNewFolder_Opened(object sender, object e)
        {
            _searchPane.ShowOnKeyboardInput = false;
        }

        private void Popup_RenameFolder_Opened(object sender, object e)
        {
            _searchPane.ShowOnKeyboardInput = false;
        }

        private void Popup_RenameFolder_Closed(object sender, object e)
        {
            _searchPane.ShowOnKeyboardInput = true;
        }
    }
}

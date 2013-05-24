using MetroExplorer.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroExplorer
{
    public class PageExplorerFileTypes
    {
        public Dictionary<string, FileTypeEnum> FileNameType = new Dictionary<string, FileTypeEnum>();
        public Dictionary<FileTypeEnum, string> FileTypeImagePath = new Dictionary<FileTypeEnum, string>();

        private PageExplorerFileTypes()
        {
            FileTypeImagePath.Add(FileTypeEnum.Image, "Assets/FilesIcon/appbar.image.png");
            FileTypeImagePath.Add(FileTypeEnum.Music, "Assets/FilesIcon/appbar.music.png");
            FileTypeImagePath.Add(FileTypeEnum.Video, "Assets/FilesIcon/appbar.film.png");
            FileTypeImagePath.Add(FileTypeEnum.OfficeWord, "Assets/FilesIcon/appbar.page.word.png");
            FileTypeImagePath.Add(FileTypeEnum.OfficeExcel, "Assets/FilesIcon/appbar.page.excel.png");
            FileTypeImagePath.Add(FileTypeEnum.OfficePowerPoint, "Assets/FilesIcon/appbar.page.powerpoint.png");
            FileTypeImagePath.Add(FileTypeEnum.Pdf, "Assets/FilesIcon/appbar.page.word.png");
            FileTypeImagePath.Add(FileTypeEnum.XmlHtmlXamlEtc, "Assets/FilesIcon/appbar.page.xml.png");
            FileTypeImagePath.Add(FileTypeEnum.Pdf, "Assets/FilesIcon/appbar.page.word.png");
            FileTypeImagePath.Add(FileTypeEnum.Pdf, "Assets/FilesIcon/appbar.page.word.png");
        }

        public static PageExplorerFileTypes Instance()
        {
            return Singleton<PageExplorerFileTypes>.Instance;
        }
    }

    public enum FileTypeEnum
    {
        Image,
        Video,
        OfficeWord,
        OfficePowerPoint,
        OfficeOneNote,
        OfficeExcel,
        Pdf,
        Txt,
        Java,
        CSharp,
        Music,
        Exe,
        Dll,
        XmlHtmlXamlEtc
    }
}

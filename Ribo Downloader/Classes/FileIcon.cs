using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Ribo_Downloader.Classes
{
    public static class FileIcon
    {
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        private const uint SHGFI_SMALLICON = 0x1; // 'Small icon
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;

        public static System.Drawing.Icon GetLargeIcon(string file)
        {
            FileIcon.SHFILEINFO shinfo = new FileIcon.SHFILEINFO();
            IntPtr hImgLarge = FileIcon.SHGetFileInfo(file, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), FileIcon.SHGFI_ICON | FileIcon.SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES);
            return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }

        public static System.Drawing.Icon GetSmallIcon(string file)
        {
            FileIcon.SHFILEINFO shinfo = new FileIcon.SHFILEINFO();
            IntPtr hImgLarge = FileIcon.SHGetFileInfo(file, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), FileIcon.SHGFI_ICON | FileIcon.SHGFI_SMALLICON);
            return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }
    }
}

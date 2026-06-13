using System;
using System.Runtime.InteropServices;

namespace MacFontRenderer.Utils
{
    public static class WindowsApiInterop
    {
        // SendMessage constants
        public const int WM_SETTINGCHANGE = 0x001A;
        public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);
        public const int SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int AddFontResourceEx(
            string lpszFilename,
            uint fl,
            IntPtr pdv);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int RemoveFontResourceEx(
            string lpszFilename,
            uint fl,
            IntPtr pdv);

        // AddFontResourceEx flags
        public const uint FR_PRIVATE = 0x10;
        public const uint FR_NOT_ENUM = 0x20;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void SetEnvironmentVariable(string name, string value);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern string GetEnvironmentVariable(string name);
    }
}

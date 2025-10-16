using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ClipMate.Services
{
    public class HotkeyService
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const int VK_V = 0x56;

        private const int HOTKEY_ID = 9000;

        public static void Register(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            RegisterHotKey(handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_V);

            HwndSource.FromHwnd(handle).AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                if (msg == 0x0312) // WM_HOTKEY
                {
                    var popup = new QuickPopup(((MainWindow)Application.Current.MainWindow).Snippets);
                    popup.Show();
                    popup.Activate();
                    handled = true;
                }
                return IntPtr.Zero;
            });
        }
    }
}
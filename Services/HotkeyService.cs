using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace ClipMate.Services
{
    public class HotkeyService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        
        // Modifiers
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private IntPtr _windowHandle;
        private HwndSource _source;
        private Action _hotkeyCallback;

        public bool Register(IntPtr windowHandle, ModifierKeys modifiers, Key key, Action callback)
        {
            _windowHandle = windowHandle;
            _hotkeyCallback = callback;

            uint modifierFlags = 0;
            if (modifiers.HasFlag(ModifierKeys.Alt))
                modifierFlags |= MOD_ALT;
            if (modifiers.HasFlag(ModifierKeys.Control))
                modifierFlags |= MOD_CONTROL;
            if (modifiers.HasFlag(ModifierKeys.Shift))
                modifierFlags |= MOD_SHIFT;
            if (modifiers.HasFlag(ModifierKeys.Windows))
                modifierFlags |= MOD_WIN;

            // Convert WPF Key to Virtual Key Code
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            bool registered = RegisterHotKey(_windowHandle, HOTKEY_ID, modifierFlags, vk);

            if (registered)
            {
                // Hook into Windows message loop
                _source = HwndSource.FromHwnd(_windowHandle);
                _source.AddHook(HwndHook);
            }

            return registered;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                _hotkeyCallback?.Invoke();
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Unregister()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _source?.RemoveHook(HwndHook);
            }
        }

        public void Dispose()
        {
            Unregister();
        }
    }
}
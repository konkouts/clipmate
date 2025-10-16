using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ClipMate
{
    public class ClipboardWatcher : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private HwndSource _hwndSource;
        private Window _hiddenWindow;
        private bool _isListening;

        public event EventHandler<string> ClipboardChanged;

        public void Start()
        {
            if (_isListening)
                return;

            // Create a hidden window to receive clipboard messages
            _hiddenWindow = new Window
            {
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Visibility = Visibility.Hidden
            };

            _hiddenWindow.Show();

            var helper = new WindowInteropHelper(_hiddenWindow);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _hwndSource.AddHook(WndProc);

            AddClipboardFormatListener(helper.Handle);
            _isListening = true;
        }

        public void Stop()
        {
            if (!_isListening)
                return;

            if (_hwndSource != null)
            {
                var helper = new WindowInteropHelper(_hiddenWindow);
                RemoveClipboardFormatListener(helper.Handle);
                _hwndSource.RemoveHook(WndProc);
                _hwndSource.Dispose();
                _hwndSource = null;
            }

            if (_hiddenWindow != null)
            {
                _hiddenWindow.Close();
                _hiddenWindow = null;
            }

            _isListening = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        string text = Clipboard.GetText();
                        
                        // Ignore empty or very long clipboard content
                        if (!string.IsNullOrWhiteSpace(text) && text.Length < 10000)
                        {
                            ClipboardChanged?.Invoke(this, text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Clipboard access can fail for various reasons
                    System.Diagnostics.Debug.WriteLine($"Clipboard access error: {ex.Message}");
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
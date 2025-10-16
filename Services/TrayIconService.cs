using System;
using System.Drawing;
using System.Windows.Forms;
using FontStyle = System.Drawing.FontStyle;

namespace ClipMate.Services
{
    public class TrayIconService : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private readonly Action _showMainWindow;
        private readonly Action _showQuickPopup;
        private readonly Action _exitApplication;

        public TrayIconService(Action showMainWindow, Action showQuickPopup, Action exitApplication)
        {
            _showMainWindow = showMainWindow;
            _showQuickPopup = showQuickPopup;
            _exitApplication = exitApplication;
            
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateTrayIcon(),
                Visible = true,
                Text = "ClipMate - Clipboard Manager"
            };

            // Double-click to show main window
            _notifyIcon.DoubleClick += (s, e) => _showMainWindow();

            // Context menu
            var contextMenu = new ContextMenuStrip();
            
            contextMenu.Items.Add("📋 Quick Access (Ctrl+Shift+V)", null, (s, e) => _showQuickPopup());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("🧾 Show Clipboard History", null, (s, e) => _showMainWindow());
            contextMenu.Items.Add("⚙️ Settings", null, (s, e) => _showMainWindow());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("🚪 Exit", null, (s, e) => _exitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private Icon CreateTrayIcon()
        {
            // Create a simple icon (you should replace this with a proper icon file)
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.FillRectangle(Brushes.DodgerBlue, 2, 2, 12, 12);
                graphics.DrawString("C", new Font("Arial", 9, FontStyle.Bold), Brushes.White, -1, -1);
            }
            
            return Icon.FromHandle(bitmap.GetHicon());
        }

        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _notifyIcon.ShowBalloonTip(3000, title, message, icon);
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
        }
    }
}
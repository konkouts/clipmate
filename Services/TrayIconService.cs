using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace ClipMate.Services
{
    public class TrayIconService
    {
        private readonly NotifyIcon _notifyIcon;

        public TrayIconService()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Information,
                Visible = true,
                Text = "ClipMate"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open ClipMate", null, (s, e) => ShowMainWindow());
            contextMenu.Items.Add("Exit", null, (s, e) => ExitApp());
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                }
            });
        }

        private void ExitApp()
        {
            _notifyIcon.Visible = false;
            Application.Current.Shutdown();
        }
    }
}
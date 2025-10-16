using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ClipMate.Services;

namespace ClipMate
{
    public partial class App : Application
    {
        private TrayIconService _trayIcon;
        private HotkeyService _hotkeyService;
        private ClipboardWatcher _clipboardWatcher;
        private ObservableCollection<ClipboardSnippet> _snippets;
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize data
            _snippets = new ObservableCollection<ClipboardSnippet>();
            LoadSnippets();

            // Initialize clipboard watcher
            _clipboardWatcher = new ClipboardWatcher();
            _clipboardWatcher.ClipboardChanged += OnClipboardChanged;
            _clipboardWatcher.Start();

            // Initialize tray icon
            _trayIcon = new TrayIconService(
                showMainWindow: ShowMainWindow,
                showQuickPopup: ShowQuickPopup,
                exitApplication: ExitApplication
            );

            // Create main window but don't show it yet
            _mainWindow = new MainWindow(_snippets);
            _mainWindow.Closing += MainWindow_Closing;

            // Register global hotkey (Ctrl+Shift+V)
            _hotkeyService = new HotkeyService();
            var helper = new WindowInteropHelper(_mainWindow);
            bool hotkeyRegistered = _hotkeyService.Register(
                helper.EnsureHandle(),
                ModifierKeys.Control | ModifierKeys.Shift,
                Key.V,
                ShowQuickPopup
            );

            if (!hotkeyRegistered)
            {
                MessageBox.Show(
                    "Warning: Could not register global hotkey Ctrl+Shift+V.\n" +
                    "The key combination might be in use by another application.",
                    "ClipMate",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            // Show notification
            _trayIcon.ShowNotification(
                "ClipMate Running",
                "Press Ctrl+Shift+V for quick access"
            );
        }

        private void LoadSnippets()
        {
            // TODO: Load from JSON file
            // For now, add some sample data
            _snippets.Add(new ClipboardSnippet 
            { 
                Content = "Welcome to ClipMate!", 
                Timestamp = DateTime.Now,
                IsPinned = true 
            });
        }

        private void SaveSnippets()
        {
            // TODO: Save to JSON file
        }

        private void OnClipboardChanged(object sender, string content)
        {
            // Avoid duplicates
            if (_snippets.Count > 0 && _snippets[0].Content == content)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _snippets.Insert(0, new ClipboardSnippet
                {
                    Content = content,
                    Timestamp = DateTime.Now,
                    IsPinned = false
                });

                // Limit history size (keep last 100 items + pinned)
                while (_snippets.Count > 100)
                {
                    var lastUnpinned = _snippets[_snippets.Count - 1];
                    if (!lastUnpinned.IsPinned)
                        _snippets.RemoveAt(_snippets.Count - 1);
                    else
                        break;
                }

                SaveSnippets();
            });
        }

        private void ShowMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_mainWindow.WindowState == WindowState.Minimized)
                    _mainWindow.WindowState = WindowState.Normal;
                
                _mainWindow.Show();
                _mainWindow.Activate();
            });
        }

        private void ShowQuickPopup()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var popup = new QuickPopup(_snippets);
                popup.Show();
            });
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Don't actually close, just hide
            e.Cancel = true;
            _mainWindow.Hide();
        }

        private void ExitApplication()
        {
            // Actually exit the application
            _clipboardWatcher?.Stop();
            _hotkeyService?.Dispose();
            _trayIcon?.Dispose();
            SaveSnippets();
            
            Application.Current.Shutdown();
        }
    }

    // Model class
    public class ClipboardSnippet : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isPinned;

        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        
        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                if (_isPinned != value)
                {
                    _isPinned = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsPinned)));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
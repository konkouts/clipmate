using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ClipMate.Models;

namespace ClipMate.Services
{
    public class ClipboardWatcher
    {
        private readonly ObservableCollection<ClipboardSnippet> _snippets;
        private string _lastText = "";

        public ClipboardWatcher(ObservableCollection<ClipboardSnippet> snippets)
        {
            _snippets = snippets;

            var timer = new System.Timers.Timer(500); // check every 0.5s
            timer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (Clipboard.ContainsText())
                        {
                            var text = Clipboard.GetText();
                            if (text != _lastText)
                            {
                                var existing = _snippets.FirstOrDefault(s => s.Content == text);
                                if (existing != null)
                                {
                                    // Move existing snippet to top
                                    var index = _snippets.IndexOf(existing);
                                    _snippets.Move(index, 0);
                                }
                                else
                                {
                                    // Insert new snippet
                                    _snippets.Insert(0, new ClipboardSnippet { Content = text });
                                }

                                _lastText = text;
                            }
                        }
                    }
                    catch { }
                });
            };
            timer.Start();
        }

    }
}
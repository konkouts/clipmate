using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ClipMate.Models;

namespace ClipMate
{
    public partial class QuickPopup : Window
    {
        private readonly ObservableCollection<ClipboardSnippet> _snippets;

        public QuickPopup(ObservableCollection<ClipboardSnippet> snippets)
        {
            InitializeComponent();
            _snippets = snippets;
            SnippetList.ItemsSource = snippets;

            SearchBox.TextChanged += (s, e) =>
            {
                var text = SearchBox.Text.ToLower();
                SnippetList.ItemsSource = _snippets
                    .Where(snip => snip.Content.ToLower().Contains(text))
                    .OrderByDescending(snip => snip.IsPinned);
            };

            SnippetList.MouseDoubleClick += (s, e) =>
            {
                if (SnippetList.SelectedItem is ClipboardSnippet snip)
                {
                    Clipboard.SetText(snip.Content);
                    Close();
                }
            };
        }
    }
}
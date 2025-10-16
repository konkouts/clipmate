﻿using System;
using ClipMate.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ClipMate
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ClipboardSnippet> _snippets;
        
        private ClipboardWatcher _watcher;
        private readonly string _dataFile = "snippets.json";

        public MainWindow(ObservableCollection<ClipboardSnippet> snippets)
        {
            InitializeComponent();
            _snippets = snippets;
            DataContext = this;

            LoadSnippets();

            _watcher = new ClipboardWatcher(_snippets);

            SearchBox.TextChanged += SearchBox_TextChanged;
            
            Loaded += (s, e) => HotkeyService.Register(this);
        }

        private void SnippetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ClipboardSnippet snippet)
            {
                Clipboard.SetText(snippet.Content);
            }
        }
        
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox == null || Snippets == null) return;

            var filterText = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(filterText) || filterText == "Search...")
            {
                SnippetList.ItemsSource = Snippets;
            }
            else
            {
                var filtered = Snippets
                    .Where(s => s.Content.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.IsPinned) // pinned first
                    .ToList();
                SnippetList.ItemsSource = filtered;

            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Search...";
        }

        private void LoadSnippets()
        {
            if (File.Exists(_dataFile))
            {
                var json = File.ReadAllText(_dataFile);
                var list = JsonSerializer.Deserialize<ObservableCollection<ClipboardSnippet>>(json);
                if (list != null)
                    Snippets = list;
            }
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true; // prevent closing
            Hide();          // hide instead of closing
        }


        private void SaveSnippets()
        {
            var json = JsonSerializer.Serialize(Snippets);
            File.WriteAllText(_dataFile, json);
        }
    }
}
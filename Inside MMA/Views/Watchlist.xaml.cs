using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для Watchlist.xaml
    /// </summary>
    public partial class Watchlist
    {
        private WatchlistViewModel _context;
        private CollectionViewSource _viewSource;
        private string _filter;
        private string _name;
        private string _board;
        private string _seccode;
        public Watchlist()
        {
            InitializeComponent();
            _viewSource = FindResource("ViewSource") as CollectionViewSource;
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _context = (WatchlistViewModel) DataContext;
            _context.GetWatchlists();
            _context.OnItemDeleted += OnItemDeleted;
        }

        private void OnItemDeleted()
        {
            DataGridSec.Focus();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((WatchlistViewModel)DataContext).Dialog = DialogCoordinator.Instance;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var t = (TextBox)sender;
            _filter = t.Text;
            switch (t.Name)
            {
                case "Name":
                    {
                        _viewSource.Filter -= FilterName;
                        if (_filter != "")
                        {
                            _name = _filter;
                            _viewSource.Filter += FilterName;
                        }
                        break;
                    }
                case "Board":
                    {
                        _viewSource.Filter -= FilterBoard;
                        if (_filter != "")
                        {
                            _board = _filter;
                            _viewSource.Filter += FilterBoard;
                        }
                        break;
                    }
                case "Seccode":
                    {
                        _viewSource.Filter -= FilterSeccode;
                        if (_filter != "")
                        {
                            _seccode = _filter;
                            _viewSource.Filter += FilterSeccode;
                        }
                        break;
                    }
            }
            DataGridSec.SelectedIndex = -1;
        }
        private void FilterName(object sender, FilterEventArgs e)
        {
            var src = e.Item as Security;
            if (src == null)
                e.Accepted = false;
            else if (src.Shortname.IndexOf(_name, StringComparison.OrdinalIgnoreCase) < 0)
                e.Accepted = false;
        }
        private void FilterBoard(object sender, FilterEventArgs e)
        {
            var src = e.Item as Security;
            if (src == null)
                e.Accepted = false;
            else if (src.Board.IndexOf(_board, StringComparison.OrdinalIgnoreCase) < 0)
                e.Accepted = false;
        }
        private void FilterSeccode(object sender, FilterEventArgs e)
        {
            var src = e.Item as Security;
            if (src == null)
                e.Accepted = false;
            else if (src.Seccode.IndexOf(_seccode, StringComparison.OrdinalIgnoreCase) < 0)
                e.Accepted = false;
        }

        private void Clipboard_OnClick(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = true;
            ClipboardText.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }

        private void CancelPopupClick(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = false;
        }

        private void ComboBox_OnDropDownOpened(object sender, EventArgs e)
        {
            _context.GetWatchlists();
        }

        private void ContextMenu_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _context.GetWatchlists();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = false;
        }
    }
}

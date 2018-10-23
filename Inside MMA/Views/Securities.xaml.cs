using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для Securities.xaml
    /// </summary>
    public partial class Securities
    {
        private CollectionViewSource _viewSource;
        private string _filter;
        private string _name;
        private string _board;
        private string _seccode;

        public Securities()
        {
            InitializeComponent();
            _viewSource = FindResource("ViewSource") as CollectionViewSource;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((SecuritiesViewModel) DataContext).Dialog = DialogCoordinator.Instance;
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

        private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            ((SecuritiesViewModel)DataContext).GetWatchlists();
        }
    }
}

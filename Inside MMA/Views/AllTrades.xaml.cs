using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для AllTrades.xaml
    /// </summary>
    public partial class AllTrades
    {
        private TimeSpan _from = new TimeSpan(0, 0, 0);
        private TimeSpan _to = new TimeSpan(23, 59, 59);

        public AllTrades()
        {
            InitializeComponent();
            FromPicker.Culture = CultureInfo.CurrentCulture;
            ToPicker.Culture = CultureInfo.CurrentCulture;
        }

        ~AllTrades()
        {
            Debug.WriteLine("AllTrades window disposed");
        }

        private void Expander_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Expander.IsExpanded = false;
        }

        private void ContextMenuSizeSelect(object sender, RoutedEventArgs e)
        {
            var selectedItem = (TradeItem)DataGridAllTrades.SelectedItem;
            if (selectedItem == null) return;
            IsSelecting.IsChecked = true;
            SelectSizeTextBox.Text = selectedItem.Quantity.ToString();
        }

        private void ContextMenuPriceSelect(object sender, RoutedEventArgs e)
        {
            var selectedItem = (TradeItem)DataGridAllTrades.SelectedItem;
            if (selectedItem == null) return;
            IsSelectingPrice.IsChecked = true;
            SelectPriceTextBox.Text = selectedItem.Price.ToString("F");
        }
    }
}

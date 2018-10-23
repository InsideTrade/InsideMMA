using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Inside_MMA.Models.Alerts;
using MahApps.Metro.Controls;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для AlertSelection.xaml
    /// </summary>
    public partial class AlertSelection
    {
        private int _lastSelectedIndex = -1;
        public AlertSelection()
        {
            InitializeComponent();
            TransitioningContentControl.DataContextChanged += TypesComboBoxOnDataContextChanged;
        }


        private void TypesComboBoxOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var dataContext = TransitioningContentControl.DataContext;
            if (dataContext is EqualsSizeAlert || dataContext is GreaterThanSizeAlert ||
                dataContext is GreaterThanEatenSize)
            {
                TransitioningContentControl.Content = FindResource("Size");
                //TypesComboBox.SelectedIndex = 0;
            }
            if (dataContext is GreaterThanPriceAlert || dataContext is SmallerThanPriceAlert)
            {
                TransitioningContentControl.Content = FindResource("Price");
                //TypesComboBox.SelectedIndex = 1;
            }
            if (dataContext is TrueAlert)
            {
                TransitioningContentControl.Content = FindResource("True");
                //TypesComboBox.SelectedIndex = 1;
            }
            if (dataContext is GreaterThanDeltaOIAlert)
            {
                TransitioningContentControl.Content = FindResource("DeltaOI");
            }
        }
        
        //private void TypeSelected(object sender, SelectionChangedEventArgs e)
        //{
        //    var transition = TypesComboBox.SelectedIndex > _lastSelectedIndex
        //        ? TransitionType.Right
        //        : TransitionType.Left;
        //    TransitioningContentControl.Transition = transition;
        //    switch (TypesComboBox.SelectedValue)
        //    {
        //        case "Eq":
        //            TransitioningContentControl.Content = FindResource("Eq");
        //            break;
        //        case "Gt":
        //            TransitioningContentControl.Content = FindResource("Gt");
        //            break;
        //    }
        //    _lastSelectedIndex = TypesComboBox.SelectedIndex;
        //}

        private void Ok(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
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

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для AlertsSelectionEditMode.xaml
    /// </summary>
    public partial class AlertsSelectionEditMode
    {
        public AlertsSelectionEditMode()
        {
            InitializeComponent();
            TransitioningContentControl.DataContextChanged += TypesComboBoxOnDataContextChanged;
        }
        private void TypesComboBoxOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (TransitioningContentControl.DataContext is EqualsSizeAlert)
            {
                TransitioningContentControl.Content = FindResource("Eq");
            }
            if (TransitioningContentControl.DataContext is GreaterThanSizeAlert)
            {
                TransitioningContentControl.Content = FindResource("Gt");
            }
        }
        private void Ok(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using static System.Windows.Controls.Validation;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для NewOrder.xaml
    /// </summary>
    public partial class NewOrder
    {
        public NewOrder()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var context = (NewOrderViewModel) DataContext;
            context.Dialog = DialogCoordinator.Instance;
            context.Close = Close;
        }

        private void Price_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var txtbox = (TextBox) sender;
            if (txtbox.IsVisible)
            {
                BindingOperations.GetBinding(Price, TextBox.TextProperty)?
                    .ValidationRules.Add(new PriceOnlyValidarionRule { ValidatesOnTargetUpdated = true });
            }
            else
            {
                BindingOperations.GetBinding(Price, TextBox.TextProperty)?.ValidationRules.Clear();
            }
            Price.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            //Close();
        }

        private void Board_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (GetHasError(Board))
                Client.Text = "-";
            else
                Client.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }
    }
}

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using static System.Windows.Controls.Validation;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для NewStopOrder.xaml
    /// </summary>
    public partial class NewStopOrder
    {
        private List<TextBox> _stopLossBoxes;
        private List<TextBox> _takeProfitBoxes;
        public NewStopOrder()
        {
            InitializeComponent();
            _stopLossBoxes = new List<TextBox>
            {
                StopLossActivationPrice,
                StopLossOrderPrice,
                StopLossQuantity,
                StopLossGuardTime
            };
            _takeProfitBoxes = new List<TextBox>
            {
                TakeProfitActivationPrice,
                TakeProfitQuantity,
                TakeProfitGuardTime,
                TakeProfitCorrection,
                TakeProfitSpread
            };
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((NewStopOrderViewModel) DataContext).Dialog = DialogCoordinator.Instance;
            ((NewStopOrderViewModel) DataContext).Close = Close;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var stopLossBox in _stopLossBoxes)
            {
                if (stopLossBox.Text != string.Empty)
                {
                    ValidateStopLoss();
                    return;
                }
            }
            foreach (var stopLossBox in _stopLossBoxes)
            {
                BindingOperations.GetBinding(stopLossBox, TextBox.TextProperty)?.ValidationRules.Clear();
                stopLossBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
        }

        private void ValidateStopLoss()
        {
            BindingOperations.GetBinding(StopLossActivationPrice, TextBox.TextProperty)?
                .ValidationRules.Add(new NumberOnlyValidationRule {ValidatesOnTargetUpdated = true});
            if (StopLossByMarket.IsChecked == false)
                BindingOperations.GetBinding(StopLossOrderPrice, TextBox.TextProperty)?
                    .ValidationRules.Add(new NumberOnlyValidationRule { ValidatesOnTargetUpdated = true });
            BindingOperations.GetBinding(StopLossQuantity, TextBox.TextProperty)?
                .ValidationRules.Add(new IntegerOnlyValidationRule { ValidatesOnTargetUpdated = true });
            BindingOperations.GetBinding(StopLossGuardTime, TextBox.TextProperty)?
                .ValidationRules.Add(new IntegerOrEmptyValidationRule { ValidatesOnTargetUpdated = true });
            foreach (var stopLossBox in _stopLossBoxes)
            {
                stopLossBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
        }

        private void OnTextChangedTP(object sender, TextChangedEventArgs e)
        {
            foreach (var takeProfitBox in _takeProfitBoxes)
            {
                if (takeProfitBox.Text != string.Empty)
                {
                    ValidateTakeProfit();
                    return;
                }
            }
            foreach (var takeProfitBox in _takeProfitBoxes)
            {
                BindingOperations.GetBinding(takeProfitBox, TextBox.TextProperty)?.ValidationRules.Clear();
                takeProfitBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
        }

        private void ValidateTakeProfit()
        {
            BindingOperations.GetBinding(TakeProfitActivationPrice, TextBox.TextProperty)?
                .ValidationRules.Add(new NumberOnlyValidationRule { ValidatesOnTargetUpdated = true });
            BindingOperations.GetBinding(TakeProfitQuantity, TextBox.TextProperty)?
                .ValidationRules.Add(new IntegerOnlyValidationRule { ValidatesOnTargetUpdated = true });
            BindingOperations.GetBinding(TakeProfitGuardTime, TextBox.TextProperty)?
                .ValidationRules.Add(new IntegerOrEmptyValidationRule { ValidatesOnTargetUpdated = true });
            BindingOperations.GetBinding(TakeProfitCorrection, TextBox.TextProperty)?
               .ValidationRules.Add(new NumberOrEmptyValidationRule { ValidatesOnTargetUpdated = true });
            BindingOperations.GetBinding(TakeProfitSpread, TextBox.TextProperty)?
               .ValidationRules.Add(new NumberOrEmptyValidationRule { ValidatesOnTargetUpdated = true });
            foreach (var takeProfitBox in _takeProfitBoxes)
            {
                takeProfitBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
        }

        private void StopLossByMarket_OnChecked(object sender, RoutedEventArgs e)
        {
            if (StopLossByMarket.IsChecked == true)
            {
                BindingOperations.GetBinding(StopLossOrderPrice, TextBox.TextProperty)?.ValidationRules.Clear();
                StopLossOrderPrice.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
            else
            {
                BindingOperations.GetBinding(StopLossOrderPrice, TextBox.TextProperty)?.ValidationRules.Clear();
                BindingOperations.GetBinding(StopLossOrderPrice, TextBox.TextProperty)?
                    .ValidationRules.Add(new NumberOnlyValidationRule { ValidatesOnTargetUpdated = true });
                StopLossOrderPrice.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
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

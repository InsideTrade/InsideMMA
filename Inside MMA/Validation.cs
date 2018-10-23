using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;

namespace Inside_MMA
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public UIElement Element { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Element?.IsEnabled == false)
                return ValidationResult.ValidResult;

            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "Field is required")
                : ValidationResult.ValidResult;
        }
    }

    public class IntegerOnlyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            uint n;
            return uint.TryParse(value?.ToString(), out n)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Integers only");
        }
    }
    public class QuantityOnlyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            uint n;
            return uint.TryParse(value?.ToString(), out n)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Size only");
        }
    }
    public class NumberOnlyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double n;
            return double.TryParse(value?.ToString(), out n)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Numbers only");
        }
    }
    public class PriceOnlyValidarionRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double n;
            return double.TryParse(value?.ToString(), out n)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Price only");
        }
    }
    public class NumberOrEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value?.ToString() == string.Empty)
                return ValidationResult.ValidResult;
            double n;
            return double.TryParse(value?.ToString(), out n)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Numbers only");
        }
    }

    public class IntegerOrEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value?.ToString() == string.Empty)
                return ValidationResult.ValidResult;
            uint n;
            return uint.TryParse(value?.ToString(), out n)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Integers only");
        }
    }

    public class BoardExistsValidationRule : ValidationRule
    {
        private static ObservableCollection<Security> Securities
            => MainWindowViewModel.SecVm._secList;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var sec = Securities.FirstOrDefault(item => item.Board == value?.ToString().ToUpper());
            return sec != null ? ValidationResult.ValidResult : new ValidationResult(false, "Unavailable board");
        }
    }

    public class SeccodeExistsValidationRule : ValidationRule
    {
        private static ObservableCollection<Security> Securities
            => MainWindowViewModel.SecVm._secList;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var sec = Securities.FirstOrDefault(item => item.Seccode == value?.ToString());
            return sec != null ? ValidationResult.ValidResult : new ValidationResult(false, "Unavailable seccode");
        }
    }
}

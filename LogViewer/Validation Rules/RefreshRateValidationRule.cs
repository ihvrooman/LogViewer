using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LogViewer.ValidationRules
{
    public class RefreshRateValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value.ToString(), out int refreshRateInMilliseconds))
            {
                if (refreshRateInMilliseconds < 50 || refreshRateInMilliseconds > 30000)
                {
                    return new ValidationResult(false, $"Refresh rate must be between 50 and 30,000 milliseconds.");
                }
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, $"Refresh rate must be a valid integer.");
        }
    }
}

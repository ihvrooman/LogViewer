using LogViewer.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LogViewer.ValidationRules
{
    public class DatabaseNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var databaseName = value.ToString();
            var chars = new char[] { '\\', '/', '.', '$', '!', '@', '#', '%', '^', '&', '*', '(', ')' };
            if (databaseName.IndexOfAny(chars) >= 0)
            {
                return new ValidationResult(false, $"The database name should not have any special characters.");
            }

            if (Settings.Default.DatabaseNames.Contains(databaseName))
            {
                return new ValidationResult(false, $"This database has already been added.");
            }

            return ValidationResult.ValidResult;
        }
    }
}

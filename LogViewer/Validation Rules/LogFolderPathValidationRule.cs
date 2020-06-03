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
    public class LogFolderPathValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var logFolderPath = value.ToString();
            if (logFolderPath.Length < 2 || logFolderPath.Substring(0, 2) != "\\\\")
            {
                return new ValidationResult(false, $"The log folder path must be a valid path in the format \"\\\\server\\logFolderPath\". You can use the default C: drive file share 'C$' if you need to access the C: drive.");
            }
            else if (Settings.Default.LogFolderPaths.Contains(logFolderPath))
            {
                return new ValidationResult(false, $"This log folder path has already been added.");
            }

            return ValidationResult.ValidResult;
        }
    }
}

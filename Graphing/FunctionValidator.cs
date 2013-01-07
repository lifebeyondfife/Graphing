using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Globalization;
using System.Windows;

namespace Graphing
{
	public class FunctionValidator : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var result = new ValidationResult(true, null);

			var input = (value ?? string.Empty).ToString();
			if (string.IsNullOrEmpty(input))
				return result;

			var errorString = GraphingModel.ValidateFunction(input);

			if (string.IsNullOrEmpty(errorString))
				return result;

			MessageBox.Show(errorString, "Error Compiling Expression", MessageBoxButton.OK, MessageBoxImage.Error);
			result = new ValidationResult(false, errorString);

			return result;
		}
	}
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace ByteAuthor.StaskStack
{
	public class BooleanInverterConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return !boolValue;
			}

			throw new NotImplementedException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
using Caliburn.Micro;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Zw.XmlLanguageEditor.ViewModels.ValueConverters
{
    public class CellBackgroundMarkerConverter : IValueConverter
    {

        private readonly Configuration config;

        public CellBackgroundMarkerConverter()
        {
            this.config = IoC.Get<Configuration>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((!config.HightlightEmptyCells) && (!config.HighlightMasterMatchingCells)) return Binding.DoNothing;

            var record = value as XmlRecordViewModel;
            if (record == null) return Binding.DoNothing;
            var dataField = parameter as string;
            if (dataField == null) return Binding.DoNothing;

            string masterValue = record.MasterValue;
            string thisValue = null;
            bool checkAgainstMasterValue = true;

            if (dataField == "MasterValue")
            {
                checkAgainstMasterValue = false;
                thisValue = record.MasterValue;
            }
            else if (dataField.StartsWith("[") && dataField.EndsWith("]"))
            {
                string indexStr = dataField.Substring(1, dataField.Length - 2);
                int index;
                if (Int32.TryParse(indexStr, out index))
                {
                    thisValue = record[index];
                }
            }
            else
            {
                var propertyInfo = record.GetType().GetProperty(dataField, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    thisValue = propertyInfo.GetValue(record)?.ToString();
                }
                else
                {
                    return Binding.DoNothing;
                }
            }

            if ((config.HightlightEmptyCells) && (String.IsNullOrWhiteSpace(thisValue)))
            {
                return Brushes.Yellow;
            }
            if ((config.HighlightMasterMatchingCells) && (checkAgainstMasterValue) && (String.Equals(thisValue, masterValue)))
            {
                return Brushes.Orange;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

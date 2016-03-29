﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Zw.XmlLanguageEditor.ViewModels.Behaviors;

namespace Zw.XmlLanguageEditor.ViewModels.ValueConverters
{
    /// <summary>
    /// Converts a binding to a ColumnConfig to a GridView the ListView can use.
    /// </summary>
    /// <remarks>
    /// Credits to: https://github.com/9swampy/DynamicPropertyPropertiesListGridViewExample
    /// </remarks>
    public class ConfigToDynamicGridViewConverter : IValueConverter
    {

        private readonly ColumnIsVisibleToWidthConverter columnIsVisibleToWidthConverter = new ColumnIsVisibleToWidthConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var config = value as ColumnConfig;
            if (config != null)
            {
                var gridView = new GridView();
                gridView.ColumnHeaderContainerStyle = Application.Current.FindResource("HeaderContainerStyle") as Style;
                gridView.ColumnHeaderTemplate = Application.Current.FindResource("HeaderTemplate") as DataTemplate;
                if (config.Columns != null)
                {
                    foreach (var column in config.Columns)
                    {
                        var bindingDisplayMember = new Binding(column.DataField);
                        GridViewColumn gvc = new GridViewColumn { Header = column, DisplayMemberBinding = bindingDisplayMember };
                        var bindingWidth = new Binding("IsVisible");
                        bindingWidth.Source = column;
                        bindingWidth.Converter = this.columnIsVisibleToWidthConverter;
                        BindingOperations.SetBinding(gvc, GridViewColumn.WidthProperty, bindingWidth);
                        gvc.SetValue(GridViewSort.PropertyNameProperty, bindingDisplayMember.Path.Path);
                        gridView.Columns.Add(gvc);
                    }
                }
                return gridView;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

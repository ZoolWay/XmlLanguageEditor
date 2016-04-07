using System;
using System.Windows;
using System.Windows.Controls;

namespace Zw.XmlLanguageEditor.ViewModels.Behaviors
{
    /// <summary>
    /// Defines optional behaviors for ListView controls.
    /// </summary>
    public static class ListViewBehaviors
    {

        /// <summary>
        /// Defines 'ScrollIntoViewListItem', a write-only item to be written from the ViewModel to scroll an item into the view.
        /// </summary>
        public static readonly DependencyProperty ScrollIntoViewListItemProperty = DependencyProperty.RegisterAttached("ScrollIntoViewListItem", typeof(object), typeof(ListViewBehaviors), new PropertyMetadata(ScrollIntoViewListItemSetValue));

        public static void SetScrollIntoViewListItem(DependencyObject d, object value)
        {
            d.SetValue(ScrollIntoViewListItemProperty, value);
        }

        private static void ScrollIntoViewListItemSetValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListView lv = d as ListView;
            if (lv == null) throw new ArgumentException("sender");
            lv.ScrollIntoView(e.NewValue);
        }

    }
}

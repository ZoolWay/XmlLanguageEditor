using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zw.XmlLanguageEditor.ViewModels.Behaviors
{
    /// <summary>
    /// Defines a configured column.
    /// </summary>
    /// <remarks>
    /// Credits to: https://github.com/9swampy/DynamicPropertyPropertiesListGridViewExample
    /// </remarks>
    public class Column : PropertyChangedBase
    {
        public string Header { get; set; }

        public string DataField { get; set; }

        public string EntryKey { get; set; }

        public string FilterValue { get; set; }

        public bool IsDetailPanelColumn { get; set; }

        public bool IsVisible { get; set; }

        public void ShowInDetailPanel()
        {
            //var eventAggregator = IoC.Get<IEventAggregator>();
            //eventAggregator.PublishOnUIThread(new SetDetailPanelKeyMessage(this, this.EntryKey));
        }
    }
}

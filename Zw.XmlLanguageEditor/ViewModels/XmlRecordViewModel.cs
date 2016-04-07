using Caliburn.Micro;
using PropertyChanged;
using System;
using System.Collections.Generic;

namespace Zw.XmlLanguageEditor.ViewModels
{
    /// <summary>
    /// ViewModel for a language record in the XmlGridView.
    /// </summary>
    public class XmlRecordViewModel : PropertyChangedBase
    {

        private List<string> secondaryValues;

        [AlsoNotifyFor("Self")]
        public string Id { get; set; }

        [AlsoNotifyFor("Self")]
        public bool IsHighlighted { get; set; }

        [AlsoNotifyFor("Self")]
        public string MasterValue { get; set; }

        public string this[int secondaryIndex]
        {
            get
            {
                return (secondaryIndex < secondaryValues.Count) ? secondaryValues[secondaryIndex] : null;
            }
            set
            {
                while (secondaryIndex >= secondaryValues.Count) secondaryValues.Add(null); // fill with emty entries if needed
                secondaryValues[secondaryIndex] = value;
                NotifyOfPropertyChange(() => Self); // Fody.PropertyChanged does not handle this automatically with the AlsoNotifyFor-attribute
            }
        }

        /// <summary>
        /// Self reference for special bindings.
        /// </summary>
        public XmlRecordViewModel Self
        {
            get { return this; }
        }

        public XmlRecordViewModel()
        {
            this.IsNotifying = false;
            this.secondaryValues = new List<string>();
        }

        internal bool MatchesSearchText(string searchText)
        {
            if (this.Id.Contains(searchText)) return true;
            if (this.MasterValue.Contains(searchText)) return true;
            foreach (var secondary in secondaryValues)
            {
                if (secondary.Contains(searchText)) return true;
            }
            return false;
        }

    }
}

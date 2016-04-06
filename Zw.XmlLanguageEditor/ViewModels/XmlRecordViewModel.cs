using Caliburn.Micro;
using System;
using System.Collections.Generic;

namespace Zw.XmlLanguageEditor.ViewModels
{
    public class XmlRecordViewModel : PropertyChangedBase
    {
        private List<string> secondaryValues;

        public string Id { get; set; }
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
            }
        }

        public XmlRecordViewModel()
        {
            this.secondaryValues = new List<string>();
        }

    }
}

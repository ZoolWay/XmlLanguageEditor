using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace Zw.XmlLanguageEditor
{
    public class Configuration
    {
        public class MruEntry
        {
            public string MasterFile { get; set; }
            public string[] SecondaryFiles { get; set; }
            public DateTime LastUsed { get; set; }
        }

        public bool HightlightEmptyCells { get; set; }
        public bool HighlightMasterMatchingCells { get; set; }
        public BindableCollection<MruEntry> MostRecentlyUsedList { get; set; }
        public bool AutoLoadMostRecent { get; set; }

        public Configuration()
        {
            this.MostRecentlyUsedList = new BindableCollection<MruEntry>();
            SetDefaults();
        }

        public void Load()
        {
            SetDefaults(); // back to default, then load explicit settings
            Properties.Settings.Default.Reload();
            this.HightlightEmptyCells = Properties.Settings.Default.HighlightEmptyCells;
            this.HighlightMasterMatchingCells = Properties.Settings.Default.HightlightMasterMatchingCells;
            var mruItems = JsonConvert.DeserializeObject<IEnumerable<MruEntry>>(Properties.Settings.Default.Mru);
            this.MostRecentlyUsedList.AddRange(mruItems);
            this.AutoLoadMostRecent = Properties.Settings.Default.AutoLoadMostRecent;
        }

        public void Save()
        {
            Properties.Settings.Default.HighlightEmptyCells = this.HightlightEmptyCells;
            Properties.Settings.Default.HightlightMasterMatchingCells = this.HighlightMasterMatchingCells;
            Properties.Settings.Default.Mru = JsonConvert.SerializeObject(this.MostRecentlyUsedList);
            Properties.Settings.Default.AutoLoadMostRecent = Properties.Settings.Default.AutoLoadMostRecent;
            Properties.Settings.Default.Save();
        }

        private void SetDefaults()
        {
            this.HightlightEmptyCells = true;
            this.HighlightMasterMatchingCells = true;
            this.MostRecentlyUsedList.Clear();
            this.AutoLoadMostRecent = false;
        }

    }
}

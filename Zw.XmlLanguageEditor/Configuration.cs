using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zw.XmlLanguageEditor
{
    public class Configuration
    {
        public bool HightlightEmptyCells { get; set; }
        public bool HighlightMasterMatchingCells { get; set; }

        public Configuration()
        {
            SetDefaults();
        }

        public void Load()
        {
            SetDefaults(); // back to default, then load explicit settings
            Properties.Settings.Default.Reload();
            this.HightlightEmptyCells = Properties.Settings.Default.HighlightEmptyCells;
            this.HighlightMasterMatchingCells = Properties.Settings.Default.HightlightMasterMatchingCells;
        }

        public void Save()
        {
            Properties.Settings.Default.HighlightEmptyCells = this.HightlightEmptyCells;
            Properties.Settings.Default.HightlightMasterMatchingCells = this.HighlightMasterMatchingCells;
            Properties.Settings.Default.Save();
        }

        private void SetDefaults()
        {
            this.HightlightEmptyCells = true;
            this.HighlightMasterMatchingCells = true;
        }

    }
}

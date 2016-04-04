using Caliburn.Micro;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System;

namespace Zw.XmlLanguageEditor.ViewModels
{
    public class ShellViewModel : Screen, IShell
    {

        private static readonly log4net.ILog log = global::log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Configuration config;

        public ShellViewModel()
        {
            this.DisplayName = "Zw.XmlLanguageEditor";
            this.IsLoading = true;
            this.config = IoC.Get<Configuration>();
        }

        public bool IsLoading { get; set; }

        public bool OptionHighlightEmptyCells { get; set; }

        public bool OptionHighlightMasterMatchingCells { get; set; }

        public bool OptionAutoAddToMaster { get; set; }

        public bool OptionAutoAddToSecondaries { get; set; }

        public XmlGridViewModel XmlGridView { get; private set; }

        public void CloseApplication()
        {
            log.Info("Closing application");
            TryClose();
        }

        public void OpenMaster()
        {
            log.Debug("Asking for new master file to open");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open master file";
            if (ofd.ShowDialog().GetValueOrDefault(false))
            {
                this.XmlGridView.OpenMasterFile(ofd.FileName);
            }
        }
    
        public void OpenSecondary()
        {
            log.Debug("Asking for secondary file to open");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Add secondary file";
            if (ofd.ShowDialog().GetValueOrDefault(false))
            {
                this.XmlGridView.AddSecondaryFile(ofd.FileName);
            }
        }

        public void SaveAll()
        {
            if (!this.XmlGridView.IsAnyLoaded) return;
            log.Debug("Saving all open files");
            this.XmlGridView.WriteAllFilesToDisk();
        }

        public void CloseAll()
        {
            if (!this.XmlGridView.IsAnyLoaded) return;
            log.Debug("Closing all open files");
            this.XmlGridView.CloseAllFiles();
        }

        protected async override void OnInitialize()
        {
            log.Debug("Initializing");
            await Task.Run(() => config.Load());
            this.OptionHighlightEmptyCells = config.HightlightEmptyCells;
            this.OptionHighlightMasterMatchingCells = config.HighlightMasterMatchingCells;
            this.XmlGridView = new XmlGridViewModel();
            await Task.Delay(250);
            this.IsLoading = false;
        }

        protected async override void OnDeactivate(bool close)
        {
            if (!close) return;
            await Task.Run(() => config.Save());
        }

        public override void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
        {
            base.NotifyOfPropertyChange(propertyName);
            if (propertyName.StartsWith("Option"))
            {
                ReadConfigValuesFromShell();
            }
        }

        private void ReadConfigValuesFromShell()
        {
            this.config.HightlightEmptyCells = this.OptionHighlightEmptyCells;
            this.config.HighlightMasterMatchingCells = this.OptionHighlightMasterMatchingCells;
        }
    }
}
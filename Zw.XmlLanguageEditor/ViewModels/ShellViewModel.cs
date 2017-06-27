using Caliburn.Micro;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System;
using System.Windows;
using Zw.XmlLanguageEditor.Ui.Events;
using DataFormat = Zw.XmlLanguageEditor.Parsing.DataFormat;

namespace Zw.XmlLanguageEditor.ViewModels
{
    public class ShellViewModel : Screen, IShell, IHandle<LoadedMasterEvent>, IHandle<ClosedMasterEvent>
    {

        private static readonly log4net.ILog log = global::log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Configuration config;

        private readonly IEventAggregator evengAggregator;

        private bool isConfigApplied;

        public ShellViewModel()
        {
            this.DisplayName = "Zw.XmlLanguageEditor";
            this.IsLoading = true;
            this.config = IoC.Get<Configuration>();
            this.evengAggregator = IoC.Get<IEventAggregator>();
            this.evengAggregator.Subscribe(this);
            this.isConfigApplied = false;
        }

        public bool IsLoading { get; set; }

        public bool OptionHighlightEmptyCells { get; set; }

        public bool OptionHighlightMasterMatchingCells { get; set; }

        public bool OptionAutoAddToMaster { get; set; }

        public bool OptionAutoAddToSecondaries { get; set; }

        public XmlGridViewModel XmlGridView { get; private set; }

        public string MasterFormatDescription { get; set; }

        public bool ShowFormatDescription
        {
            get { return !String.IsNullOrWhiteSpace(this.MasterFormatDescription); }
        }

        public void Handle(LoadedMasterEvent message)
        {
            switch (message.Format)
            {
                case DataFormat.Xml:
                    this.MasterFormatDescription = "XML";
                    break;

                default:
                    this.MasterFormatDescription = "unknown";
                    break;
            }
        }

        public void Handle(ClosedMasterEvent message)
        {
            this.MasterFormatDescription = String.Empty;
        }

        public void CloseApplication()
        {
            log.Info("Closing application");
            TryClose();
        }

        public void OpenMaster()
        {
            log.Debug("Asking for new master file to open");
            if (UserWantsToKeepExistingChanges()) return;
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

        public void CreateSecondary()
        {
            log.Debug("Asking for a new secondary file to create");
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Create new secondary file";
            sfd.OverwritePrompt = true;
            if (sfd.ShowDialog().GetValueOrDefault(false))
            {
                this.XmlGridView.CreateSecondaryFile(sfd.FileName);
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
            if (UserWantsToKeepExistingChanges()) return;
            log.Debug("Closing all open files");
            this.XmlGridView.CloseAllFiles();
        }

        protected async override void OnInitialize()
        {
            log.Debug("Initializing");
            await Task.Run(() => config.Load());
            this.OptionHighlightEmptyCells = config.HightlightEmptyCells;
            this.OptionHighlightMasterMatchingCells = config.HighlightMasterMatchingCells;
            this.isConfigApplied = true;
            this.XmlGridView = new XmlGridViewModel();
            await Task.Delay(250);
            this.IsLoading = false;
        }

        protected async override void OnDeactivate(bool close)
        {
            if (!close) return;
            await Task.Run(() => config.Save());
        }

        public void UpdateConfigValuesFromShell()
        {
            if (!this.isConfigApplied) return;
            this.config.HightlightEmptyCells = this.OptionHighlightEmptyCells;
            this.config.HighlightMasterMatchingCells = this.OptionHighlightMasterMatchingCells;
        }

        public override void CanClose(Action<bool> callback)
        {
            if (this.XmlGridView.IsChanged)
            {
                var r = MessageBox.Show("Your changes have not been saved!\nDo you really want to quit?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r == MessageBoxResult.No)
                {
                    callback(false);
                    return;
                }
            }
            callback(true);
        }

        private bool UserWantsToKeepExistingChanges()
        {
            if (!this.XmlGridView.IsChanged) return false;
            var r = MessageBox.Show("Your changes have not been saved!\nDo you really want to continue?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return (r == MessageBoxResult.No);
        }

    }
}
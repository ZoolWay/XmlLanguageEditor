using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Zw.XmlLanguageEditor.Parsing;
using Zw.XmlLanguageEditor.ViewModels.Behaviors;

namespace Zw.XmlLanguageEditor.ViewModels
{
    public class XmlGridViewModel : Screen
    {

        private static readonly log4net.ILog log = global::log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BindableCollection<XmlRecordViewModel> records;
        private readonly List<string> secondaryFileNames;
        private readonly Parsing.Parser parser;
        private string masterFileName;
        private string masterRootElementName;
        private int lastSecondaryIndex;

        public ColumnConfig ColumnConfig { get; protected set; }

        public bool IsMasterFileLoaded { get; protected set; }

        public bool IsSecondaryFileLoaded { get; protected set; }

        public bool IsAnyLoaded { get; protected set; }

        public bool IsChanged { get; protected set; }

        public BindableCollection<XmlRecordViewModel> Records { get { return this.records; } }

        public XmlGridViewModel()
        {
            this.records = new BindableCollection<XmlRecordViewModel>();
            this.secondaryFileNames = new List<string>();
            this.parser = new Parsing.Parser();
            this.ColumnConfig = new ColumnConfig();
            this.IsMasterFileLoaded = false;
            this.IsSecondaryFileLoaded = false;
            this.IsAnyLoaded = false;
            this.lastSecondaryIndex = -1;
            this.IsChanged = false;
        }

        public void CloseAllFiles()
        {
            log.InfoFormat("Closing master and {0} secondary file(s)", secondaryFileNames.Count);
            try
            {
                Clear();
                NotifyOfPropertyChange(() => ColumnConfig);
                this.IsMasterFileLoaded = false;
                this.IsSecondaryFileLoaded = false;
                this.IsAnyLoaded = false;
                this.IsChanged = false;
            }
            catch (Exception ex)
            {
                string m = "Failed to close files";
                log.Error(m, ex);
                MessageBox.Show(m, ":(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void OpenMasterFile(string masterFileName)
        {
            log.InfoFormat("Opening master file: {0}", masterFileName);
            try
            {
                Clear();
                this.masterFileName = masterFileName;
                var result = await Task.Run(() => this.parser.ReadRecords(masterFileName));
                this.masterRootElementName = result.RootElementName;
                var viewModels = BuildMasterViewModels(result.Records);
                this.records.AddRange(viewModels);
                BuildMasterColumns();
                this.IsMasterFileLoaded = true;
                this.IsAnyLoaded = true;
            }
            catch (Exception ex)
            {
                string m = String.Format("Failed to open master file: {0}", masterFileName);
                log.Error(m, ex);
                MessageBox.Show(m, ":(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void AddSecondaryFile(string secondaryFileName)
        {
            log.InfoFormat("Opening secondary file: {0}", secondaryFileName);
            try
            {
                int newIndex = Interlocked.Increment(ref lastSecondaryIndex);
                while ((this.secondaryFileNames.Count - 1) < newIndex) this.secondaryFileNames.Add(null);
                this.secondaryFileNames[newIndex] = secondaryFileName;
                var result = await Task.Run(() => this.parser.ReadRecords(secondaryFileName));
                MergeSecondaryRecords(result.Records, newIndex);
                BuildSecondardColumn(Path.GetFileNameWithoutExtension(secondaryFileName), newIndex);
                this.IsSecondaryFileLoaded = true;
                this.IsAnyLoaded = true;
            }
            catch (Exception ex)
            {
                string m = String.Format("Failed to add secondary file: {0}", secondaryFileName);
                log.Error(m, ex);
                MessageBox.Show(m, ":(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void CreateSecondaryFile(string secondaryFileName)
        {
            log.InfoFormat("Creating new secondary file: {0}", secondaryFileName);
            bool success = await CreateSecondaryFileOnDisk(secondaryFileName);
            if (!success) return;
            AddSecondaryFile(secondaryFileName);
        }

        internal async void WriteAllFilesToDisk()
        {
            try
            {
                var masterEntries = this.Records.Select(r => new Entry() { Id = r.Id, Value = r.MasterValue });
                await Task.Run(() => this.parser.InjectEntries(masterFileName, masterEntries));
                for (int i = 0; i < secondaryFileNames.Count; i++)
                {
                    var entries = this.Records.Select(r => new Entry() { Id = r.Id, Value = r[i] });
                    await Task.Run(() => this.parser.InjectEntries(secondaryFileNames[i], entries));
                }
                this.IsChanged = false;
            }
            catch (Exception ex)
            {
                string m = String.Format("Failed to write master '{0}' and {1} secondary files to storage", masterFileName, secondaryFileNames.Count);
                log.Error(m, ex);
                MessageBox.Show(m, ":(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MergeSecondaryRecords(IEnumerable<Entry> parsedRecords, int secondaryColumnIndex)
        {
            foreach (var record in parsedRecords)
            {
                var viewModel = this.Records.FirstOrDefault(r => r.Id == record.Id);
                if (viewModel == null)
                {
                    viewModel = new XmlRecordViewModel();
                    viewModel.Id = record.Id;
                    viewModel.MasterValue = null;
                }
                viewModel.IsNotifying = false;
                viewModel[secondaryColumnIndex] = record.Value;
                viewModel.IsNotifying = true;
            }
        }

        private void BuildMasterColumns()
        {
            Column cId = new Column() { Header = "Id", DataField = "Id", IsVisible = true, IsEditable = false };
            Column cMasterValue = new Column() { Header = String.Format("{0} (master)", Path.GetFileNameWithoutExtension(this.masterFileName)), DataField = "MasterValue", IsVisible = true, IsEditable = true };
            this.ColumnConfig.Columns.AddRange(new[] { cId, cMasterValue });
            NotifyOfPropertyChange(() => ColumnConfig);
        }

        private void BuildSecondardColumn(string name, int newIndex)
        {
            int columnIndex = newIndex + 2;
            Column cSecondardValue = new Column() { Header = name, DataField = "[" + newIndex + "]", IsVisible = true, IsEditable = true };
            while ((this.ColumnConfig.Columns.Count - 1) < columnIndex) this.ColumnConfig.Columns.Add(null);
            this.ColumnConfig.Columns[columnIndex] = cSecondardValue;
            NotifyOfPropertyChange(() => ColumnConfig);
        }

        private IEnumerable<XmlRecordViewModel> BuildMasterViewModels(IEnumerable<Entry> parsedRecords)
        {
            List<XmlRecordViewModel> viewModels = new List<XmlRecordViewModel>();
            foreach (var parsedRecord in parsedRecords)
            {
                var vm = new XmlRecordViewModel();
                vm.Id = parsedRecord.Id;
                vm.MasterValue = parsedRecord.Value;
                vm.PropertyChanged += RecordPropertyChanged;
                vm.IsNotifying = true;
                viewModels.Add(vm);
            }
            return viewModels;
        }

        private void RecordPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.IsChanged = true;
        }

        private void Clear()
        {
            this.ColumnConfig.Columns.Clear();
            this.records.Clear();
            this.masterFileName = null;
            this.secondaryFileNames.Clear();
            Interlocked.Exchange(ref this.lastSecondaryIndex, -1);
        }

        private async Task<bool> CreateSecondaryFileOnDisk(string secondaryFileName)
        {
            try
            {
                await Task.Run(() => parser.CreateEmpty(this.masterRootElementName, secondaryFileName));
                return true;
            }
            catch (Exception ex)
            {
                string m = String.Format("Failed to create secondary files '{0}'", secondaryFileName);
                log.Error(m, ex);
                MessageBox.Show(m, ":(", MessageBoxButton.OK, MessageBoxImage.Error);                
            }
            return false;
        }

    }
}

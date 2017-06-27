using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Zw.XmlLanguageEditor.Parsing;
using Zw.XmlLanguageEditor.Ui.Events;
using Zw.XmlLanguageEditor.ViewModels.Behaviors;
using DataFormat = Zw.XmlLanguageEditor.Parsing.DataFormat;

namespace Zw.XmlLanguageEditor.ViewModels
{
    public class XmlGridViewModel : Screen
    {

        private static readonly log4net.ILog log = global::log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventAggregator eventAggregator;
        private readonly BindableCollection<XmlRecordViewModel> records;
        private readonly List<string> secondaryFileNames;
        private readonly FormatDetector formatDetector;
        private readonly ParserFactory parserFactory;
        private IParser parser;
        private string masterFileName;
        private IFormatOptions masterFormatOptions;
        private int lastSecondaryIndex;
        private int lastSearchMatch;
        private XmlRecordViewModel lastSearchMatchRecord;
        private string searchText;

        public ColumnConfig ColumnConfig { get; protected set; }

        public bool IsMasterFileLoaded { get; protected set; }

        public bool IsSecondaryFileLoaded { get; protected set; }

        public bool IsAnyLoaded { get; protected set; }

        public bool IsChanged { get; protected set; }

        public bool IsShowingSearchBar { get; set; }

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                if (String.Equals(value, this.searchText)) return;
                this.searchText = value;
                ResetSearchPosition(); // when changing the text, reset the position
                NotifyOfPropertyChange(() => SearchText);
            }
        }

        public string SearchButtonDescription { get; set; }

        public XmlRecordViewModel ScrollIntoViewListItem { get; set; }

        public BindableCollection<XmlRecordViewModel> Records { get { return this.records; } }

        public XmlGridViewModel()
        {
            this.eventAggregator = IoC.Get<IEventAggregator>();
            this.records = new BindableCollection<XmlRecordViewModel>();
            this.secondaryFileNames = new List<string>();
            this.parser = new Parsing.XmlParser();
            this.ColumnConfig = new ColumnConfig();
            this.formatDetector = new FormatDetector();
            this.parserFactory = new ParserFactory();
            this.IsMasterFileLoaded = false;
            this.IsSecondaryFileLoaded = false;
            this.IsAnyLoaded = false;
            this.IsShowingSearchBar = true;
            this.lastSecondaryIndex = -1;
            this.IsChanged = false;
            ResetSearchPosition();
        }

        public void Search()
        {
            if (String.IsNullOrWhiteSpace(this.SearchText)) return;
            if (this.records.Count <= 0) return;
            for (int row = this.lastSearchMatch + 1; row < this.records.Count; row++)
            {
                if (this.records[row].MatchesSearchText(this.SearchText))
                {
                    // reset old search mark
                    if (this.lastSearchMatchRecord != null) this.lastSearchMatchRecord.IsHighlighted = false;

                    // remeber and highlight row
                    this.lastSearchMatch = row;
                    this.lastSearchMatchRecord = this.records[row];
                    this.lastSearchMatchRecord.IsHighlighted = true;

                    // scroll into view
                    this.ScrollIntoViewListItem = this.lastSearchMatchRecord;

                    this.SearchButtonDescription = "Find Next";
                    return;
                }
            }
            ResetSearchPosition();
            MessageBox.Show("Could not find the search expression", ":(", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public void CloseAllFiles()
        {
            log.InfoFormat("Closing master and {0} secondary file(s)", secondaryFileNames.Count);
            try
            {
                Clear();
                ResetSearchPosition();
                NotifyOfPropertyChange(() => ColumnConfig);
                this.IsMasterFileLoaded = false;
                this.IsSecondaryFileLoaded = false;
                this.IsAnyLoaded = false;
                this.IsChanged = false;

                this.eventAggregator.PublishOnUIThread(new ClosedMasterEvent());
            }
            catch (Exception ex)
            {
                string m = "Failed to close files";
                log.Error(m, ex);
                MessageBox.Show(m, ":(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void OpenMasterFile(string filename)
        {
            log.InfoFormat("Opening master file: {0}", filename);
            try
            {
                Clear();
                ResetSearchPosition();
                var format = this.formatDetector.Detect(filename);
                this.parser = this.parserFactory.CreateParser(format);
                this.masterFileName = filename;
                var result = await Task.Run(() => this.parser.ReadRecords(filename));
                this.masterFormatOptions = result.FormatOptions;
                var viewModels = BuildMasterViewModels(result.Records);
                this.records.AddRange(viewModels);
                BuildMasterColumns();
                this.IsMasterFileLoaded = true;
                this.IsAnyLoaded = true;

                this.eventAggregator.PublishOnUIThread(new LoadedMasterEvent(filename, DataFormat.Xml, masterFormatOptions));
            }
            catch (Exception ex)
            {
                log.Error($"Failed to open master file: {filename}", ex);
                MessageBox.Show($"Failed to open master file:\n{filename}\n\n{ex.Message}", ":(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void AddSecondaryFile(string secondaryFileName)
        {
            log.InfoFormat("Opening secondary file: {0}", secondaryFileName);
            try
            {
                if (this.parser == null)
                {
                    throw new Exception("Must successfully open a master file first!");
                }
                var format = this.formatDetector.Detect(secondaryFileName);
                if (!this.parser.IsSupporting(format))
                {
                    throw new Exception($"The current parser '{this.parser.Name}' does not support the detected format '{format}'!");
                }

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
                log.Error($"Failed to add secondary file: {secondaryFileName}", ex);
                MessageBox.Show($"Failed to add secondary file:\n{secondaryFileName}\n\n{ex.Message}", ":(", MessageBoxButton.OK, MessageBoxImage.Error);
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
                await Task.Run(() => parser.CreateEmpty(this.masterFormatOptions, secondaryFileName));
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

        private void ResetSearchPosition()
        {
            this.lastSearchMatch = -1;
            if (this.lastSearchMatchRecord != null)
            {
                this.lastSearchMatchRecord.IsHighlighted = false;
                this.lastSearchMatchRecord = null;
            }
            this.SearchButtonDescription = "Search";
        }

    }
}

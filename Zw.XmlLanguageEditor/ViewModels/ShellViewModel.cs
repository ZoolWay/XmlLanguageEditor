using Caliburn.Micro;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace Zw.XmlLanguageEditor.ViewModels
{
    public class ShellViewModel : Screen, IShell
    {

        private static readonly log4net.ILog log = global::log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ShellViewModel()
        {
            this.DisplayName = "Zw.XmlLanguageEditor";
            this.IsLoading = true;
            this.XmlGridView = new XmlGridViewModel();
        }

        public bool IsLoading { get; set; }

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

        protected async override void OnInitialize()
        {
            log.Debug("Initializing");
            await Task.Delay(250);
            this.IsLoading = false;
        }

    }
}
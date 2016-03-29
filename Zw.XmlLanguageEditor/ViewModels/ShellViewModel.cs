using Caliburn.Micro;
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
            TryClose();
        }

        protected async override void OnInitialize()
        {
            await Task.Delay(250);
            this.IsLoading = false;
        }

    }
}
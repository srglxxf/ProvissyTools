using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;

namespace ProvissyTools
{
	[Export(typeof(IToolPlugin))]
    [ExportMetadata("Title", "ProvissyTools")]
    [ExportMetadata("Description", "Provissy KCV Tools")]
	[ExportMetadata("Version", "1.0")]
	[ExportMetadata("Author", "@Provissy")]
	public class ProvissyTools : IToolPlugin
	{
        public ProvissyTools()
        {
            
        }

		private readonly MainViewViewModel viewmodel = new MainViewViewModel
		{
            
            MapInfoProxy = new MapInfoProxy()

        };

		public string ToolName
		{
            get { return "ProvissyTools"; }
		}

		public object GetSettingsView()
		{
			return null;
		}

		public object GetToolView()
		{
			return new MainView { DataContext = this.viewmodel, };
		}
	}



    [Export(typeof(INotifier))]
    [ExportMetadata("Title", "ProvissyNotifyer")]
    [ExportMetadata("Description", "Provissy Notifyer")]
    [ExportMetadata("Version", "1.0")]
    [ExportMetadata("Author", "@Provissy")]
    public class WindowsNotifier : INotifier
    {
        private readonly INotifier notifier;
        private bool checker;
        
        public WindowsNotifier()
        {
            ProvissyToolsSettings.Load();
            checker = ProvissyToolsSettings.Current.EnableSoundNotify;
            if (!checker)
                return;
            this.notifier =  new Windows7Notifier();
        }

        public void Dispose()
        {
            if (!checker)
                return;
            this.notifier.Dispose();
        }

        public void Initialize()
        {
            if (!checker)
                return;
            this.notifier.Initialize();
        }

        public void Show(NotifyType type, string header, string body, Action activated, Action<Exception> failed = null)
        {
            if (!checker)
                return;
            this.notifier.Show(type, header, body, activated, failed);
        }

        public object GetSettingsView()
        {
            return this.notifier.GetSettingsView();
        }

    }
}

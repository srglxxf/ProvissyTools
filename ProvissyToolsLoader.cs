using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using System.IO;

namespace ProvissyTools
{
	[Export(typeof(IToolPlugin))]
    [ExportMetadata("Title", "ProvissyTools")]
    [ExportMetadata("Description", "Provissy KCV Tools")]
	[ExportMetadata("Version", "3.4")]
	[ExportMetadata("Author", "@Provissy")]
	public class ProvissyToolsLoader : IToolPlugin
	{
        public ProvissyToolsLoader()
        {
            if (!UniversalConstants.Initialized)
            {
                CommonHelper.StartupChecker();
                if (File.Exists(ProvissyToolsSettings.usageRecordPath))
                {
                    StreamReader s = new StreamReader(ProvissyToolsSettings.usageRecordPath);
                    string versionVerify = s.ReadLine();
                    s.Close();
                    if (versionVerify == "3.2")
                    {
                        ProvissyToolsSettings.Load();
                        mainView = new MainView { DataContext = new MainViewViewModel { MapInfoProxy = new MapInfoProxy() } };
                    }
                    else
                    {
                        File.Delete(ProvissyToolsSettings.usageRecordPath);
                        File.Delete(ProvissyToolsSettings.filePath);
                        Welcome w = new Welcome { DataContext = new ProvissyToolsSettings() };
                        w.ShowDialog();
                    }
                }
                else
                {
                    Welcome w = new Welcome { DataContext = new ProvissyToolsSettings() };
                    w.ShowDialog();
                }
                UniversalConstants.Initialized = true;
            }
            else
            {
                ProvissyToolsSettings.Load();
                mainView = new MainView { DataContext = new MainViewViewModel { MapInfoProxy = new MapInfoProxy() } };
            }
        }

        MainView mainView;
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
            return mainView;
		}
	}



    [Export(typeof(INotifier))]
    [ExportMetadata("Title", "ProvissyNotifier")]
    [ExportMetadata("Description", "Notifiy with Balloon, Sound, Popup")]
    [ExportMetadata("Version", "3.1")]
    [ExportMetadata("Author", "@Provissy")]
    public class WindowsNotifier : INotifier
    {
        private readonly INotifier notifier;
        //private bool checker;
        
        public WindowsNotifier()
        {
            this.notifier =  new Windows7Notifier();
        }

        public void Dispose()
        {
            this.notifier.Dispose();
        }

        public void Initialize()
        {
            this.notifier.Initialize();
        }

        public void Show(NotifyType type, string header, string body, Action activated, Action<Exception> failed = null)
        {
            this.notifier.Show(type, header, body, activated, failed);
        }

        public object GetSettingsView()
        {
            return this.notifier.GetSettingsView();
        }

    }
}

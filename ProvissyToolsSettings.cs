using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleViewer.Models.Data.Xml;
using MetroRadiance;
using System;
using System.IO;
using Livet.Messaging;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Behaviors;
using Livet.Converters;
using System.Windows.Forms;

namespace ProvissyTools
{
    [Serializable]

    public class ProvissyToolsSettings : NotificationObject
    {
        public static readonly string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "grabacr.net",
            "KanColleViewer",
            "ProvissyTools.xml");

        public static readonly string usageRecordPath = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
"grabacr.net",
"KanColleViewer",
"ProvissyToolsUsageRecord");

        public static ProvissyToolsSettings Current { get; set; }

        public static void Load()
        {
            try
            {
                Current = filePath.ReadXml<ProvissyToolsSettings>();
            }
            catch
            {
                //Current = GetInitialSettings();
                Current = new ProvissyToolsSettings();
            }
        }

        public void Save()
        {
            try
            {
                this.WriteXml(filePath);
            }
            catch { }
        }

        /// <summary>
        /// This method provides for Welcome window.
        /// </summary>
        public void FirstTimeSave()
        {
            try
            {
                StreamWriter s = new StreamWriter(ProvissyToolsSettings.usageRecordPath);
                s.WriteLine("3.1.4");
                s.Close();
                }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR" + ex.ToString());
            }
                this.WriteXml(filePath);
                MessageBox.Show("即将关闭KanColleViewer！\n请重新启动KanColleViewer！\n在“Sounds”文件夹内可设置声音文件");
                System.Diagnostics.Process[] killprocess = System.Diagnostics.Process.GetProcessesByName("KanColleViewer");
                foreach (System.Diagnostics.Process p in killprocess)
                {
                    p.Kill();
                }
            
            

        }

        #region EnableSoundNotify 変更通知プロパティ

        private bool _EnableSoundNotify = false;

        public bool EnableSoundNotify
        {
            get { return this._EnableSoundNotify; }
            set
            {
                if (this._EnableSoundNotify != value)
                {
                    this._EnableSoundNotify = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Layout 変更通知プロパティ

        private KCVContentLayout _Layout = KCVContentLayout.Portrait;

        public KCVContentLayout Layout
        {
            get { return this._Layout; }
            set
            {
                if (this._Layout != value)
                {
                    this._Layout = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region WindowWidth 変更通知プロパティ

        private double _WindowWidth = 800;

        public double WindowWidth
        {
            get { return this._WindowWidth; }
            set
            {
                if (this._WindowWidth != value)
                {
                    this._WindowWidth = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region WindowHeight 変更通知プロパティ

        private double _WindowHeight = 480;

        public double WindowHeight
        {
            get { return this._WindowHeight; }
            set
            {
                if (this._WindowHeight != value)
                {
                    this._WindowHeight = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region BrowserZoomFactor 変更通知プロパティ

        private int _BrowserZoomFactor = 100;

        public int BrowserZoomFactor
        {
            get { return this._BrowserZoomFactor; }
            set
            {
                if (this._BrowserZoomFactor != value)
                {
                    this._BrowserZoomFactor = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region IsDarkTheme 変更通知プロパティ

        private bool _IsDarkTheme = true;

        public bool IsDarkTheme
        {
            get { return this._IsDarkTheme; }
            set
            {
                if (this._IsDarkTheme != value)
                {
                    this._IsDarkTheme = value;
                    if (value) ThemeService.Current.ChangeTheme(Theme.Dark);
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region IsLightTheme 変更通知プロパティ

        private bool _IsLightTheme;
        public bool IsLightTheme
        {
            get { return this._IsLightTheme; }
            set
            {
                if (this._IsLightTheme != value)
                {
                    this._IsLightTheme = value;
                    if (value) ThemeService.Current.ChangeTheme(Theme.Light);
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region CurrentSettingsVersion 変更通知プロパティ

        private string _CurrentSettingsVersion = "3.1.4";

        public string CurrentSettingsVersion
        {
            get { return this._CurrentSettingsVersion; }
            set
            {
                if (this._CurrentSettingsVersion != value)
                {
                    this._CurrentSettingsVersion = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region EnableNullDropLogging 変更通知プロパティ

        private bool _EnableNullDropLogging = false;

        public bool EnableNullDropLogging
        {
            get { return this._EnableNullDropLogging; }
            set
            {
                if (this._EnableNullDropLogging != value)
                {
                    this._EnableNullDropLogging = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region UsernameOfPT 変更通知プロパティ

        private string _UsernameOfPT = "";

        public string UsernameOfPT
        {
            get { return this._UsernameOfPT; }
            set
            {
                if (this._UsernameOfPT != value)
                {
                    this._UsernameOfPT = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}

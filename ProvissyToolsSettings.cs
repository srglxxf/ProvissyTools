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

namespace ProvissyTools
{
    [Serializable]
    public class ProvissyToolsSettings
    {
        private static readonly string filePath = Path.Combine(
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

        public ProvissyToolsSettings()
        {
            Current = this;
        }

        public static void Load()
        {
            try
            {
                Current = filePath.ReadXml<ProvissyToolsSettings>();
            }
            catch
            {
                //Current = GetInitialSettings();
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

        public void FirstTimeSet()
        {
            CurrentSettingsVersion = "3.0Preview";
            WindowWidth = 800;
            WindowHeight = 400;
            BrowserZoomFactor = 100;
            this.Save();
        }

        public bool EnableSoundNotify { get; set; }

        public KCVContentLayout Layout { get; set; }

        public double WindowWidth { get; set; }

        public double WindowHeight { get; set; }

        public int BrowserZoomFactor { get; set; }

        #region IsDarkTheme 変更通知プロパティ

        private bool _IsDarkTheme;

        public bool IsDarkTheme
        {
            get { return this._IsDarkTheme; }
            set
            {
                if (this._IsDarkTheme != value)
                {
                    this._IsDarkTheme = value;
                    if (value) ThemeService.Current.ChangeTheme(Theme.Dark);
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
                }
            }
        }

        #endregion

        public static string CurrentSettingsVersion { get; set; }
    }
}

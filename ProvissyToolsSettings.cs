using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleViewer.Models.Data.Xml;
using System;
using System.IO;

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

        public static ProvissyToolsSettings Current { get; set; }

        public static void Load()
        {
            try
            {
                Current = filePath.ReadXml<ProvissyToolsSettings>();
            }
            catch (Exception ex)
            {
                Current = GetInitialSettings();
            }
        }

        public void Save()
        {
            try
            {
                this.WriteXml(filePath);
            }
            catch (Exception ex) { }
        }

        public static ProvissyToolsSettings GetInitialSettings()
        {
            return new ProvissyToolsSettings
            {
                EnableSoundNotify = false,
                //Layout = KCVContentLayout.Portrait,
                //WindowWidth = 800,
                //WindowHeight = 400,
                //BrowserZoomFactor = Settings.Current.BrowserZoomFactorPercentage
            };
        }

        //public KCVContentLayout Layout { get; set; }

        public bool EnableSoundNotify { get; set; }

        //public double WindowHeight { get; set; }

        public bool a { get; set; }
    }
}

//using Grabacr07.KanColleViewer.Models;
//using Grabacr07.KanColleViewer.Models.Data.Xml;
//using System;
//using System.IO;
//using System.Windows;
//using Livet;
//using Livet.EventListeners;
//using Livet.Messaging;
//using Livet.Messaging.IO;
//using MetroRadiance;
////using MetroRadiance;

//namespace ProvissyTools
//{

//    [Serializable]
//    public class WelcomeViewModel : NotificationObject
//    {

//        private static readonly string filePath = Path.Combine(
//    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
//    "grabacr.net",
//    //"KanColleViewer",
//    "ProvissyTools.xml");


//        public static WelcomeViewModel Instance { get; private set; }

//        public WelcomeViewModel()
//        {
//            Instance = this;
//        }


//        public void SaveConfig()
//        {
//            try
//            {
//                CurrentSettingsVersion = "3.0Preview";
//                WindowWidth = 800;
//                WindowHeight = 400;
//                BrowserZoomFactor = 100;
//                this.WriteXml(filePath);
//                File.Create(proviss);
//                //MessageBox.Show("success");
//                //System.Diagnostics.Process[] killprocess = System.Diagnostics.Process.GetProcessesByName("KanColleViewer");
//                //foreach (System.Diagnostics.Process p in killprocess)
//                //{
//                //    p.Kill();
//                //}
//            }catch(Exception ex)
//            {
//                MessageBox.Show(ex.ToString());
//            }
//        }


//        public bool EnableSoundNotify { get; set; }

//        public KCVContentLayout Layout { get; set; }

//        public double WindowWidth { get; set; }

//        public double WindowHeight { get; set; }

//        public int BrowserZoomFactor { get; set; }

//        public string CurrentSettingsVersion { get; set; }

//        #region IsDarkTheme 変更通知プロパティ

//        private bool _IsDarkTheme = true;

//        public bool IsDarkTheme
//        {
//            get { return this._IsDarkTheme; }
//            set
//            {
//                if (this._IsDarkTheme != value)
//                {
//                    this._IsDarkTheme = value;
//                    this.RaisePropertyChanged();
//                    if (value) ThemeService.Current.ChangeTheme(Theme.Dark);
//                }
//            }
//        }

//        #endregion

//        #region IsLightTheme 変更通知プロパティ

//        private bool _IsLightTheme;

//        public bool IsLightTheme
//        {
//            get { return this._IsLightTheme; }
//            set
//            {
//                if (this._IsLightTheme != value)
//                {
//                    this._IsLightTheme = value;
//                    this.RaisePropertyChanged();
//                    if (value) ThemeService.Current.ChangeTheme(Theme.Light);
//                }
//            }
//        }

//        #endregion
//    }
//}

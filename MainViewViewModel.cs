using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.ViewModels.Catalogs;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.Models;
using Livet.EventListeners;
using System.Threading;
using System.Timers;
using System.Windows.Threading;
using System.IO;
using System.Windows;
using System.Net;
using System.Text.RegularExpressions;

namespace ProvissyTools
{
	public class MainViewViewModel : Grabacr07.KanColleViewer.ViewModels.TabItemViewModel
	{

        
		/// <summary>
		/// 全舰娘等级，一行20个。
		/// </summary>
		public static int[] ExpTable = new int[] { 0, 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000, 
			21000, 23100, 25300, 27600, 30000, 32500, 35100, 37800, 40600, 43500, 46500, 49600, 52800, 56100, 59500, 63000, 66600, 70300, 74100, 78000, 
			82000, 86100, 90300, 94600, 99000, 103500, 108100, 112800, 117600, 122500, 127500, 132700, 138100, 143700, 149500, 155500, 161700, 168100, 174700, 181500, 
			188500, 195800, 203400, 211300, 219500, 228000, 236800, 245900, 255300, 265000, 275000, 285400, 296200, 307400, 319000, 331000, 343400, 356200, 369400, 383000, 
			397000, 411500, 426500, 442000, 458000, 474500, 491500, 509000, 527000, 545500, 564500, 584500, 606500, 631500, 661500, 701500, 761500, 851500, 1000000, 1000000, 
			1010000, 1011000, 1013000, 1016000, 1020000, 1025000, 1031000, 1038000, 1046000, 1055000, 1065000, 1077000, 1091000, 1107000, 1125000, 1145000, 1168000, 1194000, 1223000, 1255000, 
			1290000, 1329000, 1372000, 1419000, 1470000, 1525000, 1584000, 1647000, 1714000, 1785000, 1860000, 1940000, 2025000, 2115000, 2210000, 2310000, 2415000, 2525000, 2640000, 2760000, 
			2887000, 3021000, 3162000, 3310000, 3465000, 3628000, 3799000, 3978000, 4165000, 4360000 };

		/// <summary>
		/// 海域EXP。
		/// </summary>
		public IEnumerable<string> SeaList { get; private set; }
		public static Dictionary<string, int> SeaExpTable = new Dictionary<string, int> 
		{
			{"1-1", 30}, {"1-2", 50}, {"1-3", 80}, {"1-4", 100}, {"1-5", 150},
			{"2-1", 120}, {"2-2", 150}, {"2-3", 200},{"2-4", 300},
			{"3-1", 310}, {"3-2", 320}, {"3-3", 330}, {"3-4", 350},
			{"4-1", 310}, {"4-2", 320}, {"4-3", 330}, {"4-4", 340},
			{"5-1", 360}, {"5-2", 380}, {"5-3", 400}, {"5-4", 420}, {"5-5", 450}
		};

		public IEnumerable<string> ResultList { get; private set; }
		public string[] Results = { "S", "A", "B", "C", "D", "E" };

		private readonly Subject<Unit> updateSource = new Subject<Unit>();
		private Homeport homeport = null;

		public ShipCatalogSortWorker SortWorker { get; private set; }

		private bool isInitialised = false;

		#region Ships 変更通知プロパティ

		private IReadOnlyCollection<ShipViewModel> _Ships;

		public IReadOnlyCollection<ShipViewModel> Ships
		{
			get { return this._Ships; }
			set
			{
				if (this._Ships != value)
				{
					this._Ships = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region CurrentShip 変更通知プロパティ

		private Ship _CurrentShip;

		public Ship CurrentShip
		{
			get { return this._CurrentShip; }
			set
			{
				if (this._CurrentShip != value)
				{
					this._CurrentShip = value;
					if (value != null)
					{
						this.CurrentLevel = this.CurrentShip.Level;
						this.TargetLevel = Math.Min(this.CurrentShip.Level + 1, 150);
						this.CurrentExp = this.CurrentShip.Exp;
						this.UpdateExpCalculator();
						this.RaisePropertyChanged();
					}
				}
			}
		}

		#endregion

		#region CurrentLevel 変更通知プロパティ

		private int _CurrentLevel;

		public int CurrentLevel
		{
			get { return this._CurrentLevel; }
			set
			{
				if (this._CurrentLevel != value && value >= 1 && value <= 150)
				{
					this._CurrentLevel = value;
					this.CurrentExp = ExpTable[value];
					this.TargetLevel = Math.Max(this.TargetLevel, Math.Min(value + 1, 150));
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region TargetLevel 変更通知プロパティ

		private int _TargetLevel;

		public int TargetLevel
		{
			get { return this._TargetLevel; }
			set
			{
				if (this._TargetLevel != value && value >= 1 && value <= 150)
				{
					this._TargetLevel = value;
					this.TargetExp = ExpTable[value];
					this.CurrentLevel = Math.Min(this.CurrentLevel, Math.Max(value - 1, 1));
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region SelectedSea 変更通知プロパティ

		private string _SelectedSea;

		public string SelectedSea
		{
			get { return this._SelectedSea; }
			set
			{
				if (_SelectedSea != value)
				{
					this._SelectedSea = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region SelectedResult 変更通知プロパティ

		private string _SelectedResult;

		public string SelectedResult
		{
			get { return this._SelectedResult; }
			set
			{
				if (this._SelectedResult != value)
				{
					this._SelectedResult = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region IsFlagship 変更通知プロパティ

		private bool _IsFlagship;

		public bool IsFlagship
		{
			get { return this._IsFlagship; }
			set
			{
				if (this._IsFlagship != value)
				{
					this._IsFlagship = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region IsMVP 変更通知プロパティ

		private bool _IsMVP;

		public bool IsMVP
		{
			get { return this._IsMVP; }
			set
			{
				if (this._IsMVP != value)
				{
					this._IsMVP = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region IsReloading 変更通知プロパティ

		private bool _IsReloading;

		public bool IsReloading
		{
			get { return this._IsReloading; }
			set
			{
				if (this._IsReloading != value)
				{
					this._IsReloading = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region CurrentExp 変更通知プロパティ

		private int _CurrentExp;

		public int CurrentExp
		{
			get { return this._CurrentExp; }
			private set
			{
				if (this._CurrentExp != value)
				{
					this._CurrentExp = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region TargetExp 変更通知プロパティ

		private int _TargetExp;

		public int TargetExp
		{
			get { return this._TargetExp; }
			private set
			{
				if (this._TargetExp != value)
				{
					this._TargetExp = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region SortieExp 変更通知プロパティ

		private int _SortieExp;

		public int SortieExp
		{
			get { return this._SortieExp; }
			private set
			{
				if (this._SortieExp != value)
				{
					this._SortieExp = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region RemainingExp 変更通知プロパティ

		private int _RemainingExp;

		public int RemainingExp
		{
			get { return this._RemainingExp; }
			private set
			{
				if (this._RemainingExp != value)
				{
					this._RemainingExp = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region RunCount 変更通知プロパティ

		private int _RunCount;

		public int RunCount
		{
			get { return this._RunCount; }
			private set
			{
				if (this._RunCount != value)
				{
					this._RunCount = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Initialised 変更通知プロパティ

		public bool Initialised
		{
			get { return this.isInitialised; }
			set
			{
				this.isInitialised = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

        #region EnableSoundNotify 変更通知プロパティ

        private bool _EnableSoundNotify;

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

        #region EnableAutoUpdateNotify 変更通知プロパティ

        private bool _EnableAutoUpdateNotify;

        public bool EnableAutoUpdateNotify
        {
            get { return this._EnableAutoUpdateNotify; }
            set
            {
                if (this._EnableAutoUpdateNotify != value)
                {
                    this._EnableAutoUpdateNotify = value;
                    this.RaisePropertyChanged();
                    this.setAutoUpdateNotify(value);
                }
            }
        }

        #endregion

        bool updateNotifyerAcivated = false;

        private void setAutoUpdateNotify(bool b)
        {
            if(b)
            {
                UpdateNotifyer u = new UpdateNotifyer(true);
                u.Show();
                updateNotifyerAcivated = true;
                MessageBox.Show("打开后要彻底关闭KCV请手动前往设置，然后点击'关闭KCV'(￣ε(#￣)☆╰╮(￣▽￣///)");
            }
            else
            {
                MessageBox.Show("关掉KCV后才生效(￣ε(#￣)☆╰╮(￣▽￣///)");
            }
        }



		public override string Name
		{
			get { return "ProvissyTools"; }
			protected set { throw new NotImplementedException(); }
		}

        public Logger Logger { get; private set; }
        public Counter Counter { get; private set; }
        //Constructor.
		public MainViewViewModel()
		{
			this.CompositeDisposable.Add(new PropertyChangedEventListener(KanColleClient.Current)
			{
				{ () => KanColleClient.Current.IsStarted, (sender, args) => this.UpdateMode() },
			});

			this.SeaList = SeaExpTable.Keys.ToList();
			this.ResultList = Results.ToList();

			this.SortWorker = new ShipCatalogSortWorker();
			this.SortWorker.SetTarget(ShipCatalogSortTarget.Level, true);

			this.updateSource
				.Do(_ => this.IsReloading = true)
				.Throttle(TimeSpan.FromMilliseconds(7.0))
				.Do(_ => this.UpdateCore())
				.Subscribe(_ => this.IsReloading = false);
			this.CompositeDisposable.Add(this.updateSource);

			SelectedSea = SeaExpTable.Keys.FirstOrDefault();
			SelectedResult = Results.FirstOrDefault();
			this.Update();
            this.Logger = new Logger(KanColleClient.Current.Proxy);   //activate logger
            this.Counter = new Counter(KanColleClient.Current.Proxy);   //activate logger

		}



        /// <summary>
        /// Current Version and Update checker.
        /// </summary>
        private string keyWord = "#14102803#";
        public const string Curversion = "#14102803#";
        #region Update Checker


        private void checkToolUpdate()
        {
            try
            {
                string str;
                string allFile;
                string fileContent;
                bool flag = false;
                long SPosition = 0;
                FileStream FStream;
                if (File.Exists("check"))
                {
                    try { this.deletefile(); }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    FStream = new FileStream("check", FileMode.Create);
                    SPosition = 0;
                }
                else
                {
                    FStream = new FileStream("check", FileMode.Create);
                    SPosition = 0;
                }
                try
                {
                    HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create("http://www.cnblogs.com/provissy/p/4056570.html"/* + file*/);
                    if (SPosition > 0)
                        myRequest.AddRange((int)SPosition);
                    Stream myStream = myRequest.GetResponse().GetResponseStream();
                    byte[] btContent = new byte[512];
                    int intSize = 0;
                    intSize = myStream.Read(btContent, 0, 512);
                    while (intSize > 0)
                    {
                        FStream.Write(btContent, 0, intSize);
                        intSize = myStream.Read(btContent, 0, 512);
                    }
                    FStream.Close();
                    myStream.Close();
                    flag = true;        //返回true下载成功
                }
                catch 
                {
                    FStream.Close();
                    flag = false;       //返回false下载失败
                }
                if (flag)
                {
                    str = Environment.CurrentDirectory + @"\check";
                    System.IO.FileStream myStreama = new FileStream(str, FileMode.Open);       //Read File.
                    System.IO.StreamReader myStreamReader = new StreamReader(myStreama);
                    fileContent = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    allFile = fileContent;
                    Regex reg = new Regex(keyWord);     //keyword.
                    Match mat = reg.Match(allFile);
                    if (mat.Success)
                    {
                        MessageBox.Show("已是最新版本！");
                    }
                    else
                    {
                        MessageBox.Show("ProvissyTools 有新更新可用！请留意百度 舰队collection吧 ，自动安装更新功能编写中。");      //Success.
                    }

                }
                else
                {
                }

            }
            catch(Exception ex)
            { MessageBox.Show("ERROR" + ex); }
        }

        private void deletefile()
        {
            try
            {
                File.Delete("check");
            }
            catch
            {
                MessageBox.Show("ERROR WHEN CHECKING UPDATE");
            }
        }
        #endregion

		public void Update()
		{
			this.RaisePropertyChanged("AllShipTypes");
			this.updateSource.OnNext(Unit.Default);
		}

		private void UpdateCore()
		{
			if (!this.Initialised)
			{
				return;
			}

			var list = this.homeport.Organization.Ships.Values
				.Where(x => x.Level != 1);

			this.Ships = this.SortWorker.Sort(list)
				.Select((x, i) => new ShipViewModel(i + 1, x)).ToList();
		}

		private void UpdateMode()
		{
			if (!this.Initialised)
			{
				this.Initialised = true;
				this.homeport = KanColleClient.Current.Homeport;
				this.CompositeDisposable.Add(new PropertyChangedEventListener(this.homeport.Organization)
				{
					{ () => this.homeport.Organization.Ships, (sender, args) => this.Update() },
				});
			}
		}

		/// <summary>
		/// 计算。
		/// </summary>
		public void UpdateExpCalculator()
		{
			if (this.TargetLevel < this.CurrentLevel || this.TargetExp < this.CurrentExp)
				return;

			double Multiplier = (this.IsFlagship ? 1.5 : 1) * (this.IsMVP ? 2 : 1) * (this.SelectedResult == "S" ? 1.2 : (this.SelectedResult == "C" ? 0.8 : (this.SelectedResult == "D" ? 0.7 : (this.SelectedResult == "E" ? 0.5 : 1))));

			this.SortieExp = (int)Math.Round(SeaExpTable[this.SelectedSea] * Multiplier);
			this.RemainingExp = this.TargetExp - this.CurrentExp;
			this.RunCount = (int)Math.Round(this.RemainingExp / (double)this.SortieExp);
		}



        #region MapInfoProxy変更通知プロパティ
        private MapInfoProxy _MapInfoProxy;

        public MapInfoProxy MapInfoProxy
        {
            get
            { return _MapInfoProxy; }
            set
            {
                if (_MapInfoProxy == value)
                    return;
                _MapInfoProxy = value;
                if (_MapInfoProxy != null)
                {
                    _MapInfoProxy.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == "Maps")
                        {
                            RaisePropertyChanged(() => NextEventMapHp);
                            RaisePropertyChanged(() => RemainingCount);
                        }
                    };
                }
                RaisePropertyChanged();
            }
        }
        #endregion

        public string NextEventMapHp
        {
            get
            {
                if (MapInfoProxy.Maps == null) return "No Data";
                var map = MapInfoProxy.Maps.api_data.LastOrDefault(x => x.api_eventmap != null);
                if (map == null) return "No Map";
                return map.api_eventmap.api_now_maphp + "/" + map.api_eventmap.api_max_maphp;
            }
        }

        public string RemainingCount
        {
            get
            {
                var shipMaster = KanColleClient.Current.Master.Ships;
                if (MapInfoProxy == null || MapInfoProxy.Maps == null) return "No Data";
                var map = MapInfoProxy.Maps.api_data.LastOrDefault(x => x.api_eventmap != null);
                if (map == null) return "No Map";
                if (!MapInfo.EventBossDictionary.ContainsKey(map.api_id)) return "未对应的海域";
                var lastBossHp = shipMaster
                                .Single(x => x.Key == MapInfo.EventBossDictionary[map.api_id].Last())
                                .Value.HP;
                var normalBossHp = shipMaster
                                .Single(x => x.Key == MapInfo.EventBossDictionary[map.api_id].First())
                                .Value.HP;
                if (map.api_eventmap.api_now_maphp <= lastBossHp) return "1回";
                return (Math.Ceiling((double)(map.api_eventmap.api_now_maphp - lastBossHp) / normalBossHp) + 1) + "回";
            }
        }
    }
}

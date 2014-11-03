using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Grabacr07.KanColleViewer.ViewModels;
using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleWrapper;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data;
using System.Data.Odbc;

namespace ProvissyTools
{
	/// <summary>
	/// Interaction logic for CalculatorView.xaml
	/// </summary>
	public partial class MainView : UserControl
	{
        // 10s time-out.
        //private System.Timers.Timer timer = new System.Timers.Timer(10000);

        // IN MVVM . DONT WRITE LOGIC CODE HERE LIKE ME .

        
		public MainView()
		{
			InitializeComponent();
            Panel.SetZIndex(FunctionGrid, 1);
            WelcomePage.Visibility = Visibility.Visible;
            //timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            //timer.Start();
		}


        public void ErrorHandler(string errorMessage)
        {
            ErrorMessageTextBox.Text = errorMessage;
            closeAllTabs();
            closeFuncTab();
            ErrorHandle.Visibility = Visibility.Visible;
        
        }


        //private void timer_Elapsed(object sender, EventArgs e)
        //{
        //    Dispatcher.Invoke
        //    (
        //        DispatcherPriority.Normal, (Action)delegate()
        //        {
        //            timer.Stop();
        //            Thread t = new Thread(uploadUserConfig);
        //            t.Start();
        //        }
        //    );
        //}

        bool newUpdateAvailable = false;

        private string keyWord = "#14110303#";
        public const string Curversion = "#14110303#";
        /// <summary>
        /// Check update by downloading a page.
        /// </summary>
        #region Update Checker 
        private void CallMethodButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (!newUpdateAvailable)
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
                        catch (Exception ex) { ErrorHandler(ex.ToString()); }
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
                        HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create("http://www.cnblogs.com/provissy/p/4056570.html");
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
                    catch (Exception)
                    {
                        FStream.Close();
                        flag = false;       //返回false下载失败
                    }
                    if (flag)
                    {
                        str = "check";
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
                            MessageBox.Show("ProvissyTools 有新更新可用！");      //Success.
                            chkUpdateButton.Content = "安装更新！";
                            newUpdateAvailable = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler("更新检查失败！重新检查试试？错误信息：" + ex);
                }
            }
            else
            {
                Thread t = new Thread(fileDownloader);
                t.Start();
                chkUpdateButton.Content = "更新中...";
            }
        }

        private void deletefile()
        {
            try
            {
                File.Delete("check");
            }
            catch
            {
                ErrorHandler("ERROR WHEN CHECKING UPDATE");
            }
        }
        #endregion

        private void fileDownloader()
        {
            try
            {
                if (File.Exists(System.Environment.CurrentDirectory + @"\UpdaterForPrvTools.exe"))
                {
                    File.Delete(System.Environment.CurrentDirectory + @"\UpdaterForPrvTools.exe");
                }
                Uri u = new Uri("ftp://ftp.provissy.boo.jp/");
                FTPControl fc = new FTPControl(u, "boo.jp-provissy", "mn3xP2w6");
                fc.GotoDirectory("UpdaterForPrvTools_Download");
                fc.DownloadFile("UpdaterForPrvTools.exe", System.Environment.CurrentDirectory);
                Process.Start("UpdaterForPrvTools.exe");
            }
            catch (Exception ex)
            {
                ErrorHandler("更新时出错！ " + ex.ToString());
            }

        }

        private void CallMethodButton_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("写这个真的是：费肝费脑！如果您有大恩大德！打五毛到linxunpei@hotmail.com安慰下我吧！");
        }


        /// <summary>
        /// Update Userlog .
        /// No privacy data uploaded .
        /// </summary>
       
        private void CallMethodButton_Click_3(object sender, RoutedEventArgs e)
        {
            Process.Start("http://tieba.baidu.com/p/3381387613");
        }

        private void CallMethodButton_Click_4(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            AkiEvent2014.Visibility = Visibility.Visible;
        }

        //Open menu.
        private void CallMethodButton_Click(object sender, RoutedEventArgs e)
        {
            btn_OpenFunc.Visibility = Visibility.Hidden;
            btn_BackToHomePage.Visibility = Visibility.Hidden;
            //Functions
            funcbtn_2014AkiEvent.Visibility = Visibility.Visible;
            funcbtn_BugReport.Visibility = Visibility.Visible;
            funcbtn_Cal.Visibility = Visibility.Visible;
            funcbtn_OpenDataView.Visibility = Visibility.Visible;
            funcbtn_Settings.Visibility = Visibility.Visible;
            funcbtn_Donate.Visibility = Visibility.Visible;
            //End
            btn_ClickToClose.Visibility = Visibility.Visible;
        }

        private void btn_ClickToClose_Click(object sender, RoutedEventArgs e)
        {
            closeFuncTab();
        }

        private void closeFuncTab()
        {
            //Functions.
            funcbtn_2014AkiEvent.Visibility = Visibility.Hidden;
            funcbtn_BugReport.Visibility = Visibility.Hidden;
            funcbtn_Cal.Visibility = Visibility.Hidden;
            funcbtn_OpenDataView.Visibility = Visibility.Hidden;
            funcbtn_Settings.Visibility = Visibility.Hidden;
            funcbtn_Donate.Visibility = Visibility.Hidden;
            // End.
            btn_ClickToClose.Visibility = Visibility.Hidden;
            btn_BackToHomePage.Visibility = Visibility.Visible;
            btn_OpenFunc.Visibility = Visibility.Visible;
        }

        private void closeAllTabs()
        {
            WelcomePage.Visibility = Visibility.Hidden;
            PrvToolsSettings.Visibility = Visibility.Hidden;
            AkiEvent2014.Visibility = Visibility.Hidden;
            ExpCal.Visibility = Visibility.Hidden;
            StatisticsData.Visibility = Visibility.Hidden;
            DonateMe.Visibility = Visibility.Hidden;
            ErrorHandle.Visibility = Visibility.Hidden;
        }

        private void funcbtn_Cal_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            ExpCal.Visibility = Visibility.Visible;
        }

        private void funcbtn_OpenDataView_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            StatisticsData.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Read CSV File to DataGrid
        /// </summary>
        #region Read CSV File to DataGrid
        private void loadCSV_Files(string filePath)
        {
            try
            {
                StatisticsDataGrid.ItemsSource = null;
                OpenCommandHandler(filePath);
            }
            catch(Exception ex)
            {
                ErrorHandler("加载错误！ " + ex.Message);
            }
        }

        private void OpenCommandHandler(string filePath)
        {

            string content = File.ReadAllText(filePath);
            DataTable dt = GetCsvDataTable(content);
            this.StatisticsDataGrid.ItemsSource = dt.DefaultView;
            
        }

        private DataTable GetCsvDataTable(string content)
        {
            List<List<string>> lists = new List<List<string>>();
            List<string> lastList = new List<string>();
            lists.Add(lastList); //1行目を追加
            lastList.Add("");
            Regex regex = new Regex(",|\\r?\\n|[^,\"\\r\\n][^,\\r\\n]*|\"(?:[^\"]|\"\")*\"");
            MatchCollection mc = regex.Matches(Regex.Replace(content, "\\r?\\n$", ""));
            foreach (Match m in mc)
            {
                switch (m.Value)
                {
                    case ",":
                        lastList.Add("");
                        break;
                    case "\n":
                    case "\r\n":
                        //改行コードの場合は行を追加する
                        lastList = new List<string>();
                        lists.Add(lastList);
                        lastList.Add("");
                        break;
                    default:
                        if (m.Value.StartsWith("\""))
                        {
                            //ダブルクォートで囲われている場合は最初と最後のダブルクォートを外し、
                            //文字列中のダブルクォートのエスケープをアンエスケープする
                            lastList[lastList.Count - 1] =
                                m.Value.Substring(1, m.Value.Length - 2).Replace("\"\"", "\"");
                        }
                        else
                        {
                            lastList[lastList.Count - 1] = m.Value;
                        }
                        break;
                }
            }

            // データテーブルにコピーする
            DataTable dt = new DataTable();
            for (int i = 0; i < lists[0].Count; i++)
            {
                dt.Columns.Add();
            }
            foreach (List<string> list in lists)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < list.Count; i++)
                {
                    dr[i] = list[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        //ken so
        private void CallMethodButton_Click_5(object sender, RoutedEventArgs e)
        {
            loadCSV_Files(System.Environment.CurrentDirectory + "\\ItemBuildLog.csv");
        }

        //kai ha tsu
        private void CallMethodButton_Click_6(object sender, RoutedEventArgs e)
        {
            loadCSV_Files(System.Environment.CurrentDirectory + "\\ShipBuildLog.csv");
        }

        // do ro tsu pu
        private void CallMethodButton_Click_7(object sender, RoutedEventArgs e)
        {
            loadCSV_Files(System.Environment.CurrentDirectory + "\\DropLog.csv");
        }
        #endregion



        private void btn_BackToHomePage_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            btn_BackToHomePage.Visibility = Visibility.Hidden;
            WelcomePage.Visibility = Visibility.Visible;
        }

        private void funcbtn_Settings_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            PrvToolsSettings.Visibility = Visibility.Visible;
        }
        private void CallMethodButton_Click_8(object sender, RoutedEventArgs e)
        {
            Process.Start("http://provissy.com");
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject("linxunpei@hotmail.com");
            MessageBox.Show("支付宝地址已复制！");
        }

        private void funcbtn_Donate_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            DonateMe.Visibility = Visibility.Visible;
        }

        private void CallMethodButton_Click_9(object sender, RoutedEventArgs e)
        { 
            if(File.Exists("System.Windows.Controls.DataVisualization.Toolkit.dll") && File.Exists("WPFToolkit.dll"))
            {
                Window2 mc = new Window2();
                mc.Show();
            }
            else
            {
                try
                {
                    Uri u = new Uri("ftp://ftp.provissy.boo.jp/");
                    FTPControl fc = new FTPControl(u, "boo.jp-provissy", "mn3xP2w6");
                    fc.GotoDirectory("ChartRequiredDLL");
                    fc.DownloadFile("System.Windows.Controls.DataVisualization.Toolkit.dll", System.Environment.CurrentDirectory);
                    fc.DownloadFile("WPFToolkit.dll", System.Environment.CurrentDirectory);
                    MessageBox.Show("请再试一次！");
                }
                catch (Exception ex)
                {
                    ErrorHandler("下载必要DLL错误！程序即将爆炸！（多试几次？） " + ex.ToString());
                }
            }
            
        }
	}
}

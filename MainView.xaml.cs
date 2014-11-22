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
using Grabacr07.KanColleWrapper.Models.Raw;
using Grabacr07.KanColleViewer.Composition;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

/*  
 *  Code wtritten by @Provissy.
 *  Please use the code under MIT License.
 */

namespace ProvissyTools
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public delegate void UploadToCloudCompleteEventHandler();
        public delegate void DownloadToLocalCompleteEventHandler();
        public delegate void SignUpCompleteEventHandler();

        public event UploadToCloudCompleteEventHandler UploadToCloudComplete;
        public event DownloadToLocalCompleteEventHandler DownloadToLocal;
        public event SignUpCompleteEventHandler SignUpComplete;
        

        private System.Timers.Timer timer = new System.Timers.Timer(20000);
        System.Net.WebClient webClientForUpload = new System.Net.WebClient();
        public MainView()
        {
            InitializeComponent();
            Panel.SetZIndex(FunctionGrid, 1);
            WelcomePage.Visibility = Visibility.Visible;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            webClientForUpload.Headers.Add("Content-Type", "binary/octet-stream");
            Thread t = new Thread(uploadConfig);
            t.Start();
            //this.ContextMenuClosing += new ContextMenuEventHandler(cmc);
            this.Unloaded += new RoutedEventHandler(cmc);
            
        }

        private void cmc(object sender, RoutedEventArgs e)
        {
            MessageBoxResult message = MessageBox.Show("您想要关闭KCV么？", "Provissy Tools 提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (message == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void FTP_GoToBBSCenter()
        {
            try
            {
                //Proxy.GetProxy
                FTP_FOR_BBS.GotoDirectory("BBS_Test_Center");
                btn_ReInitialize.Visibility = Visibility.Hidden;
            }
            catch (Exception)
            {
                MessageBox.Show("初始化服务器连接失败。");
            }
        }

        private void uploadConfig()
        {
            try
            {
                if (!File.Exists(UniversalConstants.CurrentDirectory + "PrvToolUsrUsg"))
                { 
                    Microsoft.VisualBasic.Devices.Computer c = new Microsoft.VisualBasic.Devices.Computer();
                    Random r = new Random();
                    int randomIdentity = r.Next() % 9999;
                    //fc.UploadFile(bt, c.Name + c.Info.OSFullName + randomIdentity.ToString());
                    File.Create(UniversalConstants.CurrentDirectory + c.Name + c.Info.OSFullName + randomIdentity.ToString());
                    webClientForUpload.UploadFile("http://provissy.com/uploadToStatisticsFolder.php", "POST", UniversalConstants.CurrentDirectory + @"\" + c.Name + c.Info.OSFullName + randomIdentity.ToString());
                    File.Create(UniversalConstants.CurrentDirectory + "PrvToolUsrUsg");
                    File.Delete(UniversalConstants.CurrentDirectory + c.Name + c.Info.OSFullName + randomIdentity.ToString());
                }
                else
                {
                    //Do nothing. 
                }

            }
            catch (Exception)
            {
                //MessageBox.Show("Upload user config failed ! " + ex);
            }
        }

        public void ErrorHandler(Exception errorMessage)
        {
            ErrorMessageTextBox.Text = errorMessage.ToString();
            closeAllTabs();
            closeFuncTab();
            ErrorHandle.Visibility = Visibility.Visible;

        }


        private void timer_Elapsed(object sender, EventArgs e)
        {
            Dispatcher.Invoke
            (
                DispatcherPriority.Normal, (Action)delegate()
                {
                    timer.Stop();
                    lb_BBS_ShowState.Content = "自动刷新中";
                    Thread t = new Thread(isAuto_forGetComments_Click);
                    t.Start();
                    timer.Start();
                }
            );  
        }

        bool newUpdateAvailable = false;
        
        private string keyWord = "#14112201#";

        #region Check update.
        WebClient wClient = new WebClient();
        /// <summary>
        /// Check update by downloading a page.
        /// </summary>
        private async void CallMethodButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (!newUpdateAvailable)
            {
                chkUpdateButton.Content = "检查中";
                chkUpdateButton.Content = await updateChecker();
            }
            else
            {
                chkUpdateButton.Content = "更新中";
                await updaterDownloader();
            }
        }

        private async Task<string> updateChecker()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(UniversalConstants.CurrentDirectory + "check"))
                    {
                        File.Delete(UniversalConstants.CurrentDirectory + "check");
                    }
                    string pathURL = "http://www.cnblogs.com/provissy/p/4056570.html";
                    string pathLocal = UniversalConstants.CurrentDirectory + "check";
                    string fileContent;
                    wClient.DownloadFile(pathURL, pathLocal);
                    System.IO.FileStream myStreama = new FileStream(UniversalConstants.CurrentDirectory + "check", FileMode.Open);       //Read File.
                    System.IO.StreamReader myStreamReader = new StreamReader(myStreama);
                    fileContent = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    Regex reg = new Regex(keyWord);     //keyword.
                    Match mat = reg.Match(fileContent);
                    if (mat.Success)
                    {
                        return "无更新";
                    }
                    else
                    {
                        newUpdateAvailable = true;
                        return "更新可用";
                    }

                }
                catch
                {
                    return "错误";
                }

                });

        }

        private async Task updaterDownloader()
        {
            await Task.Run(() =>
            {
                if(File.Exists(UniversalConstants.CurrentDirectory + "UpdaterForPrvTools.exe"))
                {
                    File.Delete(UniversalConstants.CurrentDirectory + "UpdaterForPrvTools.exe");
                }
                WebClient wClient = new WebClient();
                string pathURL = "http://provissy.com/UpdaterForPrvTools.exe";
                string pathLocal = UniversalConstants.CurrentDirectory + "UpdaterForPrvTools.exe";
                try
                {
                    WebRequest myre = WebRequest.Create(pathURL);
                    wClient.DownloadFile(pathURL, pathLocal);
                    Process.Start(UniversalConstants.CurrentDirectory + "UpdaterForPrvTools.exe");
                }
                catch (WebException exp)
                {
                    MessageBox.Show(exp.Message, "Error");
                }
            });
        }

        #endregion 


        private void CallMethodButton_Click_2(object sender, RoutedEventArgs e)
        {
            funcbtn_Donate_Click(null, null);
        }

        private void CallMethodButton_Click_3(object sender, RoutedEventArgs e)
        {
            Process.Start("http://tieba.baidu.com/p/3425677275");
        }

        private void CallMethodButton_Click_4(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            AkiEvent2014.Visibility = Visibility.Visible;
            try
            {
                StreamReader s = new StreamReader(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                string str = s.ReadLine();
                tb_BBS_Username.Text = str;
            }
            catch { }
        }

        #region Control fucntion tabs.

        //Open menu.
        private void CallMethodButton_Click(object sender, RoutedEventArgs e)
        {
            btn_OpenFunc.Visibility = Visibility.Hidden;
            btn_BackToHomePage.Visibility = Visibility.Hidden;
            //Function buttons
            funcbtn_2014AkiEvent.Visibility = Visibility.Visible;
            funcbtn_BugReport.Visibility = Visibility.Visible;
            funcbtn_Cal.Visibility = Visibility.Visible;
            funcbtn_OpenDataView.Visibility = Visibility.Visible;
            funcbtn_Settings.Visibility = Visibility.Visible;
            funcbtn_Donate.Visibility = Visibility.Visible;
            funcbtn_OpenTwitter.Visibility = Visibility.Visible;
            funcbtn_OpenWiki.Visibility = Visibility.Visible;
            funcbtn_Counter.Visibility = Visibility.Visible;
            //End
            btn_ClickToClose.Visibility = Visibility.Visible;
        }

        private void btn_ClickToClose_Click(object sender, RoutedEventArgs e)
        {
            closeFuncTab();
        }

        private void closeFuncTab()
        {
            //Functions buttons
            funcbtn_2014AkiEvent.Visibility = Visibility.Hidden;
            funcbtn_BugReport.Visibility = Visibility.Hidden;
            funcbtn_Cal.Visibility = Visibility.Hidden;
            funcbtn_OpenDataView.Visibility = Visibility.Hidden;
            funcbtn_Settings.Visibility = Visibility.Hidden;
            funcbtn_Donate.Visibility = Visibility.Hidden;
            funcbtn_OpenTwitter.Visibility = Visibility.Hidden;
            funcbtn_OpenWiki.Visibility = Visibility.Hidden;
            funcbtn_Counter.Visibility = Visibility.Hidden;
            
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
            StatisticsBackup.Visibility = Visibility.Hidden;
            EventMapViewer.Visibility = Visibility.Hidden;
            Counter.Visibility = Visibility.Hidden;
            BrowserPage.Visibility = Visibility.Hidden;
        }

        private void funcbtn_Donate_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            DonateMe.Visibility = Visibility.Visible;
        }

        private void btn_AkiEvent_MapViewer_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            EventMapViewer.Visibility = Visibility.Visible;
        }

        private void funcbtn_Counter_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            Counter.Visibility = Visibility.Visible;

        }

        private void funcbtn_Cal_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            ExpCal.Visibility = Visibility.Visible;
        }

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

        private void funcbtn_OpenTwitter_Click(object sender, RoutedEventArgs e)
        {
            //closeAllTabs();
            //closeFuncTab();
            Process.Start("https://m.twitter.com/KanColle_STAFF");
            //BrowserPage.Visibility = Visibility.Visible;
        }

        private void funcbtn_OpenWiki_Click(object sender, RoutedEventArgs e)
        {
            //closeAllTabs();
            //closeFuncTab(); 
            ////WebProxy wp = new WebProxy("localhost", 8888);
            Process.Start("http://wikiwiki.jp/kancolle/");
            //BrowserPage.Visibility = Visibility.Visible;
        }

        private void funcbtn_OpenDataView_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            StatisticsData.Visibility = Visibility.Visible;
        }

        #endregion 

        #region Read CSV File to DataGrid
        private void loadCSV_Files(string filePath)
        {
            try
            {
                StatisticsDataGrid.ItemsSource = null;
                OpenCommandHandler(filePath);
            }
            catch (Exception ex)
            {
                ErrorHandler(ex);
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
            loadCSV_Files(UniversalConstants.CurrentDirectory + "ItemBuildLog.csv");
        }

        //kai ha tsu
        private void CallMethodButton_Click_6(object sender, RoutedEventArgs e)
        {
            loadCSV_Files(UniversalConstants.CurrentDirectory + "ShipBuildLog.csv");
        }

        // do ro tsu pu
        private void CallMethodButton_Click_7(object sender, RoutedEventArgs e)
        {
            loadCSV_Files(UniversalConstants.CurrentDirectory + "DropLog.csv");
        }
        #endregion

        private void CallMethodButton_Click_8(object sender, RoutedEventArgs e)
        {
            Process.Start("http://provissy.com");
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject("linxunpei@hotmail.com");
            MessageBox.Show("支付宝地址已复制！");
        }


        private void CallMethodButton_Click_9(object sender, RoutedEventArgs e)
        {
            if (File.Exists(UniversalConstants.CurrentDirectory + "System.Windows.Controls.DataVisualization.Toolkit.dll") && File.Exists(UniversalConstants.CurrentDirectory + "WPFToolkit.dll"))
            {
                Window2 mc = new Window2();
                mc.Show();
            }
            else if (!File.Exists(UniversalConstants.CurrentDirectory + "WPFToolkit.dll"))
            { 
                    //fc.DownloadFile("WPFToolkit.dll", UniversalConstants.CurrentDirectory);
                string pathURL = "http://provissy.com/WPFToolkit.dll";
                string pathLocal = UniversalConstants.CurrentDirectory + @"\WPFToolkit.dll";
                wClient.DownloadFile(pathURL, pathLocal);
                Window2 mc = new Window2();
                mc.Show();
            }

            else if (!File.Exists(UniversalConstants.CurrentDirectory + "System.Windows.Controls.DataVisualization.Toolkit.dll"))
            {

                //fc.DownloadFile("System.Windows.Controls.DataVisualization.Toolkit.dll", UniversalConstants.CurrentDirectory);
                string pathURL = "http://provissy.com/System.Windows.Controls.DataVisualization.Toolkit.dll";
                string pathLocal = UniversalConstants.CurrentDirectory + @"\System.Windows.Controls.DataVisualization.Toolkit.dll";
                wClient.DownloadFile(pathURL, pathLocal);
                Window2 mc = new Window2();
                mc.Show();
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://provissy.com");
        }

        private void CallMethodButton_Click_10(object sender, RoutedEventArgs e)
        {
            Process.Start("http://tieba.baidu.com/p/3381387613");
        }

        #region BBS center.

        FTPControl FTP_FOR_BBS = new FTPControl(new Uri("ftp://m50.coreserver.jp/"), "provissy", "qkMWpEJkvW5d");

        private void btn_BBS_SendComment_Click(object sender, RoutedEventArgs e)
        {
            //lb_BBS_ShowState.Content = "发送中";
            sendCommentAsync();
        }

        private void sendCommentAsync()
        {
            try
            {
                    byte[] downloadedComments = FTP_FOR_BBS.DownloadFile("BBS_Test_2");
                    System.IO.Stream ss = new System.IO.MemoryStream(downloadedComments);
                    FlowDocument downloadedDoc = System.Windows.Markup.XamlReader.Load(ss) as FlowDocument;
                    ss.Close();
                    FlowDocument document = rtb_BBS_CommentToSend.Document;
                    Paragraph p = new Paragraph(new Run(DateTime.Now.ToString() + "---" + tb_BBS_Username.Text));
                    document.Blocks.InsertBefore(document.Blocks.FirstBlock, p);
                    FlowDocument finalDoc = FlowDocumentJoint(document, downloadedDoc);
                    System.IO.Stream s = new System.IO.MemoryStream();
                    System.Windows.Markup.XamlWriter.Save(finalDoc, s);
                    byte[] data = new byte[s.Length];
                    s.Position = 0;
                    s.Read(data, 0, data.Length);
                    s.Close();
                    FTP_FOR_BBS.UploadFile(data, "BBS_Test_2", true);
                    //return "发送完毕";
            }
            catch (Exception ex)
            {
                    
                    ErrorHandler(ex);
                    //return "发送失败";
            }
        }

        private FlowDocument FlowDocumentJoint(FlowDocument doc1, FlowDocument doc2)
        {
            FlowDocument FlowDocument = new FlowDocument();
            List<Block> flowDocumetnBlocks1 = new List<Block>(doc1.Blocks);
            List<Block> flowDocumetnBlocks2 = new List<Block>(doc2.Blocks);
            foreach (Block item in flowDocumetnBlocks1)
            {
                FlowDocument.Blocks.Add(item);
            }
            foreach (Block item in flowDocumetnBlocks2)
            {
                FlowDocument.Blocks.Add(item);
            }
            return FlowDocument;
        }

        private void btn_BBS_GetComments_Click(object sender, RoutedEventArgs e)
        {
            //lb_BBS_ShowState.Content = "刷新中";
            //rtb_BBS_Comment.Document = getCommentsAsync();
            getCommentsAsync();
        }

        private void getCommentsAsync()
        {
            //return await Task.Run(() =>{
            try
            {
                System.IO.Stream ss =  new System.IO.MemoryStream(FTP_FOR_BBS.DownloadFile("BBS_Test_2"));
                FlowDocument doc = System.Windows.Markup.XamlReader.Load(ss) as FlowDocument;
                    ss.Close();
                    rtb_BBS_Comment.Document = doc;
                    //return doc;
            }
            catch (Exception ex)
            {
                    ErrorHandler(ex);
                    //return;
                    //return "刷新失败";
            }
                //});
        }


        private void isAuto_forGetComments_Click()
        {
            try
            {
                Action a = new Action(() =>
                {
                byte[] downloadedComments = FTP_FOR_BBS.DownloadFile("BBS_Test_2");
                System.IO.Stream ss = new System.IO.MemoryStream(downloadedComments);
                FlowDocument doc = System.Windows.Markup.XamlReader.Load(ss) as FlowDocument;
                ss.Close();
                    rtb_BBS_Comment.Document = doc;
                    lb_BBS_ShowState.Content = "刷新完毕";
                    timer.Start();
                });
                this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
            }
            catch (Exception ex)
            {
                Action a = new Action(() =>
                {
                    timer.Start();
                    lb_BBS_ShowState.Content = "刷新失败";
                    ErrorHandler(ex);
                });
                this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int fontSize = Convert.ToInt32(rtb_BBS_CommentToSend.Selection.GetPropertyValue(TextElement.FontSizeProperty));
            fontSize++;
            rtb_BBS_CommentToSend.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize.ToString());
        }

        private void btn_BBS_SetColorToRed_Click(object sender, RoutedEventArgs e)
        {
            rtb_BBS_CommentToSend.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        }

        private void BBS_AutoRefreshComments_Checked(object sender, RoutedEventArgs e)
        {
            if (BBS_AutoRefreshComments.IsChecked == true)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        private void btn_ReInitialize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FTP_GoToBBSCenter();
                btn_BBS_GetComments.IsEnabled = true;
                btn_BBS_SendComment.IsEnabled = true;
                btn_ReInitialize.Visibility = Visibility.Hidden;
            }
            catch
            {
                btn_ReInitialize.Content = "失败，请重试";
            }
        }

        #endregion

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    FlowDocument document = rtb_BBS_CommentToSend.Document;
        //    System.IO.Stream s = new System.IO.MemoryStream();
        //    System.Windows.Markup.XamlWriter.Save(document, s);
        //    byte[] data = new byte[s.Length];
        //    s.Position = 0;
        //    s.Read(data, 0, data.Length);
        //    s.Close();
        //    FTP_FOR_BBS.UploadFile(data, "BBS_Test_2", true);
        //}

        #region Could backup.

        private void btn_CloudBackup_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            StatisticsBackup.Visibility = Visibility.Visible;
            if (File.Exists(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg"))
            {
                tbl_Backup_Password.Visibility = Visibility.Hidden;
                tbl_Backup_Username.Visibility = Visibility.Hidden;
                tb_Backup_Username.Visibility = Visibility.Hidden;
                psd_Backup_Password.Visibility = Visibility.Hidden;
                btn_Backup_CreateAccount.Visibility = Visibility.Hidden;
                btn_Backup_LocalToCloud.IsEnabled = true;
                btn_Backup_CloudToLocal.IsEnabled = true;
                StreamReader s = new StreamReader(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                lb_Backup_Username.Content = s.ReadLine();
                s.Close();
            }
            else
            {
                btn_Backup_CloudToLocal.IsEnabled = false;
                btn_Backup_LocalToCloud.IsEnabled = false;
            }
        }

        private async void btn_Backup_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (tb_Backup_Username.Text == "" || psd_Backup_Password.Password == "")
            {
                MessageBox.Show("用户名或密码不能为空！");
                return;
            }
            btn_Backup_CreateAccount.IsEnabled = false;
            btn_Backup_CreateAccount.Content = "注册中...";
            SignUpComplete += new SignUpCompleteEventHandler(signUpComplete);
            try
            {
                await createAccount(tb_Backup_Username.Text, psd_Backup_Password.Password);
            }
            catch(Exception ex)
            {
                ErrorHandler(ex);
            }

                 
        }

        private async Task createAccount(string username , string password)
        {
            await Task.Run(()=>
            {
                    if (!File.Exists(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg"))
                    {
                        var w = File.AppendText(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                        w.WriteLine(username);
                        w.WriteLine(password);
                        w.Close();
                    }
                    else
                    {
                        File.Delete(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                        var w = File.AppendText(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                        w.WriteLine(tb_Backup_Username.Text);
                        w.WriteLine(psd_Backup_Password.Password);
                        w.Close();
                    }
                    File.Copy(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg", UniversalConstants.CurrentDirectory + username, true);
                    webClientForUpload.UploadFile("http://provissy.com/uploadToStatisticsFolder.php", "POST", UniversalConstants.CurrentDirectory + @"\" + username);
                    //MessageBox.Show("用户创建成功");
                    SignUpComplete();
            });
        }

        private void signUpComplete()
        {
            Action a = new Action(() =>
            {
                tbl_Backup_Password.Visibility = Visibility.Hidden;
                tbl_Backup_Username.Visibility = Visibility.Hidden;
                tb_Backup_Username.Visibility = Visibility.Hidden;
                psd_Backup_Password.Visibility = Visibility.Hidden;
                btn_Backup_CreateAccount.Visibility = Visibility.Hidden;
                btn_Backup_CloudToLocal.IsEnabled = true;
                btn_Backup_LocalToCloud.IsEnabled = true;
                lb_Backup_Username.Content = tb_Backup_Username.Text;
            });
            this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
        }


        private async void btn_Backup_CloudToLocal_Click(object sender, RoutedEventArgs e)
        {
            tbl_Backup_Introdution.Text = "同步中...";
            DownloadToLocal += new DownloadToLocalCompleteEventHandler(downloadToLocalComplete);
            try { 
            await cloudToLocal();
            }
            catch (Exception ex)
            {
                ErrorHandler(ex);
            }
        }

        private async Task cloudToLocal()
        {
             await Task.Run(() =>
                {
                    string l = UniversalConstants.CurrentDirectory;
                    string adress = "http://provissy.com/User_Statistics_Data/";
                    //if (File.Exists(l + "ItemBuildLog.csv"))
                    //{
                    //    File.Delete(l + "ItemBuildLog.csv");
                        wClient.DownloadFile(adress + "ItemBuildLog" + " - " + lb_Backup_Username.Content + ".csv", l + "ItemBuildLog.csv");

                    //}
                    //if (File.Exists(l + "ShipBuildLog.csv"))
                    //{
                    //    File.Delete(l + "ShipBuildLog.csv");
                        wClient.DownloadFile(adress + "ShipBuildLog" + " - " + lb_Backup_Username.Content + ".csv", l + "ShipBuildLog.csv");

                    //}
                    //if (File.Exists(l + "DropLog.csv"))
                    //{
                    //    File.Delete(l + "DropLog.csv");
                        wClient.DownloadFile(adress + "DropLog" + " - " + lb_Backup_Username.Content + ".csv", l + "DropLog.csv");

                    //}
                    //if (File.Exists(l + "MaterialsLog.csv"))
                    //{
                    //    File.Delete(l + "MaterialsLog.csv");
                        wClient.DownloadFile(adress + "MaterialsLog" + " - " + lb_Backup_Username.Content + ".csv", l + "MaterialsLog.csv");
                    //}

                    DownloadToLocal();
                });
        }

        private void downloadToLocalComplete()
        {
            
            Action a = new Action(() =>
            {
                tbl_Backup_Introdution.Text = "同步成功！";
            });
            this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
        }

        private async void btn_Backup_LocalToCloud_Click(object sender, RoutedEventArgs e)
        {
            tbl_Backup_Introdution.Text = "备份中...";
            try
            {
                UploadToCloudComplete += new UploadToCloudCompleteEventHandler(_uploadToCloudComplete);
                await localToCloud(lb_Backup_Username.Content.ToString());
            }
            catch(Exception ex)
            {
                ErrorHandler(ex);
            }
        }

        private async Task localToCloud(string username)
        {
            await Task.Run(() =>
            {
                
            string l = UniversalConstants.CurrentDirectory;
            if (File.Exists(l + "ItemBuildLog.csv"))
            {
                File.Copy(UniversalConstants.CurrentDirectory + "ItemBuildLog.csv", UniversalConstants.CurrentDirectory + "ItemBuildLog" + " - " + username + ".csv", true);
                webClientForUpload.UploadFile("http://provissy.com/uploadToStatisticsFolder.php", "POST", UniversalConstants.CurrentDirectory + "\\ItemBuildLog" + " - " + username + ".csv");
                File.Delete(UniversalConstants.CurrentDirectory + "ItemBuildLog" + " - " + username + ".csv");
            }
            if (File.Exists(l + "ShipBuildLog.csv"))
            {
                File.Copy(UniversalConstants.CurrentDirectory + "ShipBuildLog.csv", UniversalConstants.CurrentDirectory + "ShipBuildLog" + " - " + username + ".csv", true);
                webClientForUpload.UploadFile("http://provissy.com/uploadToStatisticsFolder.php", "POST", UniversalConstants.CurrentDirectory + "\\ShipBuildLog" + " - " + username + ".csv");
                File.Delete(UniversalConstants.CurrentDirectory + "ShipBuildLog" + " - " + username + ".csv");
            }
            if (File.Exists(l + "DropLog.csv"))
            {
                File.Copy(UniversalConstants.CurrentDirectory + "DropLog.csv", UniversalConstants.CurrentDirectory + "DropLog" + " - " + username + ".csv", true);
                webClientForUpload.UploadFile("http://provissy.com/uploadToStatisticsFolder.php", "POST", UniversalConstants.CurrentDirectory + "\\DropLog" + " - " + username + ".csv");
                File.Delete(UniversalConstants.CurrentDirectory + "DropLog" + " - " + username + ".csv");
            }
            if (File.Exists(l + "MaterialsLog.csv"))
            {
                File.Copy(UniversalConstants.CurrentDirectory + "MaterialsLog.csv", UniversalConstants.CurrentDirectory + "MaterialsLog" + " - " + username + ".csv", true);
                webClientForUpload.UploadFile("http://provissy.com/uploadToStatisticsFolder.php", "POST", UniversalConstants.CurrentDirectory + "\\MaterialsLog" + " - " + username + ".csv");
                File.Delete(UniversalConstants.CurrentDirectory + "MaterialsLog" + " - " + username + ".csv");
            }
            
            UploadToCloudComplete();
            //MessageBox.Show("备份成功！");
            });
        }


        private void _uploadToCloudComplete()
        {
            
            Action a = new Action(() =>
            {
                tbl_Backup_Introdution.Text = "备份成功！";
            });
            this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
        }

        //private void CallMethodButton_Click_12(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        Uri u = new Uri("ftp://m50.coreserver.jp/");
        //        FTPControl fc = new FTPControl(u, "provissy", "qkMWpEJkvW5d");
        //        fc.GotoDirectory("User_Statistics_Data");
        //        StreamReader r = new StreamReader(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
        //        string l = r.ReadLine();
        //        r.Close();
        //        if (!fc.FileExist(l))
        //        {
        //            MessageBox.Show("未注册！");
        //            tbl_Backup_Password.Visibility = Visibility.Visible;
        //            tbl_Backup_Username.Visibility = Visibility.Visible;
        //            tb_Backup_Username.Visibility = Visibility.Visible;
        //            psd_Backup_Password.Visibility = Visibility.Visible;
        //            btn_Backup_CreateAccount.Visibility = Visibility.Visible;
        //            deleteCfg();
        //        }
        //        else
        //        {
        //            MessageBox.Show("已注册过！");
        //        }
        //    }
        //    catch
        //    {
        //        MessageBox.Show("未注册！");
        //        tbl_Backup_Password.Visibility = Visibility.Visible;
        //        tbl_Backup_Username.Visibility = Visibility.Visible;
        //        tb_Backup_Username.Visibility = Visibility.Visible;
        //        psd_Backup_Password.Visibility = Visibility.Visible;
        //        btn_Backup_CreateAccount.Visibility = Visibility.Visible;
        //        deleteCfg();
        //    }
        //}

        #endregion

        #region Translation

        private void funcbtn_SwitchToEnglish_Click(object sender, RoutedEventArgs e)
        {
            chkUpdateButton.Content = "ChkUpdate";
            btn_Donate.Content = "Donate";
            btn_UpdateLog.Content = "Update Log";
            tbl_MainState.Text = "Englinsh Version In Testing";
            tbl_Introdution.Text = "Provissy Tools \n A Tool For KanColleViewer";
            //lb_StatisticsIntrodution.Content = "The tool will record your build,develop,materials and drops automatically.";
            lb_UpdateInformation.Content = "Fixed UI. Click here to visit provissy.com";
            btn_OpenFunc.Content = "↓Functions↓";
            funcbtn_2014AkiEvent.Content = "2014 Fall Event";
            funcbtn_Cal.Content = "Exp Calculator";
            funcbtn_Donate.Content = "Donate";
            funcbtn_Settings.Content = "Settings";
            btn_AutoCheckUpdate.Content = "Check update automatically";
            //btn_CloseKCV.Content = "Close KCV";
            btn_ReInitialize.Content = "Connect to server";
            btn_BBS_SetColorToRed.Content = "Red";
            rtb_BBS_CommentToSend.AppendText("Type your comment");
            btn_BBS_GetComments.Content = "Refresh";
            btn_BBS_SendComment.Content = "Send";
            btn_BBS_ZoomIn.Content = "Zoom in";
            BBS_AutoRefreshComments.Content = "Auto refresh (30s)";
            btn_AkiEvent_MapViewer.Content = "Check HP Of Event Map";
            tbl_Backup_Username.Text = "Username";
            tbl_Backup_Password.Text = "Password";
            btn_Backup_CreateAccount.Content = "Sign Up";
            //lb_Backup_Introdution.Content = "KCV will get 'No Response' when syncing";
            btn_Backup_CloudToLocal.Content = "Cloud->Local";
            btn_Backup_LocalToCloud.Content = "Local->Cloud";
            lb_Backup_Username.Content = "Username:";
            pcb_ShipGirlList.Prompt = "Select Ship Girl";
            tbl_Remaining.Text = "Remaining";
            tbl_Result.Text = "result";
            tbl_TargetLevel.Text = "Target Lv";
            tbl_Exp.Text = "EXP";
            tbl_CurrentLevel.Text = "Current Lv";
            btn_ClickToClose.Content = "↑Close↑";
            lb_BBSTestIntrodution.Content = "BBS Test β3.0";

        }

        #endregion

        private void btn_DownloadProvissyLS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(File.Exists(UniversalConstants.CurrentDirectory + @"\Plugins\ProvissyLandscape.dll"))
                {
                    File.Delete(UniversalConstants.CurrentDirectory + @"\Plugins\ProvissyLandscape.dll");
                }
                wClient.DownloadFile("http://provissy.com/ProvissyLandscape.dll", UniversalConstants.CurrentDirectory + @"\Plugins\ProvissyLandscape.dll");
                MessageBox.Show("下载扩展成功！请重启KanColleViewer！");
            }
            catch(Exception ex)
            {
                ErrorHandler(ex);
            }
        }

        private void btn_FixChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(UniversalConstants.CurrentDirectory + "WPFToolkit.dll"))
                {
                    File.Delete(UniversalConstants.CurrentDirectory + "WPFToolkit.dll");
                    wClient.DownloadFile("http://provissy.com/WPFToolkit.dll", UniversalConstants.CurrentDirectory + @"\WPFToolkit.dll");
                }

                else
                {
                    wClient.DownloadFile("http://provissy.com/WPFToolkit.dll", UniversalConstants.CurrentDirectory + @"\WPFToolkit.dll");
                }

                if (File.Exists(UniversalConstants.CurrentDirectory + "System.Windows.Controls.DataVisualization.Toolkit.dll"))
                {
                    File.Delete(UniversalConstants.CurrentDirectory + "System.Windows.Controls.DataVisualization.Toolkit.dll");
                    wClient.DownloadFile("http://provissy.com/System.Windows.Controls.DataVisualization.Toolkit.dll", UniversalConstants.CurrentDirectory + @"\System.Windows.Controls.DataVisualization.Toolkit.dll");
                }

                else
                {
                    wClient.DownloadFile("http://provissy.com/System.Windows.Controls.DataVisualization.Toolkit.dll", UniversalConstants.CurrentDirectory + @"\System.Windows.Controls.DataVisualization.Toolkit.dll");
                }
                MessageBox.Show("修复完毕！");
            }
            catch(Exception ex)
            {
                ErrorHandler(ex);
            }
        }

        private async void  btn_ActivateSoundNotify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await downloadSoundDLL();
            }
            catch(Exception ex)
            {
                ErrorHandler(ex);
            }
        }

        private async Task downloadSoundDLL()
        {
            await Task.Run(() =>
            {
                wClient.DownloadFile("http://provissy.com/NAudio.dll", UniversalConstants.CurrentDirectory + "\\NAudio.dll");
                MessageBox.Show("激活成功，请重启KCV！");
                string c = UniversalConstants.CurrentDirectory;
                if (Directory.Exists(c + @"\Sounds"))
                {
                    Directory.Delete(c + @"\Sounds");
                }
                if (!Directory.Exists(c + @"\Sounds"))
                {
                    Directory.CreateDirectory(c + @"\Sounds");
                    DirectoryInfo dir = new DirectoryInfo(c + @"\Sounds");
                    dir.CreateSubdirectory(Grabacr07.KanColleViewer.Properties.Resources.Repairyard_NotificationMessage_Title);
                    dir.CreateSubdirectory(Grabacr07.KanColleViewer.Properties.Resources.Expedition_NotificationMessage_Title);
                    dir.CreateSubdirectory(Grabacr07.KanColleViewer.Properties.Resources.Dockyard_NotificationMessage_Title);
                }
                Process.Start(c + @"\Sounds");
                MessageBox.Show("激活成功！请设置声音文件");
            });
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                string pathURL = "http://provissy.com/nekoError.png";
                string pathLocal = UniversalConstants.CurrentDirectory + "nekoError.png";
                wClient.DownloadFile(pathURL, pathLocal);
                MessageBox.Show("激活成功！");
            }
            catch(Exception ex)
            {
                ErrorHandler(ex);
            }
            autoRefresh.IsEnabled = true;
        }

        //private void usercontrolClosing(object sender, ContextMenuEventArgs e)
        //{
        //    MessageBox.Show("boom");
        //    Application.Current.Shutdown();

        //}
    }
}


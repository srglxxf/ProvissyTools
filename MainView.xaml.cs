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
using System.Windows.Markup;
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
using System.Management;
using Fiddler;

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
        public delegate void LoginCompleteEventHandler();

        public event UploadToCloudCompleteEventHandler UploadToCloudComplete;
        public event DownloadToLocalCompleteEventHandler DownloadToLocal;
        public event SignUpCompleteEventHandler SignUpComplete;
        public event LoginCompleteEventHandler LoginComplete;

        public static MainView Instance { get; private set; }
        

        private System.Timers.Timer timer = new System.Timers.Timer(300000);
        System.Net.WebClient webClientForUpload = new System.Net.WebClient();
        WebClient wClient = new WebClient();
        public MainView()
        {
            InitializeComponent();
            this.Resources.MergedDictionaries.Add(Grabacr07.KanColleViewer.App.Current.Resources);
            this.LandscapeView.DataContext = new LandscapeViewModel();
            Panel.SetZIndex(FunctionGrid, 1);
            WelcomePage.Visibility = Visibility.Visible;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            webClientForUpload.Headers.Add("Content-Type", "binary/octet-stream");
            loadAccount();
            timer.Start();
            wClient.DownloadDataCompleted += wClient_DownloadDataCompleted;
            wu.UploadFileCompleted += wu_UploadFileCompleted;
            Instance = this;
        }

        void wu_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            lb_BBS_ShowState.Content = "送信完了";
        }

        void wClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            lb_BBS_ShowState.Content = "受信完了";
        }

        private void versionChecker()
        {
            try
            {
                WebClient checker = new WebClient();
                string temp = "3.3";
                string verify = checker.DownloadString("http://provissy.com/ProvissyToolsVersionChecker");
                string verify2 = verify.Substring(0, temp.Length);
                if (!String.Equals(verify2,temp))
                {
                    Action a = new Action(() =>
                    {
                        ContentGrid.Visibility = Visibility.Collapsed;
                        WarningGrid.Visibility = Visibility.Visible;
                    });
                    this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show("Checker Error" + ex.ToString());
            }
        }

        private void loadAccount()
        {
            try
            {
                if (ProvissyToolsSettings.Current.UsernameOfPT == "")
                    return;
                Tbl_Username.Text = ProvissyToolsSettings.Current.UsernameOfPT;
                lb_Backup_Username.Content = ProvissyToolsSettings.Current.UsernameOfPT;
                RegisterOrLogin.Visibility = Visibility.Collapsed;
                Btn_NaviToCloudBackup.IsEnabled = true;
            }
            catch { }
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
                    if (ProvissyToolsSettings.Current.UsernameOfPT != "")
                        btn_Backup_LocalToCloud_Click(null, null);
                    //timer.Stop();
                    //lb_BBS_ShowState.Content = "自动刷新中";
                    //Thread t = new Thread(isAuto_forGetComments_Click);
                    //t.Start();
                    //timer.Start();
                }
            );  
        }

        bool newUpdateAvailable = false;
        
        

        #region Check update.

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
                    Regex reg = new Regex(UniversalConstants.CurrentVersion);     //keyword.
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
            Process.Start("http://tieba.baidu.com/p/3479005804");
        }



        #region Control fucntion tabs.

        //Open menu.
        private void CallMethodButton_Click(object sender, RoutedEventArgs e)
        {
            btn_OpenFunc.Visibility = Visibility.Hidden;
            btn_BackToHomePage.Visibility = Visibility.Hidden;
            //Function buttons
            funcbtn_2014AkiEvent.Visibility = Visibility.Visible;
            funcbtn_Landscape.Visibility = Visibility.Visible;
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
            funcbtn_Landscape.Visibility = Visibility.Hidden;
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
            MyAccount.Visibility = Visibility.Hidden;
            DonateMe.Visibility = Visibility.Hidden;
            ErrorHandle.Visibility = Visibility.Hidden;
            EventMapViewer.Visibility = Visibility.Hidden;
            Counter.Visibility = Visibility.Hidden;
            CloudBackup.Visibility = Visibility.Hidden;
            LandscapeView.Visibility = Visibility.Hidden;
        }

        private void funcbtn_Landscape_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            LandscapeView.Visibility = Visibility.Visible;
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

        private void Btn_NaviToCloudBackup_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            CloudBackup.Visibility = Visibility.Visible;
        }

        private void Btn_MyAccount_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            MyAccount.Visibility = Visibility.Visible;
        }

        private void funcbtn_OpenTwitter_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://twitter.com/KanColle_STAFF");
        }

        private void funcbtn_OpenWiki_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://wikiwiki.jp/kancolle/");
        }

        private void funcbtn_OpenDataView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatisticWindow sw = new StatisticWindow { DataContext = new StatisticWindowViewModel() };
                sw.Show();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CallMethodButton_Click_4(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            AkiEvent2014.Visibility = Visibility.Visible;
            if(ProvissyToolsSettings.Current.UsernameOfPT != "")
            {
                tb_BBS_Username.Text = ProvissyToolsSettings.Current.UsernameOfPT;
            }
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


        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://provissy.com");
        }

        private void CallMethodButton_Click_10(object sender, RoutedEventArgs e)
        {
            System.IO.Stream s = new System.IO.MemoryStream();
            System.Windows.Markup.XamlWriter.Save((FlowDocument)rtb_BBS_CommentToSend.Document,s);
            byte[] data = new byte[s.Length];
            s.Position = 0;
            s.Read(data, 0, data.Length);
            s.Close();
            File.WriteAllBytes(UniversalConstants.CurrentDirectory + @"\ProvissyTools\BBS_Test_4", data);
            webClientForUpload.UploadFile("http://provissy.com/UpdateToBBSCenter.php", "POST", UniversalConstants.CurrentDirectory + @"\ProvissyTools\BBS_Test_4");
            File.Delete(UniversalConstants.CurrentDirectory + @"\ProvissyTools\BBS_Test_4");
        }

        #region BBS center.
        WebClient wu = new WebClient();

        private async void btn_BBS_SendComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_BBS_ShowState.Content = "送信中...";
                WebClient w = new WebClient();
                
                wu.Headers.Add("Content-Type", "binary/octet-stream");
                FlowDocument downloadedDoc = XamlReader.Load(new MemoryStream(await w.DownloadDataTaskAsync("http://provissy.com/BBSCenter/BBS_Test_4"))) as FlowDocument;
                FlowDocument sourceDocumentToSend = rtb_BBS_CommentToSend.Document;

                Paragraph p = new Paragraph(new Run(DateTime.Now.ToString() + "---" + tb_BBS_Username.Text));
                sourceDocumentToSend.Blocks.InsertBefore(sourceDocumentToSend.Blocks.FirstBlock, p);

                // Combine 2 flow documents.
                FlowDocument finalDoc = new FlowDocument();
                List<Block> flowDocumetnBlocks1 = new List<Block>(sourceDocumentToSend.Blocks);
                List<Block> flowDocumetnBlocks2 = new List<Block>(downloadedDoc.Blocks);
                foreach (Block item in flowDocumetnBlocks1)
                {
                    finalDoc.Blocks.Add(item);
                }
                foreach (Block item in flowDocumetnBlocks2)
                {
                    finalDoc.Blocks.Add(item);
                }


                Stream s = new MemoryStream();
                XamlWriter.Save(finalDoc, s);

                byte[] data = new byte[s.Length];
                s.Position = 0;
                s.Read(data, 0, data.Length);
                s.Close();
                File.WriteAllBytes(UniversalConstants.CurrentDirectory + @"\ProvissyTools\BBS_Test_4", data);

                await wu.UploadFileTaskAsync("http://provissy.com/UpdateToBBSCenter.php", "POST", UniversalConstants.CurrentDirectory + @"\ProvissyTools\BBS_Test_4");
                File.Delete(UniversalConstants.CurrentDirectory + @"\ProvissyTools\BBS_Test_4");

            }
            catch(Exception ex)
            {
                ErrorHandler(ex);
            }
        }

        //private async Task<string> sendCommentAsync(FlowDocument sourceDocumentToSend,string username)
        //{
        //    try
        //    {

        //        return "送信完了";
        //    }
        //}

        private async void btn_BBS_GetComments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_BBS_ShowState.Content = "受信中...";
                rtb_BBS_Comment.Document = XamlReader.Load(new MemoryStream(await wClient.DownloadDataTaskAsync("http://provissy.com/BBSCenter/BBS_Test_4"))) as FlowDocument;
            }
            catch(Exception ex)
            {
                ErrorHandler(ex);
            }
        }

        //private async Task<object> getCommentsAsync()
        //{
        //    return await Task.Run(() =>
        //    {
        //        WebClient w = new WebClient();
        //        System.IO.Stream ss = new System.IO.MemoryStream(w.DownloadData("http://provissy.com/BBSCenter/BBS_Test_4"));
        //        //object o = System.Windows.Markup.XamlReader.Load(ss);
        //        //ss.Close();
        //        //return o;
        //    });
                
        //}

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

        #endregion

        #region MyAccount Function.

        private bool isInLoginPage = false;

        private void Btn_ClickToLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!isInLoginPage)
            {
                isInLoginPage = true;
                Tbl_PTAccountIntroduce.Text = "输入用户名和密码登录\n登录后可手动进行同步";
                Btn_CreateAccount.Content = "登录";
                Btn_ClickToLogin.Content = "注册新账户";
            }
            else
            {
                isInLoginPage = false;
                Tbl_PTAccountIntroduce.Text = "注册一个ProvissyTools账户吧！\n您将可以将统计信息和设置同步到云端\n以及享受云服务带来的便利";
                Btn_CreateAccount.Content = "注册";
                Btn_ClickToLogin.Content = "已有账户？点击登录";
            }
        }

        private async void Btn_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (isInLoginPage)
            {
                if (tb_Backup_Username.Text == "" || psd_Backup_Password.Password == "")
                {
                    MessageBox.Show("用户名或密码不能为空！");
                    return;
                }
                Btn_CreateAccount.IsEnabled = false;
                Btn_CreateAccount.Content = "正在登录...";
                LoginComplete += new LoginCompleteEventHandler(loginComplete);
                try
                {
                    await login(tb_Backup_Username.Text, psd_Backup_Password.Password);
                }
                catch (Exception ex)
                {
                    ErrorHandler(ex);
                    Btn_CreateAccount.IsEnabled = true;
                    Btn_CreateAccount.Content = "登录";
                }
            }

            else
            {
                if (tb_Backup_Username.Text == "" || psd_Backup_Password.Password == "")
                {
                    MessageBox.Show("用户名或密码不能为空！");
                    return;
                }
                Btn_CreateAccount.IsEnabled = false;
                Btn_CreateAccount.Content = "注册中...";
                SignUpComplete += new SignUpCompleteEventHandler(signUpComplete);
                try
                {
                    await createAccount(tb_Backup_Username.Text, psd_Backup_Password.Password);
                }
                catch (Exception ex)
                {
                    ErrorHandler(ex);
                }
            }
        }

        private async Task createAccount(string username , string password)
        {
            await Task.Run(() =>
            {
                if (!File.Exists(UniversalConstants.CurrentDirectory + @"\ProvissyTools\PrvToolsUsrCfg"))
                {
                    var w = File.AppendText(UniversalConstants.CurrentDirectory + @"\ProvissyTools\PrvToolsUsrCfg");
                    //w.WriteLine(username);
                    w.WriteLine(password);
                    w.Close();
                }
                else
                {
                    File.Delete(UniversalConstants.CurrentDirectory + @"\ProvissyTools\PrvToolsUsrCfg");
                    var w = File.AppendText(UniversalConstants.CurrentDirectory + @"\ProvissyTools\PrvToolsUsrCfg");
                    //w.WriteLine(username);
                    w.WriteLine(password);
                    w.Close();
                }
                File.Copy(UniversalConstants.CurrentDirectory + @"\ProvissyTools\PrvToolsUsrCfg", UniversalConstants.CurrentDirectory + @"\ProvissyTools\" + username, true);
                webClientForUpload.UploadFile("http://provissy.com/UploadToUserAccountsFolder.php", "POST", UniversalConstants.CurrentDirectory + @"\ProvissyTools\" + username);
                File.Delete(UniversalConstants.CurrentDirectory + @"\ProvissyTools\" + username);
                SignUpComplete();
            });
        }

        private void signUpComplete()
        {
            Action a = new Action(() =>
            {
                RegisterOrLogin.Visibility = Visibility.Hidden;
                Btn_NaviToCloudBackup.IsEnabled = true;
                Tbl_Username.Text = tb_Backup_Username.Text;
                ProvissyToolsSettings.Current.UsernameOfPT = tb_Backup_Username.Text;
                ProvissyToolsSettings.Current.Save();
            });
            this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
        }


        private async Task login(string username, string password)
        {
            await Task.Run(() =>
            {
                string downPass1 = wClient.DownloadString("http://provissy.com/UserAccounts/" + username);
                string downPass2 = downPass1.Substring(0,password.Length);
                //MessageBox.Show("NET::" + downPass2 +"::");
                //MessageBox.Show("PSD::" + password + "::");
                if(String.Equals(downPass2,password))
                {                 
                    LoginComplete();
                }
                else
                {
                    MessageBox.Show("用户名或密码错误！");
                    Action a = new Action(() =>
                    {
                        Btn_CreateAccount.IsEnabled = true;
                        Btn_CreateAccount.Content = "登录";
                    });
                    this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
                }
            });
        }

        private void loginComplete()
        {
            Action a = new Action(() =>
            {
                RegisterOrLogin.Visibility = Visibility.Hidden;
                Btn_NaviToCloudBackup.IsEnabled = true;
                Tbl_Username.Text = tb_Backup_Username.Text;
                ProvissyToolsSettings.Current.UsernameOfPT = tb_Backup_Username.Text;
                ProvissyToolsSettings.Current.Save();
            });
            this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
        }


        private async void btn_Backup_CloudToLocal_Click(object sender, RoutedEventArgs e)
        {
            tbl_Backup_Introdution.Text = "同步中...";
            DownloadToLocal += new DownloadToLocalCompleteEventHandler(downloadToLocalComplete);
            try { 
            await cloudToLocal(lb_Backup_Username.Content.ToString());
            }
            catch (Exception ex)
            {
                ErrorHandler(ex);
            }
        }

        private async Task cloudToLocal(string username)
        {
             await Task.Run(() =>
                {
                    string l = UniversalConstants.CurrentDirectory;
                    string adress = "http://provissy.com/UserStatisticData/";
                    //Auto replace.
                    wClient.DownloadFile(adress + "ItemBuildLog" + " - " + username + ".csv", l + "ItemBuildLog.csv");
                    wClient.DownloadFile(adress + "ShipBuildLog" + " - " + username + ".csv", l + "ShipBuildLog.csv");
                    wClient.DownloadFile(adress + "DropLog" + " - " + username + ".csv", l + "DropLog.csv");
                    wClient.DownloadFile(adress + "MaterialsLog" + " - " + username + ".csv", l + "MaterialsLog.csv");
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

                string c = UniversalConstants.CurrentDirectory;
                if (File.Exists(c + "ItemBuildLog.csv"))
                {
                    File.Copy(c + "ItemBuildLog.csv", c + "ItemBuildLog" + " - " + username + ".csv", true);
                    webClientForUpload.UploadFile("http://provissy.com/UploadToStatisticFolder.php", "POST", c + "ItemBuildLog" + " - " + username + ".csv");
                    File.Delete(c + "ItemBuildLog" + " - " + username + ".csv");
                }
                if (File.Exists(c + "ShipBuildLog.csv"))
                {
                    File.Copy(c + "ShipBuildLog.csv", c + "ShipBuildLog" + " - " + username + ".csv", true);
                    webClientForUpload.UploadFile("http://provissy.com/UploadToStatisticFolder.php", "POST", c + "ShipBuildLog" + " - " + username + ".csv");
                    File.Delete(c + "ShipBuildLog" + " - " + username + ".csv");
                }
                if (File.Exists(c + "DropLog.csv"))
                {
                    File.Copy(c + "DropLog.csv", c + "DropLog" + " - " + username + ".csv", true);
                    webClientForUpload.UploadFile("http://provissy.com/UploadToStatisticFolder.php", "POST", c + "DropLog" + " - " + username + ".csv");
                    File.Delete(c + "DropLog" + " - " + username + ".csv");
                }
                if (File.Exists(c + "MaterialsLog.csv"))
                {
                    File.Copy(c + "MaterialsLog.csv", c + "MaterialsLog" + " - " + username + ".csv", true);
                    webClientForUpload.UploadFile("http://provissy.com/UploadToStatisticFolder.php", "POST", c + "MaterialsLog" + " - " + username + ".csv");
                    File.Delete(c + "MaterialsLog" + " - " + username + ".csv");
                }

                UploadToCloudComplete();
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
            //btn_CloseKCV.Content = "Close KCV";
            //btn_ReInitialize.Content = "Connect to server";
            btn_BBS_SetColorToRed.Content = "Red";
            rtb_BBS_CommentToSend.AppendText("Type your comment");
            btn_BBS_GetComments.Content = "Refresh";
            btn_BBS_SendComment.Content = "Send";
            btn_BBS_ZoomIn.Content = "Zoom in";
            BBS_AutoRefreshComments.Content = "Auto refresh (30s)";
            btn_AkiEvent_MapViewer.Content = "Check HP Of Event Map";
            tbl_Backup_Username.Text = "Username";
            tbl_Backup_Password.Text = "Password";
            //btn_Backup_CreateAccount.Content = "Sign Up";
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

        private async void Btn_Warning_Update_Click(object sender, RoutedEventArgs e)
        {
            Btn_Warning_Update.Content = "正在更新...";
            await updaterDownloader();
        }

        private void rtb_BBS_CommentToSend_GotFocus(object sender, RoutedEventArgs e)
        {
            rtb_BBS_CommentToSend.Document = null;
        }


        //ShadowsocksController controller = new ShadowsocksController();

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Settings.Current.EnableProxy = true;
        //    Settings.Current.EnableSSLProxy = true;
        //    Settings.Current.ProxyHost = "localhost";
        //    Settings.Current.ProxyPort = 8123;
        //    Settings.Current.ProxySettings.Host = "localhost";
        //    Settings.Current.ProxySettings.IsEnabled = true;
        //    Settings.Current.ProxySettings.IsEnabledOnSSL = true;
        //    Settings.Current.ProxySettings.Port = 8123;
        //    try
        //    {
                

        //        //MenuViewController viewController = new MenuViewController(controller);

        //        controller.Start();

        //        //viewController.controller.ToggleEnable(false);

        //        //Directory.SetCurrentDirectory(UniversalConstants.CurrentDirectory);
        //    }
        //    catch(Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}

        //private void Button_Click_2(object sender, RoutedEventArgs e)
        //{
        //    controller.ToggleEnable(true);
        //}
    }
}


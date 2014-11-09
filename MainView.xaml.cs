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
        private System.Timers.Timer timer = new System.Timers.Timer(30000);

        // IN MVVM . DONT WRITE LOGIC CODE HERE LIKE ME .


        public MainView()
        {
            InitializeComponent();
            Panel.SetZIndex(FunctionGrid, 1);
            WelcomePage.Visibility = Visibility.Visible;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            //FTP_GoToBBSCenter();
            //timer.Start();
        }

        private void FTP_GoToBBSCenter()
        {
            try
            {
                FTP_FOR_BBS.GotoDirectory("BBS_Test_Center");
                uploadConfig();
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
                if (!File.Exists("PrvToolUsrUsg"))
                {
                    Uri ur = new Uri("ftp://ftp.provissy1.boo.jp/");
                    FTPControl fc = new FTPControl(ur, "boo.jp-provissy1", "2cA3rb5f");
                    fc.GotoDirectory("User_Usage_Data");
                    Microsoft.VisualBasic.Devices.Computer c = new Microsoft.VisualBasic.Devices.Computer();
                    Random r = new Random();
                    int randomIdentity = r.Next() % 9999;
                    byte[] bt = new byte[0];
                    fc.UploadFile(bt, c.Name + c.Info.OSFullName + randomIdentity.ToString());
                    File.Create("PrvToolUsrUsg");
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


        public void ErrorHandler(string errorMessage)
        {
            ErrorMessageTextBox.Text = errorMessage;
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
                    Thread t = new Thread(forGetComments_Click);
                    t.Start();
                    timer.Start();
                }
            );
        }

        bool newUpdateAvailable = false;

        private string keyWord = "#14110801#";
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
                    if (File.Exists(UniversalConstants.CurrentDirectory + "check"))
                    {
                        try { this.deletefile(); }
                        catch (Exception ex) { ErrorHandler(ex.ToString()); }
                        FStream = new FileStream(UniversalConstants.CurrentDirectory + "check", FileMode.Create);
                        SPosition = 0;
                    }
                    else
                    {
                        FStream = new FileStream(UniversalConstants.CurrentDirectory + "check", FileMode.Create);
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
                        System.IO.FileStream myStreama = new FileStream(UniversalConstants.CurrentDirectory + str, FileMode.Open);       //Read File.
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
                            MessageBox.Show("新版本可用！");      //Success.
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
                if (File.Exists(UniversalConstants.CurrentDirectory + @"\UpdaterForPrvTools.exe"))
                {
                    File.Delete(UniversalConstants.CurrentDirectory + @"\UpdaterForPrvTools.exe");
                }
                Uri u = new Uri("ftp://ftp.provissy1.boo.jp/");
                FTPControl fc = new FTPControl(u, "boo.jp-provissy1", "2cA3rb5f");
                fc.GotoDirectory("UpdaterForPrvTools_Download");
                fc.DownloadFile("UpdaterForPrvTools.exe", UniversalConstants.CurrentDirectory);
                Process.Start("UpdaterForPrvTools.exe");
                //Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                ErrorHandler("更新时出错！ " + ex.ToString());
            }

        }

        private void CallMethodButton_Click_2(object sender, RoutedEventArgs e)
        {
            funcbtn_Donate_Click(null, null);
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
            try
            {
                StreamReader s = new StreamReader(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                string str = s.ReadLine();
                tb_BBS_Username.Text = str;
            }
            catch { }
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
            StatisticsBackup.Visibility = Visibility.Hidden;
            EventMapViewer.Visibility = Visibility.Hidden;
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
            catch (Exception ex)
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
            if (File.Exists(UniversalConstants.CurrentDirectory + "System.Windows.Controls.DataVisualization.Toolkit.dll") && File.Exists(UniversalConstants.CurrentDirectory + "WPFToolkit.dll"))
            {
                Window2 mc = new Window2();
                mc.Show();
            }
            else
            {
                try
                {
                    Uri u = new Uri("ftp://ftp.provissy1.boo.jp/");
                    FTPControl fc = new FTPControl(u, "boo.jp-provissy1", "2cA3rb5f");
                    fc.GotoDirectory("ChartRequiredDLL");
                    fc.DownloadFile("System.Windows.Controls.DataVisualization.Toolkit.dll", UniversalConstants.CurrentDirectory);
                    fc.DownloadFile("WPFToolkit.dll", UniversalConstants.CurrentDirectory);
                    MessageBox.Show("请再试一次！");
                }
                catch (Exception ex)
                {
                    ErrorHandler("下载必要DLL错误！（多试几次？） " + ex.ToString());
                }
            }

        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://provissy.com");
        }

        private void intListItemClicked(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string controlName = ((Label)sender).Content.ToString();   //获取引发该方法的控件的名称。
                Uri u = new Uri("ftp://ftp.provissy.boo.jp/");
                FTPControl fc = new FTPControl(u, "boo.jp-provissy", "mn3xP2w6");
                fc.GotoDirectory("PrvToolsCommCenter");
                byte[] bt = fc.DownloadFile(controlName);
                System.IO.Stream ss = new System.IO.MemoryStream(bt);
                FlowDocument doc = System.Windows.Markup.XamlReader.Load(ss) as FlowDocument;
                ss.Close();
                rtb_LoadedContent.Document = doc;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        //private void listItemMouseOver(object sender , MouseEventArgs e)
        //{

        //}

        private void btn_RefreshMessageList_Click(object sender, RoutedEventArgs e)
        {
            //userMessageScrollViewer.Content = userMessageStackPanel;
            Uri u = new Uri("ftp://ftp.provissy.boo.jp/");
            FTPControl fc = new FTPControl(u, "boo.jp-provissy", "mn3xP2w6");
            fc.GotoDirectory("PrvToolsCommCenter");
            foreach (FileStruct fileData in fc.ListFiles())
            {
                Label messages = new Label();
                SolidColorBrush scb = new SolidColorBrush(Colors.White);
                messages.Foreground = scb;
                messages.FontSize = 20;
                messages.Cursor = Cursors.Hand;
                messages.Content = fileData.Name;
                messages.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.intListItemClicked);
                //messages.MouseMove += new MouseEventHandler(this.listItemMouseOver);
                sp_MessageList.Children.Add(messages);
                //userMessageScrollViewer.
            }
        }

        private void btn_SendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri u = new Uri("ftp://ftp.provissy.boo.jp/");
                FTPControl fc = new FTPControl(u, "boo.jp-provissy", "mn3xP2w6");
                if (!fc.FileExist(tb_TitleOfContent.Text))
                {
                    fc.GotoDirectory("PrvToolsCommCenter");
                    FlowDocument document = rtb_ContentToSend.Document;
                    System.IO.Stream s = new System.IO.MemoryStream();
                    System.Windows.Markup.XamlWriter.Save(document, s);
                    byte[] data = new byte[s.Length];
                    s.Position = 0;
                    s.Read(data, 0, data.Length);
                    s.Close();
                    fc.UploadFile(data, tb_TitleOfContent.Text);
                }
                else
                {
                    MessageBox.Show("已有同名文件了哦！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CallMethodButton_Click_10(object sender, RoutedEventArgs e)
        {
            Process.Start("http://tieba.baidu.com/p/3381387613");
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btn_BBS_SendComment_Click(object sender, RoutedEventArgs e)
        {
            lb_BBS_ShowState.Content = "发送中";
            Thread t = new Thread(ForSendComment_Click);
            t.Start();

        }

        private void ForSendComment_Click()
        {
            try
            {

                Action a = new Action(() =>
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
                    lb_BBS_ShowState.Content = "发送完毕";
                });
                this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);

                //string tempText = System.Text.Encoding.UTF8.GetString(downloadedText);
                //string tempText2 = tempText += "\n" + DateTime.Now.ToString() + "---" + tb_BBS_Username.Text + "\n" + tb_CommentToSend.Text;
                //byte[] bytes = Encoding.UTF8.GetBytes(tempText2);
                //fc.UploadFile(bytes, "BBS_Test_1", true);
            }
            catch (Exception ex)
            {
                Action a = new Action(() =>
                {
                    lb_BBS_ShowState.Content = "发送失败";
                    ErrorHandler(ex.ToString());
                });
                this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
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
            lb_BBS_ShowState.Content = "刷新中";
            Thread t = new Thread(forGetComments_Click);
            t.Start();
        }

        private void forGetComments_Click()
        {
            try
            {
                //byte[] downloadedText = fc.DownloadFile("BBS_Test_1");
                //tb_BBS_Comments.Text = System.Text.Encoding.UTF8.GetString(downloadedText);

                Action a = new Action(() =>
                {
                    byte[] downloadedComments = FTP_FOR_BBS.DownloadFile("BBS_Test_2");
                    System.IO.Stream ss = new System.IO.MemoryStream(downloadedComments);
                    FlowDocument doc = System.Windows.Markup.XamlReader.Load(ss) as FlowDocument;
                    ss.Close();
                    rtb_BBS_Comment.Document = doc;
                    lb_BBS_ShowState.Content = "刷新完毕";
                });
                this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
            }
            catch (Exception ex)
            {
                Action a = new Action(() =>
                {
                    lb_BBS_ShowState.Content = "刷新失败";
                    ErrorHandler(ex.ToString());
                });
                this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);

            }
        }


        private void isAuto_forGetComments_Click()
        {
            try
            {
                //byte[] downloadedText = fc.DownloadFile("BBS_Test_1");
                //tb_BBS_Comments.Text = System.Text.Encoding.UTF8.GetString(downloadedText);
                byte[] downloadedComments = FTP_FOR_BBS.DownloadFile("BBS_Test_2");
                System.IO.Stream ss = new System.IO.MemoryStream(downloadedComments);
                FlowDocument doc = System.Windows.Markup.XamlReader.Load(ss) as FlowDocument;
                ss.Close();
                Action a = new Action(() =>
                {
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
                    ErrorHandler(ex.ToString());
                });
                this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);

            }
        }



        FTPControl FTP_FOR_BBS = new FTPControl(new Uri("ftp://ftp.provissy1.boo.jp/"), "boo.jp-provissy1", "2cA3rb5f");

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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int fontSize = Convert.ToInt32(rtb_BBS_CommentToSend.Selection.GetPropertyValue(TextElement.FontSizeProperty));
            fontSize++;
            rtb_BBS_CommentToSend.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize.ToString());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_BBS_SetColorToRed_Click(object sender, RoutedEventArgs e)
        {
            rtb_BBS_CommentToSend.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        }



        private void btn_BBS_AutoRefreshComments_Click(object sender, RoutedEventArgs e)
        {

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

            //FTP_FOR_BBS.DownloadDataCompleted += new FTPControl.De_DownloadDataCompleted(refreshData);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FlowDocument document = rtb_BBS_CommentToSend.Document;
            System.IO.Stream s = new System.IO.MemoryStream();
            System.Windows.Markup.XamlWriter.Save(document, s);
            byte[] data = new byte[s.Length];
            s.Position = 0;
            s.Read(data, 0, data.Length);
            s.Close();
            FTP_FOR_BBS.UploadFile(data, "BBS_Test_2", true);
        }

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

        private void btn_Backup_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (tb_Backup_Username.Text == "" || psd_Backup_Password.Password == "")
            {
                MessageBox.Show("用户名或密码不能为空！");
            }
            else
            {
                try
                {
                    if (!File.Exists(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg"))
                    {
                        var w = File.AppendText(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                        w.WriteLine(tb_Backup_Username.Text);
                        w.WriteLine(psd_Backup_Password.Password);
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

                    Uri ur = new Uri("ftp://ftp.provissy1.boo.jp/");
                    FTPControl fc = new FTPControl(ur, "boo.jp-provissy1", "2cA3rb5f");
                    fc.GotoDirectory("User_Statistics_Data");
                    if (fc.FileExist(tb_Backup_Username.Text))
                    {
                        MessageBox.Show("用户名已被注册！");
                    }
                    else
                    {
                        StreamReader sr = new StreamReader(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                        string username = sr.ReadLine();
                        sr.Close();
                        fc.UploadFile(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg", username);
                        MessageBox.Show("用户创建成功");
                        tbl_Backup_Password.Visibility = Visibility.Hidden;
                        tbl_Backup_Username.Visibility = Visibility.Hidden;
                        tb_Backup_Username.Visibility = Visibility.Hidden;
                        psd_Backup_Password.Visibility = Visibility.Hidden;
                        btn_Backup_CreateAccount.Visibility = Visibility.Hidden;
                        btn_Backup_CloudToLocal.IsEnabled = true;
                        btn_Backup_LocalToCloud.IsEnabled = true;
                        lb_Backup_Username.Content = tb_Backup_Username.Text;
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler(ex.ToString());
                }
            }
        }

        private void btn_Backup_CloudToLocal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri u = new Uri("ftp://ftp.provissy1.boo.jp/");
                FTPControl fc = new FTPControl(u, "boo.jp-provissy1", "2cA3rb5f");
                fc.GotoDirectory("User_Statistics_Data");
                string l = UniversalConstants.CurrentDirectory;
                if (File.Exists(l + "ItemBuildLog.csv"))
                {
                    File.Delete(l + "ItemBuildLog.csv");
                    fc.DownloadFile("ItemBuildLog" + " - " + lb_Backup_Username.Content + ".csv", UniversalConstants.CurrentDirectory, "ItemBuildLog.csv");

                }
                if (File.Exists(l + "ShipBuildLog.csv"))
                {
                    File.Delete(l + "ShipBuildLog.csv");
                    fc.DownloadFile("ShipBuildLog" + " - " + lb_Backup_Username.Content + ".csv", UniversalConstants.CurrentDirectory, "ShipBuildLog.csv");

                }
                if (File.Exists(l + "DropLog.csv"))
                {
                    File.Delete(l + "DropLog.csv");
                    fc.DownloadFile("DropLog" + " - " + lb_Backup_Username.Content + ".csv", UniversalConstants.CurrentDirectory, "DropLog.csv");

                }
                if (File.Exists(l + "MaterialsLog.csv"))
                {
                    File.Delete(l + "MaterialsLog.csv");
                    fc.DownloadFile("MaterialsLog" + " - " + lb_Backup_Username.Content + ".csv", UniversalConstants.CurrentDirectory, "MaterialsLog.csv");
                }
                MessageBox.Show("同步成功！");
            }
            catch (Exception ex)
            {
                ErrorHandler(ex.ToString());
            }
        }

        private void btn_Backup_LocalToCloud_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri u = new Uri("ftp://ftp.provissy1.boo.jp/");
                FTPControl fc = new FTPControl(u, "boo.jp-provissy1", "2cA3rb5f");
                fc.GotoDirectory("User_Statistics_Data");
                string l = UniversalConstants.CurrentDirectory;
                if (File.Exists(l + "ItemBuildLog.csv"))
                {
                    fc.UploadFile(UniversalConstants.CurrentDirectory + "ItemBuildLog.csv", "ItemBuildLog" + " - " + lb_Backup_Username.Content + ".csv", true);

                }
                if (File.Exists(l + "ShipBuildLog.csv"))
                {
                    fc.UploadFile(UniversalConstants.CurrentDirectory + "ShipBuildLog.csv", "ShipBuildLog" + " - " + lb_Backup_Username.Content + ".csv", true);

                }
                if (File.Exists(l + "DropLog.csv"))
                {
                    fc.UploadFile(UniversalConstants.CurrentDirectory + "DropLog.csv", "DropLog" + " - " + lb_Backup_Username.Content + ".csv", true);

                }
                if (File.Exists(l + "MaterialsLog.csv"))
                {
                    fc.UploadFile(UniversalConstants.CurrentDirectory + "MaterialsLog.csv", "MaterialsLog" + " - " + lb_Backup_Username.Content + ".csv", true);

                }
                MessageBox.Show("备份成功！");

            }
            catch (Exception ex)
            {
                ErrorHandler(ex.ToString());
            }
        }

        private void CallMethodButton_Click_11(object sender, RoutedEventArgs e)
        {
            StatisticsBackup.Visibility = Visibility.Hidden;
        }

        private void CallMethodButton_Click_12(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri u = new Uri("ftp://ftp.provissy1.boo.jp/");
                FTPControl fc = new FTPControl(u, "boo.jp-provissy1", "2cA3rb5f");
                fc.GotoDirectory("User_Statistics_Data");
                StreamReader r = new StreamReader(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
                string l = r.ReadLine();
                r.Close();
                if (!fc.FileExist(l))
                {
                    MessageBox.Show("未注册！");
                    tbl_Backup_Password.Visibility = Visibility.Visible;
                    tbl_Backup_Username.Visibility = Visibility.Visible;
                    tb_Backup_Username.Visibility = Visibility.Visible;
                    psd_Backup_Password.Visibility = Visibility.Visible;
                    btn_Backup_CreateAccount.Visibility = Visibility.Visible;
                    deleteCfg();
                }
                else
                {
                    MessageBox.Show("已注册过！");
                }
            }
            catch
            {
                MessageBox.Show("未注册！");
                tbl_Backup_Password.Visibility = Visibility.Visible;
                tbl_Backup_Username.Visibility = Visibility.Visible;
                tb_Backup_Username.Visibility = Visibility.Visible;
                psd_Backup_Password.Visibility = Visibility.Visible;
                btn_Backup_CreateAccount.Visibility = Visibility.Visible;
                deleteCfg();
            }
        }

        private void deleteCfg()
        {
            try
            {
                File.Delete(UniversalConstants.CurrentDirectory + "PrvToolsUsrCfg");
            }
            catch { }
        }

        private void btn_AkiEvent_MapViewer_Click(object sender, RoutedEventArgs e)
        {
            closeAllTabs();
            closeFuncTab();
            EventMapViewer.Visibility = Visibility.Visible;
        }
    }
}


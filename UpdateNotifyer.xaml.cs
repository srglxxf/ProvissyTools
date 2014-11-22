using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;    
using System.Windows.Threading;


namespace ProvissyTools
{
    /// <summary>
    /// Interaction logic for UpdateNotifyer.xaml
    /// </summary>
    public partial class UpdateNotifyer : Window
    {
        private string keyWord = "#14112201#";
        private System.Timers.Timer timer = new System.Timers.Timer(500000);
        private short chk;
        //public UpdateNotifyer()
        //{
        //    InitializeComponent();
        //    timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        //    timer.Start();
        //    Thread t = new Thread(() => CHK_Update("check.txt"));
        //    t.Start();
        //}

        public UpdateNotifyer()
        {
                InitializeComponent();
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Start();
                Thread t = new Thread(() => CHK_Update( UniversalConstants.CurrentDirectory + "check.txt"));
                t.Start();
                this.ShowInTaskbar = false;
        }


        private void timer_Elapsed(object sender, EventArgs e)
        {
            Dispatcher.Invoke
            (
                DispatcherPriority.Normal, (Action)delegate()
                {
                    Thread t = new Thread(() => CHK_Update(UniversalConstants.CurrentDirectory + "check.txt"));
                    t.Start();
                }
            );
        }
        private void mouseClicked1(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void mouseClicked2(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        public void CHK_Update(string strFileName)
        {
            try
            {
                string str;
                string allFile;
                string fileContent;
                bool flag = false;
                long SPosition = 0;
                FileStream FStream;
                if (File.Exists(strFileName))
                {
                    try { this.deletefile(); }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    FStream = new FileStream(strFileName, FileMode.Create);
                    SPosition = 0;
                }
                else
                {
                    FStream = new FileStream(strFileName, FileMode.Create);
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
                catch (Exception)
                {
                    FStream.Close();
                    flag = false;       //返回false下载失败
                }
                if (flag)
                {
                    str = UniversalConstants.CurrentDirectory + "check.txt";
                    System.IO.FileStream myStreama = new FileStream(str, FileMode.Open);       //Read File.
                    System.IO.StreamReader myStreamReader = new StreamReader(myStreama);
                    fileContent = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    allFile = fileContent;
                    Regex reg = new Regex(keyWord);     //keyword.
                    Match mat = reg.Match(allFile);
                    if (mat.Success)
                    {
                        //No Update.
                    }
                    else
                    {
                        //Success.
                        Action a = new Action(() => { (this.Resources["NotifyAnimation"] as Storyboard).Begin(); });
                        this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
                    }
                    this.deletefile();
                }
                else
                {
                    Action a = new Action(() => { chk = 3; });
                    this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
                }
            }
            catch
            {
                MessageBox.Show("自动检查更新失败。");
            }


        }
        private void deletefile()
        {
            File.Delete(UniversalConstants.CurrentDirectory + "check.txt");
        }

        private void closeNotiryer1(object sender, MouseButtonEventArgs e)
        {
            (this.Resources["NotifyerClose"] as Storyboard).Begin();

        }

        private void closeNotifyer2(object sender, MouseButtonEventArgs e)
        {
            (this.Resources["NotifyerClose"] as Storyboard).Begin();
        }

        private void closeNotifyer3(object sender, MouseButtonEventArgs e)
        {
            (this.Resources["NotifyerClose"] as Storyboard).Begin();
            timer.Stop();

        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

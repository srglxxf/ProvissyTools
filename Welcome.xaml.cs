using System.Windows;
using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Management;
using Grabacr07.KanColleViewer.Composition;
using System.Security.Cryptography;
using System.Text;

namespace ProvissyTools
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private async void CallMethodButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (retryCount <= 1)
                {
                    Pgb_Progress.Value = 0;
                    //Tbl_Introdution.Text = "正在重试...";
                    Btn_Retry.Visibility = Visibility.Hidden;
                    MainContent.Visibility = Visibility.Hidden;
                    LoadingGrid.Visibility = Visibility.Visible;
                    if (!Directory.Exists("ProvissyTools"))
                    {
                        Directory.CreateDirectory("ProvissyTools");
                    }
                    Pgb_Progress.Value += await downloadSoundDLL();
                    Pgb_Progress.Value += await downloadNekoCompareImage();
                    Pgb_Progress.Value += await downloadChartDll1();
                    Pgb_Progress.Value += await downloadChartDll2();
                    Pgb_Progress.Value += await uploadUsage();
                    if (Directory.Exists("Sounds")) { Directory.Delete("Sounds", true); }
                    Directory.CreateDirectory("Sounds");
                    DirectoryInfo d = new DirectoryInfo("Sounds");
                    d.CreateSubdirectory(NotifyType.Build.ToString());
                    d.CreateSubdirectory(NotifyType.Expedition.ToString());
                    d.CreateSubdirectory(NotifyType.Rejuvenated.ToString());
                    d.CreateSubdirectory(NotifyType.Repair.ToString());
                }
                else
                {
                    MessageBoxResult m = MessageBox.Show("一直出现同一错误？点击确定前往百度网盘下载必要文件，并且解压至KCV目录，即可正常使用ProvissyTools。\n如果确认要这么做，点击确定后点击完成设置", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if(m == MessageBoxResult.Yes)
                    {
                        Process.Start("http://pan.baidu.com/s/1AkGkY;");
                        Btn_Finish.Visibility = Visibility.Visible;
                        retryCount = 0;
                    }
                }
            }
            catch(Exception ex)
            {
                Tbl_Introdution.Text = "错误！请重试";
                Btn_Retry.Visibility = Visibility.Visible;
                MessageBox.Show(ex.ToString());
            }
        }

        private async Task<double> downloadSoundDLL()
        {
            return await Task.Run(() =>
             {
                 WebClient w = new WebClient();
                 w.DownloadFile("http://provissy.com/NAudio.dll", UniversalConstants.CurrentDirectory + @"\NAudio.dll");
                 FileStream file = new FileStream(UniversalConstants.CurrentDirectory + @"\NAudio.dll", System.IO.FileMode.Open);
                 MD5 md5 = new MD5CryptoServiceProvider();
                 byte[] retVal = md5.ComputeHash(file);
                 file.Close();
                 StringBuilder sb = new StringBuilder();
                 for (int i = 0; i < retVal.Length; i++)
                 {
                     sb.Append(retVal[i].ToString("x2"));
                 }
                 if (string.Equals(sb.ToString(), "F6FFB14117E3E209E4BDDEDBB8E952B9"))
                 {
                     return 20;
                 }
                 else
                 {
                     throw new Exception("MD5不匹配！");
                 }
                 
             });
        }

        private async Task<double> downloadChartDll1()
        {
            return await Task.Run(() =>
            {
                WebClient w = new WebClient();
                w.DownloadFile("http://provissy.com/WPFToolkit.dll", UniversalConstants.CurrentDirectory + @"\WPFToolkit.dll");
                FileStream file = new FileStream(UniversalConstants.CurrentDirectory + @"\WPFToolkit.dll", System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                if (string.Equals(sb.ToString(), "195ED09E0B4F3B09EA4A3B67A0D3F396"))
                {
                    return 20;
                }
                else
                {
                    throw new Exception("MD5不匹配！");
                }
            });
        }

        private async Task<double> downloadChartDll2()
        {
            return await Task.Run(() =>
            {
                WebClient w = new WebClient();
                w.DownloadFile("http://provissy.com/System.Windows.Controls.DataVisualization.Toolkit.dll", UniversalConstants.CurrentDirectory + @"\System.Windows.Controls.DataVisualization.Toolkit.dll");
                FileStream file = new FileStream(UniversalConstants.CurrentDirectory + @"\System.Windows.Controls.DataVisualization.Toolkit.dll", System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                if (string.Equals(sb.ToString(), "6813EBECD58E557E1D65C08E2B1030AF"))
                {
                    return 20;
                }
                else
                {
                    throw new Exception("MD5不匹配！");
                }
            });
        }

        private async Task<double> downloadNekoCompareImage()
        {
            return await Task.Run(() =>
            {
                WebClient w = new WebClient();
                w.DownloadFile("http://provissy.com/nekoError.png", UniversalConstants.CurrentDirectory + @"\ProvissyTools\nekoError.png");
                FileStream file = new FileStream(UniversalConstants.CurrentDirectory + @"\ProvissyTools\nekoError.png", System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                if (string.Equals(sb.ToString(), "3959048BC55B1C50F3A2106CF6BBA16F"))
                {
                    return 20;
                }
                else
                {
                    throw new Exception("MD5不匹配！");
                }
            });
        }

        private async Task<double> uploadUsage()
        {
            return await Task.Run(() =>
            {
                Microsoft.VisualBasic.Devices.Computer c = new Microsoft.VisualBasic.Devices.Computer();
                Random r = new Random();
                int identity = r.Next();
                WebClient w = new WebClient();
                StreamWriter s = new StreamWriter(UniversalConstants.CurrentDirectory + c.Name + c.Info.OSFullName + identity);
                s.WriteLine(Guid.NewGuid().ToString());
                s.WriteLine(DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString());
                s.WriteLine(GetSystemType());
                s.WriteLine(GetTotalPhysicalMemory());
                s.Close();
                w.UploadFile("http://provissy.com/UploadToUsageFolder.php", "POST", UniversalConstants.CurrentDirectory + c.Name + c.Info.OSFullName + identity);
                File.Delete(UniversalConstants.CurrentDirectory + c.Name + c.Info.OSFullName + identity);
                return 20;
            });
        }

        private void PgbValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(Pgb_Progress.Value == 100)
            {
                Btn_Finish.Visibility = Visibility.Visible;
            }
        }

        string GetTotalPhysicalMemory()
        {
            try
            {

                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["TotalPhysicalMemory"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        string GetSystemType()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["SystemType"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        private void TextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject("linxunpei@hotmail.com");
            MessageBox.Show("支付宝地址已复制！");
        }

        int retryCount = 0;

        private void Btn_Retry_Click(object sender, RoutedEventArgs e)
        {
            CallMethodButton_Click(null, null);
            Tbl_Introdution.Text = "正在重试...";
            retryCount++;
        }
    }
}

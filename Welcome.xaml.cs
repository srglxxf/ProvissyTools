using System.Windows;
using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Management;

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

        //private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    ProvissyToolsSettings.Load();
        //}

        private async void CallMethodButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                MainContent.Visibility = Visibility.Hidden;
                LoadingGrid.Visibility = Visibility.Visible;
                if (!Directory.Exists("ProvissyTools"))
                {
                    Directory.CreateDirectory("ProvissyTools");
                }
                Pgb_Progress.Value += await downloadSoundDLL();
                Pgb_Progress.Value += await SubActivateNekoDetector();
                Pgb_Progress.Value += await downloadChartDll1();
                Pgb_Progress.Value += await downloadChartDll2();
                Pgb_Progress.Value += await uploadUsage();

                ProvissyToolsSettings.Current.FirstTimeSet();
                MessageBox.Show("Success!");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async Task<double> downloadSoundDLL()
        {
           return await Task.Run(() =>
            {
                if (!File.Exists(UniversalConstants.CurrentDirectory + @"\NAudio.dll"))
                {
                    WebClient w = new WebClient();
                    w.DownloadFile("http://provissy.com/NAudio.dll", UniversalConstants.CurrentDirectory + @"\NAudio.dll");
                    return 20;
                }
                else
                {
                    return 20;
                }
            });
        }

        private async Task<double> downloadChartDll1()
        {
            return await Task.Run(() =>
            {
                WebClient w = new WebClient();
                if (File.Exists(UniversalConstants.CurrentDirectory + "WPFToolkit.dll"))
                {
                    File.Delete(UniversalConstants.CurrentDirectory + "WPFToolkit.dll");
                    w.DownloadFile("http://provissy.com/WPFToolkit.dll", UniversalConstants.CurrentDirectory + @"\WPFToolkit.dll");
                }

                else
                {
                    w.DownloadFile("http://provissy.com/WPFToolkit.dll", UniversalConstants.CurrentDirectory + @"\WPFToolkit.dll");
                } 
                return 20;
            });
        }

        private async Task<double> downloadChartDll2()
        {
            return await Task.Run(() =>
            {
                WebClient w = new WebClient();
                if (File.Exists(UniversalConstants.CurrentDirectory + "System.Windows.Controls.DataVisualization.Toolkit.dll"))
                {
                    File.Delete(UniversalConstants.CurrentDirectory + "System.Windows.Controls.DataVisualization.Toolkit.dll");
                    w.DownloadFile("http://provissy.com/System.Windows.Controls.DataVisualization.Toolkit.dll", UniversalConstants.CurrentDirectory + @"\System.Windows.Controls.DataVisualization.Toolkit.dll");
                }

                else
                {
                    w.DownloadFile("http://provissy.com/System.Windows.Controls.DataVisualization.Toolkit.dll", UniversalConstants.CurrentDirectory + @"\System.Windows.Controls.DataVisualization.Toolkit.dll");
                }
                return 20;
            });
        }

        private async Task<double> SubActivateNekoDetector()
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(UniversalConstants.CurrentDirectory + @"\ProvissyTools\nekoError.png"))
                {
                    WebClient w = new WebClient();
                    string pathURL = "http://provissy.com/nekoError.png";
                    string pathLocal = UniversalConstants.CurrentDirectory + @"\ProvissyTools\nekoError.png";
                    w.DownloadFile(pathURL, pathLocal);
                    return 20;
                }
                else
                {
                    return 20;
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
                s.WriteLine(GetIPAddress());
                s.WriteLine(GetSystemType());
                s.WriteLine(GetTotalPhysicalMemory());
                s.Close();
                w.UploadFile("http://provissy.com/UploadToUsageFolder.php", "POST", UniversalConstants.CurrentDirectory + c.Name + c.Info.OSFullName + identity);
                File.Delete(UniversalConstants.CurrentDirectory + c.Name + c.Info.OSFullName + identity);
                return 20;
            });
        }

        private void Btn_Finishi_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter s = new StreamWriter(ProvissyToolsSettings.usageRecordPath);
            s.WriteLine("3.0Preview");
            s.Close();
            System.Diagnostics.Process[] killprocess = System.Diagnostics.Process.GetProcessesByName("KanColleViewer");
            foreach (System.Diagnostics.Process p in killprocess)
            {
                p.Kill();
            }
        }

        private void PgbValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(Pgb_Progress.Value == 100)
            {
                Btn_Finishi.Visibility = Visibility.Visible;
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

        string GetIPAddress()
        {
            try
            {
                //获取IP地址
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        //st=mo["IpAddress"].ToString();
                        System.Array ar;
                        ar = (System.Array)(mo.Properties["IpAddress"].Value);
                        st = ar.GetValue(0).ToString();
                        break;
                    }
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
    }
}

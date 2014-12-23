using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ProvissyTools
{
    class CommonHelper
    {
        public static void StartupChecker()
        {
            try
            {
                WebClient w = new WebClient();
                Regex reg = new Regex(UniversalConstants.CurrentVersion);
                Match mat = reg.Match(w.DownloadString("http://www.cnblogs.com/provissy/p/4056570.html"));
                if (mat.Success)
                {
                }
                else
                {
                    if (File.Exists(UniversalConstants.CurrentDirectory + "UpdaterForPrvTools.exe"))
                    {
                        File.Delete(UniversalConstants.CurrentDirectory + "UpdaterForPrvTools.exe");
                    }
                    w.DownloadFile("http://provissy.com/UpdaterForPrvTools.exe", UniversalConstants.CurrentDirectory + "UpdaterForPrvTools.exe");
                    Process.Start(UniversalConstants.CurrentDirectory + "UpdaterForPrvTools.exe");
                }
            }
            catch
            {
                MessageBox.Show("自动更新检查失败");
            }

        }
    }
}

using Grabacr07.KanColleViewer.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for AnimationNotifier.xaml
    /// </summary>
    public partial class AnimationNotifier : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        public static AnimationNotifier Current { get; set; }
        public AnimationNotifier()
        {
            InitializeComponent();
            this.Topmost = true;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromSeconds(5);
            this.ShowInTaskbar = false;
            Current = this;
        }

        public void AnimationStart(NotifyType type, string header)
        {
            try
            {
                Tbl_NotifyContent.Text = header;
                (this.Resources["StartAnimation"] as Storyboard).Begin();
                timer.Start();
            }
            catch { }
        }


        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            (this.Resources["EndAnimation"] as Storyboard).Begin();
        }

        private void Close_Tbl(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            (this.Resources["EndAnimation"] as Storyboard).Begin();
        }

        private void Close_Recentage(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            (this.Resources["EndAnimation"] as Storyboard).Begin();
        }

    }
}

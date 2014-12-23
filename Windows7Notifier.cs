using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grabacr07.KanColleViewer.Composition;
using Application = System.Windows.Application;
using Grabacr07.KanColleViewer;
using System.Diagnostics;

namespace ProvissyTools
{
    internal class Windows7Notifier : INotifier
    {
        CustomSound sound = new CustomSound();
        private NotifyIcon notifyIcon;
        private EventHandler activatedAction;
        private AnimationNotifier animeNotifier;

        public void Initialize()
        {
            const string iconUri = "pack://application:,,,/KanColleViewer;Component/Assets/app.ico";

            Uri uri;
            if (!Uri.TryCreate(iconUri, UriKind.Absolute, out uri))
                return;

            var streamResourceInfo = Application.GetResourceStream(uri);
            if (streamResourceInfo == null)
                return;

            using (var stream = streamResourceInfo.Stream)
            {
                this.notifyIcon = new NotifyIcon
                {
                    Text = App.ProductInfo.Title,
                    Icon = new Icon(stream),
                    Visible = true,
                };
                ContextMenu menu = new ContextMenu();

                MenuItem closeItem = new MenuItem();
                closeItem.Text = "退出 KanColleViewer（强制）";
                closeItem.Click += new EventHandler(delegate
                    {
                        System.Diagnostics.Process[] killprocess = System.Diagnostics.Process.GetProcessesByName("KanColleViewer");
                        foreach (System.Diagnostics.Process p in killprocess)
                        {
                            p.Kill();
                        }
                    });

                MenuItem addItem = new MenuItem();
                addItem.Text = "关闭计算机";
                addItem.Click += new EventHandler(delegate { Process.Start("shutdown.exe", "-s -t 00"); });

                menu.MenuItems.Add(addItem);
                menu.MenuItems.Add(closeItem);

                notifyIcon.ContextMenu = menu;
            }
            animeNotifier = new AnimationNotifier();
            animeNotifier.Show();

        }

        public void Show(NotifyType type, string header, string body, Action activated, Action<Exception> failed = null)
        {
            if (this.notifyIcon == null)
                return;

            if (activated != null)
            {
                this.notifyIcon.BalloonTipClicked -= this.activatedAction;

                this.activatedAction = (sender, args) => activated();
                this.notifyIcon.BalloonTipClicked += this.activatedAction;
            }
            notifyIcon.ShowBalloonTip(2000, header, body, ToolTipIcon.Info);
            sound.SoundOutput(type, header, false);
            animeNotifier.AnimationStart(type, body);
        }

        public object GetSettingsView()
        {
            return null;
        }

        public void Dispose()
        {
            if (this.notifyIcon != null)
            {
                this.notifyIcon.Dispose();
            }
            if(this.animeNotifier != null)
            {
                this.animeNotifier.Close();
            }
        }
    }
}

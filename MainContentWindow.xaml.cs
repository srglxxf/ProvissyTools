using Grabacr07.KanColleViewer.Views;
using System.ComponentModel;

namespace ProvissyTools
{
    /// <summary>
    /// Interaction logic for MainContentWindow.xaml
    /// </summary>
    public partial class MainContentWindow
    {
        public static MainContentWindow Current { get; private set; }

        public MainContentWindow()
        {
            InitializeComponent();

            Current = this;
            MainWindow.Current.Closed += (sender, args) => this.Close();

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Current = null;
            
            ProvissyToolsSettings.Current.WindowWidth = this.ActualWidth;
            ProvissyToolsSettings.Current.WindowHeight = this.ActualHeight;

            base.OnClosing(e);
        }

        private void GlowMetroWindow_Closing(object sender, CancelEventArgs e)
        {
            LandscapeViewModel.Instance.CurrentLayout = KCVContentLayout.Portrait;
        }
    }
}

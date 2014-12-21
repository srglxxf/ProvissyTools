using Grabacr07.KanColleViewer.ViewModels;
using Livet;

namespace ProvissyTools
{
    class StatisticWindowViewModel : WindowViewModel
    {

        #region CurrentEnableNullDropLogging 変更通知プロパティ

        public bool CurrentEnableNullDropLogging
        {
            get { return ProvissyToolsSettings.Current.EnableNullDropLogging; }
            set
            {
                if (ProvissyToolsSettings.Current.EnableNullDropLogging != value)
                {
                    ProvissyToolsSettings.Current.EnableNullDropLogging = value;
                    ProvissyToolsSettings.Current.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

    }
}

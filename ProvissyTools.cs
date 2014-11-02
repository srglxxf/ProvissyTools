using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;

namespace ProvissyTools
{
	[Export(typeof(IToolPlugin))]
    [ExportMetadata("Title", "ProvissyTools")]
    [ExportMetadata("Description", "Provissy KCV Tools")]
	[ExportMetadata("Version", "1.0")]
	[ExportMetadata("Author", "@Provissy")]
	public class ProvissyTools : IToolPlugin
	{
		private readonly MainViewViewModel viewmodel = new MainViewViewModel
		{
		};

		public string ToolName
		{
            get { return "ProvissyTools"; }
		}

		public object GetSettingsView()
		{
			return null;
		}

		public object GetToolView()
		{
			return new MainView { DataContext = this.viewmodel, };
		}
	}
}

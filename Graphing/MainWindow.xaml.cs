using System;
using System.Windows;

namespace Graphing
{
	public partial class MainWindow
	{
		private GraphingViewModel ViewModel { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			this.ViewModel = new GraphingViewModel(new GraphingModel(this.GraphPlot));
			this.DataContext = this.ViewModel;
			DrawGraph();
		}

		private void GraphPlot_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			DrawGraph();
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			DrawGraph();
		}

		private void DrawGraph()
		{
			try
			{
				this.ViewModel.DrawGraph(true);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Error Plotting Graph", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.CodeDom;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Windows.Input;

namespace Graphing
{
	public partial class MainWindow : Window
	{
		private GraphingViewModel ViewModel { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			this.ViewModel = new GraphingViewModel(new GraphingModel(this.GraphPlot));
			this.DataContext = this.ViewModel;
			DrawGraph(true);
		}

		private void GraphPlot_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			DrawGraph(false);
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			DrawGraph(true);
		}

		private void DrawGraph(bool functionsChanged)
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

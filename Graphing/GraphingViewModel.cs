using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Mathematics.Graphing;

namespace Graphing
{
	public class GraphingViewModel : DependencyObject
	{
		public string FofX { get; set; }
		public string FofXDerivative { get; set; }
		public string GofX { get; set; }
		public string GofXDerivative { get; set; }
		public string HofX { get; set; }
		public string HofXDerivative { get; set; }

		public static readonly DependencyProperty UpperBoundProperty =
			DependencyProperty.Register("UpperBound", typeof(double), typeof(GraphingViewModel), new UIPropertyMetadata());
		public double UpperBound
		{
			get { return (double) GetValue(UpperBoundProperty); }
			set { SetValue(UpperBoundProperty, value); }
		}

		public static readonly DependencyProperty LowerBoundProperty =
			DependencyProperty.Register("LowerBound", typeof(double), typeof(GraphingViewModel), new UIPropertyMetadata());
		public double LowerBound
		{
			get { return (double) GetValue(LowerBoundProperty); }
			set { SetValue(LowerBoundProperty, value); }
		}

		public GraphingModel GraphModel { get; set; }

		public GraphingViewModel(GraphingModel graphModel)
		{
			this.GraphModel = graphModel;
			this.LowerBound = 0;
			this.UpperBound = 10;
		}

		internal void DrawGraph(bool functionsChanged)
		{
			if (this.LowerBound == this.UpperBound)
				this.UpperBound++;
			else if (this.LowerBound > this.UpperBound)
			{
				var temp = this.UpperBound;
				this.UpperBound = this.LowerBound;
				this.LowerBound = temp;
			}

			if (functionsChanged)
				this.GraphModel.DrawGraph(new[] { this.FofX, this.GofX, this.HofX },
					new[] { this.FofXDerivative, this.GofXDerivative, this.HofXDerivative }, this.LowerBound, this.UpperBound);
			else
				this.GraphModel.DrawGraph(this.LowerBound, this.UpperBound);
		}
	}
}

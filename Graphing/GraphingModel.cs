using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Mathematics;
using Mathematics.Graphing;
using Microsoft.CSharp;

namespace Graphing
{
	public class FormulaPlot
	{
		public Func<double, double> Function { get; set; }
		public Func<double, double> Derivative { get; set; }
		public SolidColorBrush Colour { get; set; }
		public double LowerBound { get; set; }
		public double UpperBound { get; set; }
	}

	public class GraphingModel
	{
		public const double Limit = 1E-10;

		public GraphDisplay GraphPlot { get; set; }
		public IList<FormulaPlot> FormulaList;

		private static CompilerParameters CompilerParams { get; set; }
		static GraphingModel()
		{
			GraphingModel.CompilerParams = new CompilerParameters();
			GraphingModel.CompilerParams.ReferencedAssemblies.Add("system.dll");
			GraphingModel.CompilerParams.GenerateExecutable = false;
			GraphingModel.CompilerParams.GenerateInMemory = true;
		}

		public GraphingModel(GraphDisplay graphPlot)
		{
			this.GraphPlot = graphPlot;
		}

		public static string ValidateFunction(string function)
		{
			var comp = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });

			var codeString = new StringBuilder();
			codeString.Append      ("using System; \n");
			codeString.Append      ("namespace Internal {\n");
			codeString.Append      ("    public class Dynamic {\n");
			codeString.Append      ("        public Func<double, double> GetFunc() {\n");
			codeString.AppendFormat("            return x => {0};\n", function);
			codeString.Append      ("        }\n");
			codeString.Append      ("    }\n");
			codeString.Append      ("}\n");

			var compilerResults = comp.CompileAssemblyFromSource(GraphingModel.CompilerParams, codeString.ToString());
			if (compilerResults.Errors.HasErrors)
			{
				var errorString = new StringBuilder();
				var errors = compilerResults.Errors.OfType<CompilerError>().ToArray();
				for (var i = 0; i < errors.Length; ++i)
				{
					errorString.Append(i == errors.Length - 1 ? errors[i].ErrorText : errors[i].ErrorText + "\n");
				}

				return errorString.ToString();
			}

			return string.Empty;
		}

		private static IEnumerable<Func<double, double>> CreateFunctions(IList<string> functionStrings)
		{
			var comp = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });

			var code = new StringBuilder();
			code.Append("using System; \n");
			code.Append("namespace Internal {\n");
			code.Append("    public class Dynamic {\n");
			var i = 0;
			foreach (var function in functionStrings)
			{
				if (string.IsNullOrEmpty(function))
				{
					++i;
					continue;
				}

				code.Append      ("        public Func<double, double> GetFunc" + i + "() {\n");
				code.AppendFormat("            return x => {0};\n", function);
				code.Append      ("        }\n");
				i++;
			}
			code.Append("    }\n");
			code.Append("}\n");

			var compilerResults = comp.CompileAssemblyFromSource(GraphingModel.CompilerParams, code.ToString());
			var assembly = compilerResults.CompiledAssembly;
			var dynamicInstance = assembly.CreateInstance("Internal.Dynamic");

			var functionList = new Func<double, double>[i];
			i = 0;
			foreach (var function in functionStrings)
			{
				if (string.IsNullOrEmpty(function))
				{
					++i;
					continue;
				}

				var mi = dynamicInstance.GetType().GetMethod(string.Format("GetFunc{0}", i));
				functionList[i++] = (Func<double, double>) mi.Invoke(dynamicInstance, null);
			}

			return functionList;
		}

		private void DrawGridAndAxes(double lowerBound, double upperBound)
		{
			double yTop, yBottom, xLeft, xRight;

			FindRange(lowerBound, upperBound, out yTop, out yBottom, out xLeft, out xRight);

			var xInterval = (xRight - xLeft) / 10d;
			var yInterval = (yTop - yBottom) / 10d;

			this.GraphPlot.Clear();
			this.GraphPlot.YTop = yTop + yInterval;
			this.GraphPlot.YBottom = yBottom - yInterval;
			this.GraphPlot.XLeft = xLeft - xInterval;
			this.GraphPlot.XRight = xRight + xInterval;

			this.GraphPlot.AddGrid(xInterval, yInterval,
				new Mathematics.Graphing.GraphStyle { Stroke = new SolidColorBrush(Colors.LightGray), Thickness = 1 });

			var axisStyle = new Mathematics.Graphing.GraphStyle { Stroke = new SolidColorBrush(Colors.Black), Thickness = 3 };
			this.GraphPlot.AddVerticalRule(0, axisStyle);
			this.GraphPlot.AddHorizontalRule(0, axisStyle);

			for (var i = yBottom + yInterval; i <= yTop; i += yInterval)
				this.GraphPlot.AddHorizontalTickLabel(xLeft, Math.Round(i, 2),
					new Mathematics.Graphing.LabelStyle { LeftMargin = -35 });

			for (var i = xLeft + xInterval; i <= xRight; i += xInterval)
				this.GraphPlot.AddVerticalTickLabel(Math.Round(i, 2), yBottom,
					new Mathematics.Graphing.LabelStyle { BottomMargin = -20 });
		}

		private void FindRange(double lowerBound, double upperBound,
			out double yMaxTop, out double yMinBottom, out double xMinLeft, out double xMaxRight)
		{
			var interval = (upperBound - lowerBound) / 100d;

			yMaxTop = Double.NegativeInfinity;
			yMinBottom = Double.PositiveInfinity;
			xMinLeft = Double.PositiveInfinity;
			xMaxRight = Double.NegativeInfinity;

			for (var j = 0; j < this.FormulaList.Count(); ++j)
			{
				var formula = this.FormulaList[j];

				if (formula.Function == null || formula.Derivative == null)
					continue;

				double? yTopNullable = null;
				double? yBottomNullable = null;
				double? xLeftNullable = null;
				double? xRightNullable = null;

				for (var i = lowerBound; i <= upperBound; i += interval)
				{
					var formulaValue = formula.Function(i);
					var derivativeValue = formula.Derivative(i);

					if (Double.IsNaN(formulaValue) || Double.IsNaN(derivativeValue) ||
						Double.IsInfinity(formulaValue) || Double.IsInfinity(derivativeValue))
						continue;

					xRightNullable = i;

					if (!xLeftNullable.HasValue)
						xLeftNullable = i;

					yTopNullable = Math.Max(yTopNullable ?? Double.NegativeInfinity, formulaValue);
					yBottomNullable = Math.Min(yBottomNullable ?? Double.PositiveInfinity, formulaValue);
				}

				if (!yTopNullable.HasValue)
					throw new Exception(string.Format("Formula has no valid values in the range [{0}, {1}]", lowerBound, upperBound));

				formula.LowerBound = xLeftNullable.Value;
				formula.UpperBound = xRightNullable.Value;

				yMaxTop = Math.Max(yMaxTop, yTopNullable.Value);
				yMinBottom = Math.Min(yMinBottom, yBottomNullable.Value);
				xMinLeft = Math.Min(xMinLeft, xLeftNullable.Value);
				xMaxRight = Math.Max(xMaxRight, xRightNullable.Value);
			}

			yMaxTop = Double.IsNegativeInfinity(yMaxTop) ? upperBound : yMaxTop;
			yMinBottom = Double.IsPositiveInfinity(yMinBottom) ? lowerBound : yMinBottom;
			xMinLeft = Double.IsPositiveInfinity(xMinLeft) ? lowerBound : xMinLeft;
			xMaxRight = Double.IsNegativeInfinity(xMaxRight) ? upperBound : xMaxRight;
		}

		public void DrawGraph(double lowerBound, double upperBound)
		{
			DrawGridAndAxes(lowerBound, upperBound);

			var colours = new[] { new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Blue), new SolidColorBrush(Colors.Lime) };

			var formulaList = this.FormulaList.Zip(colours, (f, c) => new { Formula = f, Colour = c });
			foreach (var formula in formulaList)
			{
				var combinedFormula = formula.Formula;
				combinedFormula.Colour = formula.Colour;

				if (combinedFormula.Function == null || combinedFormula.Derivative == null)
					continue;

				this.GraphPlot.AddFunction(new Function(combinedFormula.Function, combinedFormula.Derivative),
					combinedFormula.LowerBound, combinedFormula.UpperBound, 100,
					new Mathematics.Graphing.GraphStyle { Stroke = combinedFormula.Colour, Thickness = 3 });
			}
		}

		public void DrawGraph(IList<string> functionStrings, double lowerBound, double upperBound)
		{
			var functionList = CreateFunctions(functionStrings).ToList();
			var derivativeList = functionList.Select(Derivative).ToList();

			var combinedFormulae = functionList.Zip(derivativeList, (f, d) => new { Function = f, Derivative = d });
			var formulaList = combinedFormulae.Select(formula => new FormulaPlot
				{
					Function = formula.Function,
					Derivative = formula.Derivative
				}).ToList();

			this.FormulaList = formulaList;

			DrawGraph(lowerBound, upperBound);
		}

		public static Func<double, double> Derivative(Func<double, double> f)
		{
			return x => (f(x + Limit) - f(x)) / Limit;
		}
	}
}

using System;
using System.Reflection;
using System.Windows;

namespace Graphing
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				String resourceName = Assembly.GetExecutingAssembly().GetName().Name + "." + new AssemblyName(args.Name).Name + ".dll";

				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
				{
					Byte[] assemblyData = new Byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);

					return Assembly.Load(assemblyData);
				}
			};
		}
	}
}

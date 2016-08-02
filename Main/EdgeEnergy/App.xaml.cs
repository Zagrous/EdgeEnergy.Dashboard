//-------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------


using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using EdgeEnergy.CutterDashboard.Assets.LocalizedResources;
using EdgeEnergy.CutterDashboard.Utils;
using log4net;

namespace EdgeEnergy.CutterDashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private string language;
        private static readonly ILog Log = LogManager.GetLogger("root");

		/// <summary>
		/// Initializes a new instance of the <see cref="App"/> class.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">More than one instance of the <see cref="T:System.Windows.Application"/> class is created per <see cref="T:System.AppDomain"/>.</exception>
		public App()
		{
		    GlobalContext.Properties["LogFile"] = FileUtil.GetLogFile();
		    log4net.Config.XmlConfigurator.Configure();

            FileUtil.SetCurrentPath();
            Log.InfoFormat("Starting EdgeEnergyDashboard: {0}", FileUtil.GetCurrentPath());

			//Startup += AppStartup;
			DispatcherUnhandledException += OnDispatcherUnhandledException;

			SetupLocalization();
			InitializeComponent();
		}

		/// <summary>
		/// This method checks if we have a localization for the current culture's language and if not falls back 
		/// the CurrentCulture and CurrentUICulture to the ones defined in the default resources.
		/// It also sets the language of the root visual. This affects all converters in data bindings.
		/// </summary>
		/// <returns></returns>
		private void SetupLocalization()
		{
			// Let's get the startup parameters to see if we need to "force" a locale.  We can look at the query parameters"
			// for click-once deployments, and command line parameters for a normal execution.  
			// Note: We skip the first command line parameter as it is always the path to the running exe.
            //NameValueCollection cmdlineParams = Infragistics.Windows.Toolkit.Web.HttpUtilities.QueryStringParameters() ??
            //                    System.Environment.GetCommandLineArgs().Skip(1).ToArray().ToNameValueCollection();
            //string lc = cmdlineParams["lc"];
            //CultureInfo uiCulture;
            //if (lc != null)
            //{
            //    uiCulture = new CultureInfo(lc);
            //    Thread.CurrentThread.CurrentUICulture = uiCulture;
            //}
            //else
            //{
            //    uiCulture = CultureInfo.CurrentUICulture;
            //}
            var uiCulture = new CultureInfo("en-GB");

			var resourceCulture = new CultureInfo(Strings.Language);
			if (uiCulture.TwoLetterISOLanguageName != resourceCulture.TwoLetterISOLanguageName)
			{
				uiCulture = resourceCulture;
				Thread.CurrentThread.CurrentUICulture = uiCulture;
			}

			Thread.CurrentThread.CurrentCulture = uiCulture;
			language = uiCulture.Name;
		}

		/// <summary>
		/// Apps the startup.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.Windows.StartupEventArgs"/> instance containing the event data.</param>
		private void AppStartup(object sender, StartupEventArgs e)
		{
			var mainWindow = new MainWindow {Language = XmlLanguage.GetLanguage(language)};
		    MainWindow = mainWindow;
			MainWindow.Show();
		}

		/// <summary>
		/// Handles the DispatcherUnhandledException event of the App control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
		static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				System.Diagnostics.Debugger.Break();
			}

			e.Handled = true;
			ErrorWindow.CreateNew(e.Exception, StackTracePolicy.Always);
		}
    }
}

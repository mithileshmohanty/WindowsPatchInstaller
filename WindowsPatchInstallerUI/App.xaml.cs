using log4net;
using System;
using System.Windows;

namespace WindowsPatchInstallerUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILog _log;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger("WindowsPatchInstallerUI");
            _log.Info("WindowsPatchInstallerUI started.");
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = "ERROR: Dispatcher Unhandled Exception. ";
            if (e.Exception != null && e.Exception.Message != null)
                errorMessage += " :: " + e.Exception.Message;
            if (e.Exception != null)
                errorMessage += " :: " + e.Exception.StackTrace;
            _log.Error(errorMessage);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMessage = "ERROR: App Domain Unhandled Exception. ";
            if (e.ExceptionObject != null && ((Exception)e.ExceptionObject).Message != null)
                errorMessage += " :: " + ((Exception)e.ExceptionObject).Message;
            if (e.ExceptionObject != null && ((Exception)e.ExceptionObject).StackTrace != null)
                errorMessage += " :: " + ((Exception)e.ExceptionObject).StackTrace;
            _log.Error(errorMessage);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = "ERROR: Current Dispatcher Unhandled Exception. ";
            if (e.Exception != null && e.Exception.Message != null)
                errorMessage += " :: " + e.Exception.Message;
            if (e.Exception != null)
                errorMessage += " :: " + e.Exception.StackTrace;
            _log.Error(errorMessage);
        }
    }
}

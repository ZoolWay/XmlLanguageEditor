namespace Zw.XmlLanguageEditor
{
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;
    using ViewModels;

    public class AppBootstrapper : BootstrapperBase
    {

        private static readonly log4net.ILog log;

        static AppBootstrapper()
        {
            log4net.Config.XmlConfigurator.Configure();
            log = global::log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        private SimpleContainer container;

        public AppBootstrapper()
        {
            log.Debug("Creating bootstrapper");
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            System.Windows.Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DisplayRootViewFor<IShell>();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Fatal(String.Format("Unhandles appdomain exception; object: '{0}' ({1})", e.ExceptionObject, e.ExceptionObject?.GetType().FullName), e.ExceptionObject as Exception);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            log.Fatal(String.Format("Unhandled dispatcher exception; dispatcher associated thread: '{0}'", e.Dispatcher?.Thread?.ManagedThreadId), e.Exception);
        }
    }
}
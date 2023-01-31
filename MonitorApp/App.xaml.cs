using System;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using MonitorApp.DataAccess.Services;
using MonitorApp.Helpers;
using MonitorApp.ViewModels;
using MonitorApp.Views;

namespace MonitorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Props

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public App()
        {
            Services = ConfigureServices();
        }

        /// <summary>
        /// When application starts set initial view
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShellWindow shellView = new()
            {
                DataContext = Services.GetService<IShell>()
            };

            shellView.Show();
            //Services.GetService<IThemeHelper>()?.Set(ThemeType.Dark, BackgroundType.Mica, false);
        }


        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IMessenger, WeakReferenceMessenger>();
            services.AddTransient<IShell, ShellViewModel>();
            services.AddTransient<IProcessHelper, ProcessHelper>();
            services.AddTransient<IConnectionHelper, ConnectionHelper>();
            services.AddTransient<IAppMonitorDbService, AppMonitorDbService>();
            services.AddTransient<ISnackbarMessageQueue, SnackbarMessageQueue>();


            return services.BuildServiceProvider();
        }
    }
}
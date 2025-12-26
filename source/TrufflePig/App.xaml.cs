using LCTWorks.Telemetry;
using LCTWorks.WinUI;
using LCTWorks.WinUI.Activation;
using LCTWorks.WinUI.Helpers;
using LCTWorks.WinUI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.Security;
using System.Threading.Tasks;
using TrufflePig.Services;
using TrufflePig.ViewModels;
using TrufflePig.Xaml.Views;

namespace TrufflePig;

public partial class App : Application, IAppExtended
{
    private Window? _window;

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
       CreateDefaultBuilder().
       UseContentRoot(AppContext.BaseDirectory).
       ConfigureServices((context, services) =>
       {
           services
           // Default Activation Handler
           .AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>()
           .AddSingleton<ActivationService>()

           // Services
           .AddSingleton<FrameNavigationService>()
           .AddSingleton<NavigationHistoryService>()
           .AddSentry(string.Empty, RuntimePackageHelper.Environment, RuntimePackageHelper.IsDebug(), RuntimePackageHelper.GetTelemetryContextData())

           //ViewModels
           .AddSingleton<ShellViewModel>()
           .AddTransient<WebPreviewViewModel>()

           //Views
           .AddSingleton<ShellPage>()
           .AddTransient<WebPreviewPage>()

           // Configuration
           //.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
           ;
       }).Build();

        InitializePageHelper();

        UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    public IHost Host
    {
        get;
    }

    public Window MainWindow => _window ??= new MainWindow();

    public static T GetService<T>()
        where T : class
    {
        if ((Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public void AppDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        //if (e.ExceptionObject is Exception exception)
        //{
        //    //AppCenterService.ReportUnhandledException(exception);
        //    exception.Data["AppExType"] = "AppDomainUnhandledException";
        //    _telemetryService.ReportUnhandledException(exception);
        //}
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var page = GetService<ShellPage>();
        await GetService<ActivationService>().ActivateAsync(args, page);
    }

    private static void InitializePageHelper()
    {
        NavigationPageMap.Configure<WebPreviewViewModel, WebPreviewPage>();
        //NavigationPageMap.Configure<UserCollectionViewModel, FontCollectionPage>();
        //NavigationPageMap.Configure<FontFamilyObservableObject, DetailsPage>();
    }

    [SecurityCritical]
    private void App_UnhandledException(object _, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        //_telemetryService?.ReportUnhandledException(e.Exception);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs? e)
    {
        //if (e?.Exception == null)
        //{
        //    return;
        //}
        //var flattenedExceptions = e.Exception.Flatten().InnerExceptions;
        //foreach (var exception in flattenedExceptions)
        //{
        //    _telemetryService.TrackError(exception);
        //}
        //e.SetObserved();
    }
}
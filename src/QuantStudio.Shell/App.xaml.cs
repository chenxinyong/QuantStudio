using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuantStudio.CTP.Data;
using Serilog;
using Serilog.Events;
using Volo.Abp;

namespace QuantStudio.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;
    private readonly IAbpApplicationWithExternalServiceProvider _abpApplication;
    private readonly IShellEngine _shellEngine;

    private IHost CreateHostBuilder()
    {
        return Host
            .CreateDefaultBuilder(null)
            .UseAutofac()
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication<ShellModule>();
            }).Build();
    }

    private void Initialize(IServiceProvider serviceProvider)
    {
        _abpApplication.Initialize(serviceProvider);
    }

    public App()
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt", rollingInterval: RollingInterval.Day))
            .CreateLogger();

        _host = CreateHostBuilder();
        _abpApplication = _host.Services.GetService<IAbpApplicationWithExternalServiceProvider>();
        _shellEngine = _host.Services.GetService<IShellEngine>();
    }

    protected async override void OnStartup(StartupEventArgs e)
    {
        try
        {
            Log.Information("Starting WPF host.");
            await _host.StartAsync();

            Initialize(_host.Services);


            // 主窗体
            MainWindow mainWindow = _host.Services.GetService<MainWindow>();
           
            // splashScreenWindow

            SplashWindow splashWindow = _host.Services.GetService<SplashWindow>();
            splashWindow.Show();

            await Task.Delay(1000 * 3);

            splashWindow.Close();

            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
        }
    }

    protected async override void OnExit(ExitEventArgs e)
    {
        _abpApplication.Shutdown();
        await _host.StopAsync();
        _host.Dispose();
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public new static App Current => (App)Application.Current;

    public IServiceProvider Services { get { return _host.Services; } }

    public ILogger Logger { get; }

    public IShellEngine ShellEngine { get { return _shellEngine; } }

}

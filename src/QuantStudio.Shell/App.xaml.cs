using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;

namespace QuantStudio.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;
    private readonly IAbpApplicationWithExternalServiceProvider _abpApplication;

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
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .CreateLogger();

        _host = CreateHostBuilder();
        _abpApplication = _host.Services.GetService<IAbpApplicationWithExternalServiceProvider>();
    }

    protected async override void OnStartup(StartupEventArgs e)
    {
        try
        {
            Log.Information("Starting WPF host.");
            await _host.StartAsync();

            Initialize(_host.Services);

            _host.Services.GetService<MainWindow>()?.Show();

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
}

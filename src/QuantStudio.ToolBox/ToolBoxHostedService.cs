using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuantStudio.CTP;
using Serilog;
using Volo.Abp;

namespace QuantStudio.ConsoleApp;

public class ToolBoxHostedService : IHostedService
{
    private IAbpApplicationWithInternalServiceProvider _abpApplication;

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public ToolBoxHostedService(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _abpApplication =  await AbpApplicationFactory.CreateAsync<ToolBoxModule>(options =>
        {
            options.Services.ReplaceConfiguration(_configuration);
            options.Services.AddSingleton(_hostEnvironment);

            options.UseAutofac();
            options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
        });

        await _abpApplication.InitializeAsync();

        var helloWorldService = _abpApplication.ServiceProvider.GetRequiredService<CTPMarketDataManager>();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _abpApplication.ShutdownAsync();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        var marketDataManager = _abpApplication.ServiceProvider.GetRequiredService<CTPMarketDataManager>();
        // 订阅行情数据
    }
}

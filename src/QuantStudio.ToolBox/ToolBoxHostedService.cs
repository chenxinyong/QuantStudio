using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuantStudio.CTP.Data;
using QuantStudio.CTP.Trade;
using Serilog;
using Volo.Abp;

namespace QuantStudio.ToolBox;

public class ToolBoxHostedService : IHostedService
{
    private IAbpApplicationWithInternalServiceProvider _abpApplication;

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;
    private DataManager _dataManager;

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

        _dataManager = _abpApplication.ServiceProvider.GetRequiredService<DataManager>();

        await _dataManager?.Initialize();
        await _dataManager?.RunAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _dataManager?.CloseTradingAsync();
        await _abpApplication.ShutdownAsync();
    }
}

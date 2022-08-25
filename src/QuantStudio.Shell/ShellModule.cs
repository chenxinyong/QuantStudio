using Microsoft.Extensions.DependencyInjection;
using QuantStudio.CTP;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace QuantStudio.Shell;

[DependsOn(typeof(AbpAutofacModule)
    , typeof(CTPModule)
    )]
public class ShellModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<MainWindow>();

        // Viewmodels
        context.Services.AddTransient<MainViewModel>();
    }
}

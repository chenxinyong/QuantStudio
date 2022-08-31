using Microsoft.Extensions.DependencyInjection;
using QuantStudio.CTP;
using QuantStudio.Shell.ViewModels;
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
        context.Services.AddSingleton<LoginWindow>();
        context.Services.AddSingleton<SplashWindow>();

        // Viewmodels
        context.Services.AddTransient<MainViewModel>();
        context.Services.AddTransient<LoginViewModel>();
        context.Services.AddTransient<SplashViewModel>();
    }
}

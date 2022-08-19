using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Security;

namespace QuantStudio.CTP;

[DependsOn(
    typeof(AbpAutofacModule)
    , typeof(AbpSecurityModule)
)]
public class CTPModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}

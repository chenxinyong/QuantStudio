using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace QuantStudio.Shell
{
    public interface IShellEngine : ISingletonDependency
    {
        public bool IsDataManagerConnected { get; }

        public Task RunDataManagerAsync();

        public Task StopDataManagerAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuantStudio.Shell
{
    public class Bootstrapper : ShellBootstrapperBase
    {
        protected override DependencyObject CreateShell()
        {
            return null;
            //return Container.Resolve<MainWindow>();
        }
    }
}

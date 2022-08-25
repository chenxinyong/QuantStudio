using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuantStudio.Shell
{
    public abstract class ShellApplicationBase : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeInternal();
        }

        /// <summary>
        /// Run the initialization process.
        /// </summary>
        void InitializeInternal()
        {
            Initialize();
            OnInitialized();
        }

        protected virtual void Initialize()
        {
            var shell = CreateShell();
            if (shell != null)
            {

                InitializeShell(shell);
            }
        }

        protected abstract Window CreateShell();

        protected virtual void InitializeShell(Window shell)
        {
            MainWindow = shell;
        }

        protected virtual void OnInitialized()
        {
            MainWindow?.Show();
        }

        protected virtual void InitializeModules()
        {
            
        }

    }
}

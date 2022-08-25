using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuantStudio.Shell
{
    public abstract class ShellBootstrapperBase
    {
        protected DependencyObject Shell { get; set; }

        public void Run()
        {
            ConfigureViewModelLocator();
            Initialize();
            OnInitialized();
        }

        protected virtual void ConfigureViewModelLocator()
        {
            // 
        }

        protected virtual void Initialize()
        {
            var shell = CreateShell();
            if (shell != null)
            {
                InitializeShell(shell);
            }

            InitializeModules();
        }


        protected abstract DependencyObject CreateShell();

        protected virtual void InitializeShell(DependencyObject shell)
        {
            Shell = shell;
        }

        protected virtual void OnInitialized()
        {
            if (Shell is Window window)
                window.Show();
        }

        protected virtual void InitializeModules()
        {

        }
    }
}

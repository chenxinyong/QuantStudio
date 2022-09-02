using QuantStudio.CTP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio.Shell
{
    public class ShellEngine : IShellEngine
    {
        private readonly DataManager _dataManager;

        public ShellEngine(DataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public bool IsDataManagerConnected { get { return _dataManager.IsConnected; } }

        public DataManager DataManager { get { return _dataManager; } }

        public async Task RunDataManagerAsync()
        {
            await _dataManager.Initialize();
            await _dataManager.RunAsync();
            await Task.CompletedTask;
        }

        public async Task StopDataManagerAsync()
        {
            await Task.CompletedTask;
        }
    }
}

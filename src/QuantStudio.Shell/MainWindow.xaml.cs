using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using QuantStudio.CTP.Data;
using QuantStudio.CTP.Trade;

namespace QuantStudio.Shell
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window ,IShell
    {
        private DataManager _dataManager;
        private TradeManager _tradeManager;
        public MainWindow(DataManager dataManager,TradeManager tradeManager, MainViewModel viewModel)
        {
            _dataManager = dataManager;
            _tradeManager = tradeManager;
            this.DataContext = viewModel;

            InitializeComponent();
        }

        #region IShell

        public async Task RunDataManagerAsync()
        {
            Task.Run(() => {
                _dataManager.Initialize();
                _dataManager.RunAsync();
            });
        }

        public async Task StopDataManagerAsync()
        {
            await Task.CompletedTask;
        }

        #endregion
    }
}

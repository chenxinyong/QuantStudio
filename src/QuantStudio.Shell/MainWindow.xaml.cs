using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using QuantStudio.CTP.Data;
using QuantStudio.CTP.Trade;
using System;
using System.IO;
using System.Windows.Input;
using QuantStudio.CTP.Data.Market;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Path = System.IO.Path;

namespace QuantStudio.Shell
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If data is dirty, prompt user and ask for a response
            bool isTradingNow = true;
            if (isTradingNow)
            {
                var result = MessageBox.Show("是否确定关闭应用程序？","提示",MessageBoxButton.YesNo);

                // User doesn't want to close, cancel closure
                if (result == MessageBoxResult.No)
                {
                    // 保存数据或其他重要的操作

                    e.Cancel = true;
                }
            }
        }


        private async void btnLoadCsvFiles_Click(object sender, RoutedEventArgs e)
        {
            var _csvFileDataPovider = App.Current.Services.GetService<CsvDataPovider>();
            var _shell = App.Current.ShellEngine;

            Dictionary<string, List<MarketData>> dict = new Dictionary<string, List<MarketData>>();
            // loadCsvFiles
            string folderName =  Path.Combine( $"D:\\Ticks\\App_Data\\Ticks");
            string symbol = "fu2301";
            DirectoryInfo folder = new DirectoryInfo(folderName);
            FileInfo[] files = folder.GetFiles($"*{symbol}*", SearchOption.AllDirectories);
            List<FileInfo> fileNames = new List<FileInfo>();
            fileNames.AddRange(files);
            foreach(FileInfo fileInfo in fileNames)
            {
                string[] fileNameSplits = fileInfo.Name.Split('_');
                string quoteDate = fileNameSplits[1].Split('.')[0];
                // quoteDate = $"{quoteDate.Substring(0, 4)}{quoteDate.Substring(4, 2)}{quoteDate.Substring(6, 2)}";

                var tResult = await _csvFileDataPovider.ReadFilesAsync(fileInfo.FullName);
                if(tResult.Any())
                {
                    if(!dict.ContainsKey(quoteDate))
                    {
                        dict.Add(quoteDate, new List<MarketData>() {});
                    }

                    dict[quoteDate] = tResult;
                }
            }

            // 保存Tick数据
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Current.ShellEngine
        }
    }
}

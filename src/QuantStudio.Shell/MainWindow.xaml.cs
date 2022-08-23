using CTP;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using QuantStudio.CTP;
using QuantStudio.CTP.Data;
using QuantStudio.CTP.Trader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Volo.Abp.DependencyInjection;

namespace QuantStudio.Shell;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly CsvFileDataPovider _csvFileMarketDataReader;
    IConfiguration _configuration;
    IHostEnvironment _environment;
    DataManager _dataManager;

    public MainWindow(IHostEnvironment environment,IConfiguration configuration, CsvFileDataPovider csvFileMarketDataReader,DataManager dataManager)
    {
        _environment = environment;
        _configuration = configuration;
        _csvFileMarketDataReader = csvFileMarketDataReader;

        _dataManager = dataManager;

        InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e)
    {

    }

    private void ButtonAddName_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtName.Text) && !lstNames.Items.Contains(txtName.Text))
        {
            lstNames.Items.Add(txtName.Text);
            txtName.Clear();
        }
    }

    private void txtName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            if (!string.IsNullOrWhiteSpace(txtName.Text) && !lstNames.Items.Contains(txtName.Text))
            {
                lstNames.Items.Add(txtName.Text);
                txtName.Clear();
            }
        }
    }

    Action _cancelWork;

    private async Task DoDataManagerRun(CancellationToken token)
    {
        _dataManager.Initialize();
        await _dataManager.RunAsync();
    }

    private async void LoadCsvFiles_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();

            this._cancelWork = () =>
            {
                cancellationTokenSource.Cancel();
            };

            this.btnAddOk.IsEnabled = false;

            var token = cancellationTokenSource.Token;

            await Task.Run(() => DoDataManagerRun(token));
        }
        catch(Exception ex)
        {

        }

        this.btnAddOk.IsEnabled = true;
        this._cancelWork = null;
    }
}

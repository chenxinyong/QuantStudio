using CTP;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using QuantStudio.CTP;
using QuantStudio.CTP.Data;
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

    public MainWindow(IHostEnvironment environment,IConfiguration configuration, CsvFileDataPovider csvFileMarketDataReader)
    {
        _environment = environment;
        _configuration = configuration;
        _csvFileMarketDataReader = csvFileMarketDataReader;

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

    private void LoadCsvFiles_Click(object sender, RoutedEventArgs e)
    {
        // LoadCsvFiles
        List<FileInfo> fileNames = new List<FileInfo>();

        string fileName = Path.Combine(_environment.ContentRootPath, CTPConsts.MarketDataFolder.App_Data, "FutAC_TickKZ_CTP_Daily_202207");
        if (Directory.Exists(fileName))
        {
            var items = _csvFileMarketDataReader.ReadFilesAsync(fileName,"20220729");
        }

        //string fileName = Path.Combine(_environment.ContentRootPath, CTPConsts.MarketDataFolder.App_Data, "FutAC_TickKZ_CTP_Daily_202207","sc");
        //if(File.Exists(fileName))
        //{
        //    fileNames.Add(new FileInfo(fileName));
        //    var items = _csvFileMarketDataReader.ReadFilesAsync(fileNames);
        //}
    }
}

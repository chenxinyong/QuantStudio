using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Configuration;
using QuantStudio.CTP.Data.Market;
using System.Globalization;

namespace QuantStudio.CTP.Data
{
    /// <summary>
    /// HDF5文件读取与写入
    /// </summary>
    public class HDF5DataProvider : ITransientDependency
    {
        #region private

        IConfiguration _configuration;
        IHostEnvironment _environment;

        #endregion

        #region Ctor / Initialize

        public ILogger<CsvDataPovider> Logger { get; set; }

        public HDF5DataProvider(IConfiguration configuration, IHostEnvironment environment)
        {
            Logger = NullLogger<CsvDataPovider>.Instance;
            _configuration = configuration;
            _environment = environment;
        }

        public void Initialize()
        {

        }

        #endregion

        public async Task<List<MarketData>> ReadFilesAsync(string fileName)
        {
            // 文件夹
            if (File.Exists(fileName))
            {
                FileInfo fileNames = new FileInfo(fileName);
                List<MarketData> cTPMarketDatas = await ReadFilesAsync(fileNames);

                return cTPMarketDatas;
            }
            else
            {
                return new List<MarketData>();
            }
        }

        public async Task<List<MarketData>> ReadFilesAsync(FileInfo fileInfo)
        {
            List<MarketData> mdList = new List<MarketData>();

            if (File.Exists(fileInfo.FullName))
            {
                // fileName
            }

            return mdList;
        }
    }
}

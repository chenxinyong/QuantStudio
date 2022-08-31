
using CTP;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using QuantStudio.CTP.Data.Market;
using CsvHelper;

namespace QuantStudio.CTP.Data
{

    /// <summary>
    /// Csv文件格式的数据读写提供者
    /// </summary>
    public class CsvDataPovider : ITransientDependency
    {
        #region private

        IConfiguration _configuration;
        IHostEnvironment _environment;

        #endregion

        #region Ctor / Initialize

        public ILogger<CsvDataPovider> Logger { get; set; }

        public CsvDataPovider(IConfiguration configuration, IHostEnvironment environment)
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
            List<MarketData> mdList = new List<MarketData>();
            
            if(File.Exists(fileName))
            {
                // fileName
                using (var reader = new StreamReader(fileName))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<MarketData>();
                        if (await csv.ReadAsync())
                        {
                            csv.ReadHeader();

                            while (csv.Read())
                            {
                                MarketData record = new MarketData(
                                    csv.GetField<string>(0), csv.GetField<string>(1), csv.GetField<string>(2), csv.GetField<string>(3), csv.GetField<decimal>(4), csv.GetField<decimal>(5), csv.GetField<decimal>(6), csv.GetField<decimal>(7),
                                     csv.GetField<decimal>(8), csv.GetField<decimal>(9), csv.GetField<decimal>(10), csv.GetField<int>(11), csv.GetField<decimal>(12), csv.GetField<decimal>(13),
                                     csv.GetField<decimal>(14), csv.GetField<decimal>(15), csv.GetField<decimal>(16), csv.GetField<decimal>(17), csv.GetField<decimal>(18), csv.GetField<decimal>(19), TimeOnly.Parse($"{csv.GetField<string>(20)}"), csv.GetField<int>(21),
                                     csv.GetField<decimal>(22), csv.GetField<int>(23), csv.GetField<decimal>(24), csv.GetField<int>(25), csv.GetField<decimal>(26), csv.GetField<int>(27), csv.GetField<decimal>(28), csv.GetField<int>(29), csv.GetField<decimal>(30), csv.GetField<int>(31), csv.GetField<decimal>(32), csv.GetField<int>(33),
                                     csv.GetField<decimal>(34), csv.GetField<int>(35), csv.GetField<decimal>(36), csv.GetField<int>(37), csv.GetField<decimal>(38), csv.GetField<int>(39), csv.GetField<decimal>(40), csv.GetField<int>(41), csv.GetField<decimal>(42), csv.GetField<string>(43)
                                    ); 

                                mdList.Add(record);
                            }
                        }
                    }
                }
            }

            return mdList;
        }
    }
}

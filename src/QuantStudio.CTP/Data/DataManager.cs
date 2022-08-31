using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using CTP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Security.Encryption;
using Microsoft.Extensions.Configuration;
using QuantStudio.Data.Market;
using static QuantStudio.CTP.CTPConsts;
using CsvHelper;
using System.Globalization;
using Path = System.IO.Path;
using QuantStudio.CTP.Data.Market;
using CsvHelper.Configuration;
using System.Linq.Dynamic.Core.Tokenizer;

namespace QuantStudio.CTP.Data
{
    /// <summary>
    /// CTP行情数据管理
    /// </summary>
    public class DataManager : ISingletonDependency
    {
        #region private 

        private CTPMdReceiver _mdReceiver;
        private CTPSettings _ctpSettings;
        private IConfiguration _configuration;
        private IHostEnvironment _hostEnvironment;
        private IStringEncryptionService _stringEncryptionService;

        // Tick / Bars / 当前的行情数据
        private Dictionary<string, MarketData> _dictCurrentCTPTicks = new Dictionary<string, MarketData>();

        // MarketData队列
        private Dictionary<string, Queue<MarketData>> _dictQueueMarketDatas = new Dictionary<string, Queue<MarketData>>();

        // ThostFtdcDepthMarketDataField 队列
        private ConcurrentQueue<ThostFtdcDepthMarketDataField> _ftdcDepthMarketDataFieldsQueue = new ConcurrentQueue<ThostFtdcDepthMarketDataField>();

        private MarketData ConverTo(ThostFtdcDepthMarketDataField MarketData)
        {
            MarketData data = new MarketData(
                MarketData.TradingDay,
                MarketData.InstrumentID,
                MarketData.ExchangeID,
                MarketData.ExchangeInstID,
                MarketData.LastPrice.SafeDecimalCast().Normalize(),
                MarketData.PreSettlementPrice.SafeDecimalCast().Normalize(),
                MarketData.PreClosePrice.SafeDecimalCast().Normalize(),
                MarketData.PreOpenInterest.SafeDecimalCast().Normalize(),

                MarketData.OpenPrice.SafeDecimalCast().Normalize(),
                MarketData.HighestPrice.SafeDecimalCast().Normalize(),
                MarketData.LowestPrice.SafeDecimalCast().Normalize(),
                MarketData.Volume,
                MarketData.Turnover.SafeDecimalCast().Normalize(),
                MarketData.OpenInterest.SafeDecimalCast().Normalize(),
                MarketData.ClosePrice.SafeDecimalCast().Normalize(),
                MarketData.SettlementPrice.SafeDecimalCast().Normalize(),

                MarketData.UpperLimitPrice.SafeDecimalCast().Normalize(),
                MarketData.LowerLimitPrice.SafeDecimalCast().Normalize(),

                MarketData.PreDelta.SafeDecimalCast().Normalize(),
                MarketData.CurrDelta.SafeDecimalCast().Normalize(),
                TimeOnly.Parse(MarketData.UpdateTime),
                MarketData.UpdateMillisec,

                // 买1/卖1
                MarketData.BidPrice1.SafeDecimalCast().Normalize(),
                MarketData.BidVolume1,
                MarketData.AskPrice1.SafeDecimalCast().Normalize(),
                MarketData.AskVolume1,
                // 买2/卖2
                MarketData.BidPrice2.SafeDecimalCast().Normalize(),
                MarketData.BidVolume2,
                MarketData.AskPrice2.SafeDecimalCast().Normalize(),
                MarketData.AskVolume2,
                // 买3/卖3
                MarketData.BidPrice3.SafeDecimalCast().Normalize(),
                MarketData.BidVolume3,
                MarketData.AskPrice3.SafeDecimalCast().Normalize(),
                MarketData.AskVolume3,
                // 买4/卖4
                MarketData.BidPrice4.SafeDecimalCast().Normalize(),
                MarketData.BidVolume4,
                MarketData.AskPrice4.SafeDecimalCast().Normalize(),
                MarketData.AskVolume4,
                // 买5/卖5
                MarketData.BidPrice5.SafeDecimalCast().Normalize(),
                MarketData.BidVolume5,
                MarketData.AskPrice5.SafeDecimalCast().Normalize(),
                MarketData.AskVolume5,
                // 均价
                MarketData.AveragePrice.SafeDecimalCast().Normalize(),
                MarketData.ActionDay
            );

            return data;
        }

        #region DataApi

        private void _mdReceiver_OnDepthMarketDataEvent(object? sender, DepthMarketDataArgs e)
        {
            // 
            _ftdcDepthMarketDataFieldsQueue.Enqueue(e.MarketData);
        }

        private async void _mdReceiver_OnHeartBeatEvent(object? sender, HeartBeatEventArgs e)
        {
            int threeMinutes = 1000 * 60 * 3;
            while (true)
            {
                if (!_mdReceiver.IsConnected)
                {
                    await _mdReceiver.Connect();
                }

                await Task.Delay(threeMinutes);
            }
        }

        private async void _mdReceiver_OnCloseTradingEvent(object? sender, CloseTradingArgs e)
        {
            // 收盘作业
            await DoCloseTradingEvent(DateTime.Now);
        }

        #endregion

        private Action _autoConnecting;

        private async Task HandlerDepthMarket()
        {
            ThostFtdcDepthMarketDataField depthMarketData = new ThostFtdcDepthMarketDataField();
            while (true)
            {
                if(_ftdcDepthMarketDataFieldsQueue.TryDequeue(out depthMarketData))
                {
                    // 放入对应的队列中
                    if (!_dictQueueMarketDatas.ContainsKey(depthMarketData.InstrumentID))
                    {
                        _dictQueueMarketDatas.Add(depthMarketData.InstrumentID, new Queue<MarketData>());
                    }

                    MarketData marketData = ConverTo(depthMarketData);
                    _dictQueueMarketDatas[depthMarketData.InstrumentID].Enqueue(marketData);

                    // 当前Tick
                    if(!_dictCurrentCTPTicks.ContainsKey(depthMarketData.InstrumentID))
                    {
                        _dictCurrentCTPTicks.Add(depthMarketData.InstrumentID,marketData);
                    }
                    else
                    {
                        _dictCurrentCTPTicks[depthMarketData.InstrumentID] = marketData;
                    }
                }

                await DoCloseTradingEvent(DateTime.Now);
            }
        }

        private async Task DoCloseTradingEvent(DateTime dt)
        {
            // 自动执行收盘作业
            if (_dictQueueMarketDatas.Values.Any() && CTPClosingTradingTimeFrames.IsCTPClosingTradingTradingTime(dt))
            {
                // 每分钟存储一次数据
                foreach (var kvp in _dictQueueMarketDatas)
                {
                    if(kvp.Value.Any())
                    {
                        string categoryCode = "";
                        if (kvp.Key.Substring(1, 1).ToCharArray()[0].IsBetween('/', ':'))
                        {
                            categoryCode = kvp.Key.Substring(0, 1);
                        }
                        else
                        {
                            categoryCode = kvp.Key.Substring(0, 2);
                        }

                        if (ExchangeCategories.ContainsKey(categoryCode))
                        {
                            string marketCode = ExchangeCategories[categoryCode].MarketCode;
                            categoryCode = ExchangeCategories[categoryCode].Symbol;

                            string symbolPath = Path.Combine(_hostEnvironment.ContentRootPath, MarketDataFolder.App_Data, MarketDataFolder.Ticks, marketCode, categoryCode);
                            if (!Directory.Exists(symbolPath))
                            {
                                Directory.CreateDirectory(symbolPath);
                            }
                            string tradingDate = kvp.Value.First().TradingDay;
                            string csvFileName = Path.Combine(symbolPath, $"{kvp.Key}_{tradingDate}.csv");
                            if (!File.Exists(csvFileName))
                            {
                                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                                {
                                    // write the header again.
                                    HasHeaderRecord = true,
                                };

                                using (var writer = new StreamWriter(csvFileName))
                                using (var csv = new CsvWriter(writer, config))
                                {
                                    await csv.WriteRecordsAsync(kvp.Value.ToList());
                                }
                            }
                            else
                            {
                                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                                {
                                    // Don't write the header again.
                                    HasHeaderRecord = false,
                                };

                                using (var stream = File.Open(csvFileName, FileMode.Append))
                                using (var writer = new StreamWriter(stream))
                                using (var csv = new CsvWriter(writer, config))
                                {
                                    csv.WriteRecords(kvp.Value.ToList());
                                }
                            }

                            kvp.Value.Clear();
                        }
                    }
                }
            }
            await Task.CompletedTask;
        }

        private List<string> GenerateInstrumentIDs()
        {
            List<string> instrumentIDs = new List<string>();
            var exchange = ExchangeCategories.ToList().GroupBy(p => p.Value.MarketCode);
            foreach (var exchangeItem in exchange)
            {
                DateOnly begin = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
                foreach(var item in exchangeItem)
                {
                    for (int i = 0; i <= 13; i++)
                    {
                        string dateTimeString = begin.AddMonths(i).ToString("yyyyMMdd");
                        string yearMonth = dateTimeString.Substring(2,4);
                        string contractCod = item.Value.Symbol;

                        if (exchangeItem.Key.Equals(Exchange.CZCE.Code))
                        {
                            yearMonth = dateTimeString.Substring(3, 3);
                        }

                        instrumentIDs.Add(contractCod + yearMonth);
                    }
                }
            }
            return instrumentIDs;
        }

        private void DoCTPSettings(CTPSettings settings)
        {
            List<string> subscribeInstrumentIDs = GenerateInstrumentIDs();

            _mdReceiver.SetsubscribeInstrumentID(subscribeInstrumentIDs);
        }

        #endregion

        public ILogger<DataManager> Logger { get; set; }

        #region ctor / Initialize

        public async Task Initialize()
        {
            _autoConnecting = async () => {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(3));
                    if (!_mdReceiver.IsConnected && CTPOnlineTradingTimeFrames.IsCTPOnlineTradingTime(DateTime.Now))
                    {
                        await _mdReceiver.Connect();
                    }
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mdReceiver"></param>
        public DataManager(CTPMdReceiver mdReceiver, IConfiguration configuration, IStringEncryptionService stringEncryptionService, IHostEnvironment hostEnvironment)
        {
            Logger = NullLogger<DataManager>.Instance;

            _mdReceiver = mdReceiver;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _stringEncryptionService = stringEncryptionService;

            _ctpSettings = _configuration.GetSection("CTPSettings").Get<CTPSettings>();

            // 解密敏感信息
            _ctpSettings.Investor.UserID = _stringEncryptionService.Decrypt(_ctpSettings.Investor.UserID);
            _ctpSettings.Investor.Password = _stringEncryptionService.Decrypt(_ctpSettings.Investor.Password);

            _mdReceiver = mdReceiver;
            _mdReceiver.OnHeartBeatEvent += _mdReceiver_OnHeartBeatEvent;
            _mdReceiver.OnDepthMarketDataEvent += _mdReceiver_OnDepthMarketDataEvent;
            _mdReceiver.OnCloseTradingEvent += _mdReceiver_OnCloseTradingEvent;

            _mdReceiver.Initialize(_ctpSettings, DoCTPSettings);
        }

        #endregion

        #region public 

        public async Task RunAsync()
        {
            if (CTPOnlineTradingTimeFrames.IsCTPOnlineTradingTime(DateTime.Now))
            {
                await _mdReceiver.Connect();
            }

            //  await Task.Run(() => _autoConnecting.Invoke());

            await HandlerDepthMarket();
        }

        /// <summary>
        /// 收盘作业
        /// </summary>
        /// <returns></returns>
        public async Task CloseTradingAsync()
        {
            await DoCloseTradingEvent(DateTime.Now);
        }

        #endregion
    }
}

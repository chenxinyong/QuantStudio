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
using QLNet;
using Path = System.IO.Path;
using QuantStudio.CTP.Data.Market;

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
                DateTime.Parse(MarketData.UpdateTime),
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

        private async Task DoMarketDataEvent(MarketData marketData)
        {
            // 放入对应的队列中
            if (!_dictQueueMarketDatas.ContainsKey(marketData.InstrumentID))
            {
                _dictQueueMarketDatas.Add(marketData.InstrumentID, new Queue<MarketData>());
            }

            _dictQueueMarketDatas[marketData.InstrumentID].Enqueue(marketData);

            // 最新数据
            if (!_dictCurrentCTPTicks.ContainsKey(marketData.InstrumentID))
            {
                _dictCurrentCTPTicks.Add(marketData.InstrumentID, marketData);
            }

            else
            {
                _dictCurrentCTPTicks[marketData.InstrumentID] = marketData;
            }

            // CTP数据存储
            await DoCloseTradingEvent();
        }

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
            await DoCloseTradingEvent();
        }

        private async Task DoCloseTradingEvent()
        {
            TimeOnly closeTrading = new TimeOnly(3, 0);
            TimeOnly timeOnly = TimeOnly.FromDateTime(DateTime.Now);
            if (timeOnly.Minute / 15 == 0)
            {
                // 每分钟存储一次数据
                foreach (var kvp in _dictQueueMarketDatas)
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
                        string symbolPath = Path.Combine(_hostEnvironment.ContentRootPath, MarketDataFolder.App_Data, marketCode, MarketDataFolder.Ticks);
                        if (!Directory.Exists(symbolPath))
                        {
                            Directory.CreateDirectory(symbolPath);
                        }

                        // 保存数据
                        using (var writer = new StreamWriter(Path.Combine(symbolPath, $"{kvp.Key}.csv")))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(kvp.Value.ToList());
                        }
                    }
                }
            }
            await Task.CompletedTask;
        }

        private void InitExchangePath(string appDataPath)
        {
            var categoriesExchange = ExchangeCategories.Values.GroupBy(p => p.MarketCode).ToList();
            foreach (var exchange in categoriesExchange)
            {
                // Itcks
                string itcksPath = Path.Combine(appDataPath, exchange.Key, MarketDataFolder.Ticks);
                if (!Directory.Exists(itcksPath))
                {
                    Directory.CreateDirectory(itcksPath);
                }

                // Minutes
                string minutesPath = Path.Combine(appDataPath, exchange.Key, MarketDataFolder.Minutes);
                if (!Directory.Exists(minutesPath))
                {
                    Directory.CreateDirectory(minutesPath);
                }

                // dailies
                string dialyPath = Path.Combine(appDataPath, exchange.Key, MarketDataFolder.Dialy);
                if (!Directory.Exists(dialyPath))
                {
                    Directory.CreateDirectory(dialyPath);
                }
            }
        }

        private void InitDataPath()
        {
            string appDataPath = Path.Combine(_hostEnvironment.ContentRootPath, MarketDataFolder.App_Data);
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            //// SHFE - 上海交易所
            InitExchangePath(appDataPath);
        }

        private void DoCTPSettings(CTPSettings settings)
        {
        }

        #endregion

        public ILogger<DataManager> Logger { get; set; }

        #region ctor / Initialize

        public async void Initialize()
        {
            if (CTPOnlineTradingTimeFrames.IsCTPOnlineTradingTime(DateTime.Now))
            {
                await _mdReceiver.Connect();
            }
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

            Initialize();
        }

        #endregion

        #region public 



        #endregion
    }
}

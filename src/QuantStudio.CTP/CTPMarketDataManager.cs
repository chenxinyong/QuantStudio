﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
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
using QuantStudio;
using static QuantStudio.CTP.CTPConsts;
using QuantStudio.CTP.Data.Market;
using CsvHelper;
using System.Globalization;

namespace QuantStudio.CTP
{
    /// <summary>
    /// CTP行情数据管理
    /// </summary>
    public class CTPMarketDataManager : ISingletonDependency
    {
        #region private 

        private CTPMdReceiver _mdReceiver;
        private CTPSettings _ctpSettings;
        private IConfiguration _configuration;
        private IHostEnvironment _hostEnvironment;
        private IStringEncryptionService _stringEncryptionService;

        // Tick / Bars / 当前的行情数据
        private Dictionary<string, CTPMarketData> _dictCurrentCTPTicks = new Dictionary<string, CTPMarketData>();

        // MarketData队列
        private Dictionary<string, Queue<CTPMarketData>> _dictQueueMarketDatas = new Dictionary<string, Queue<CTPMarketData>>();

        private CTPMarketData ConverTo(ThostFtdcDepthMarketDataField MarketData)
        {
            CTPMarketData data = new CTPMarketData(
                MarketData.TradingDay,
                MarketData.InstrumentID,
                MarketData.ExchangeID,
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

        private async Task DoMarketDataEvent(CTPMarketData marketData)
        {
            // 放入对应的队列中
            if (!_dictQueueMarketDatas.ContainsKey(marketData.InstrumentID))
            {
                _dictQueueMarketDatas.Add(marketData.InstrumentID, new Queue<CTPMarketData>());
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

        private async void _mdReceiver_OnDepthMarketDataEvent(object? sender, DepthMarketDataArgs e)
        {
            // 
            try
            {
                // 处理深度行情数据
                CTPMarketData cTPMarketData = ConverTo(e.MarketData);
                await DoMarketDataEvent(cTPMarketData);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private async void _mdReceiver_OnHeartBeatEvent(object? sender, HeartBeatEventArgs e)
        {
            int threeMinutes = 1000 * 60 * 3;
            while (true)
            {
                if(!_mdReceiver.IsConnected)
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
            TimeOnly timeOnly = TimeOnly.FromDateTime(DateTime.Now);
            if(timeOnly.Second == 0)
            {
                // 每分钟存储一次数据
                foreach(var kvp in _dictQueueMarketDatas)
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

                    if(CTPConsts.ExchangeCategories.ContainsKey(categoryCode))
                    {
                        string marketCode = CTPConsts.ExchangeCategories[categoryCode].MarketCode;
                        categoryCode = CTPConsts.ExchangeCategories[categoryCode].Symbol;
                        string symbolPath = Path.Combine(_hostEnvironment.ContentRootPath,CTPConsts.MarketDataFolder.App_Data, marketCode, CTPConsts.MarketDataFolder.Ticks);
                        if(!Directory.Exists(symbolPath))
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
            var categoriesExchange = CTPConsts.ExchangeCategories.Values.GroupBy(p => p.MarketCode).ToList();
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
            InitDataPath();
        }

        #endregion

        public ILogger<CTPMarketDataManager> Logger { get; set; }

        #region ctor / Initialize

        private async void Initialize()
        {
            if (CTPOnlineTradingTimeFrames.IsPOnlineTradingTime(DateTime.Now))
            {
                await _mdReceiver.Connect();
            }
            else
            {
                // 加载历史数据
                var marketGroups = CTPConsts.ExchangeCategories.Values.GroupBy(p => p.MarketCode);
                foreach(var market in marketGroups)
                {
                    string marketPath = Path.Combine(_hostEnvironment.ContentRootPath, CTPConsts.MarketDataFolder.App_Data, market.Key);
                    // ticks
                    foreach( var item in market)
                    {
                        string categoryPath = Path.Combine(marketPath, CTPConsts.MarketDataFolder.Ticks, item.Symbol);
                        if(Directory.Exists(categoryPath))
                        {
                            // 读取全部文件
                            DirectoryInfo directoryInfo = new DirectoryInfo(categoryPath);
                            FileInfo[] files =  directoryInfo.GetFiles();
                            files.ToList().ForEach(file => 
                            {
                                // csv文件读取
                                using (var reader = new StreamReader(file.FullName))
                                {
                                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                                    {
                                        var records = csv.GetRecords<CTPMarketData>();
                                        if(records.Any())
                                        {
                                            // 加入队列

                                        }
                                    }
                                }
                            });
                        }
                    }

                    // minutes
                    foreach (var item in market)
                    {
                        string categoryPath = Path.Combine(marketPath, CTPConsts.MarketDataFolder.Minutes, item.Symbol);
                        if (Directory.Exists(categoryPath))
                        {
                            // 读取全部文件
                            DirectoryInfo directoryInfo = new DirectoryInfo(categoryPath);
                            FileInfo[] files = directoryInfo.GetFiles();
                            files.ToList().ForEach(file =>
                            {
                                // csv文件读取
                                using (var reader = new StreamReader(file.FullName))
                                {
                                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                                    {
                                        var records = csv.GetRecords<CTPMarketData>();
                                        if (records.Any())
                                        {
                                            // 加入队列

                                        }
                                    }
                                }
                            });
                        }
                    }

                    foreach (var item in market)
                    {
                        string categoryPath = Path.Combine(marketPath, CTPConsts.MarketDataFolder.Dialy, item.Symbol);
                        if (Directory.Exists(categoryPath))
                        {
                            // 读取全部文件
                            DirectoryInfo directoryInfo = new DirectoryInfo(categoryPath);
                            FileInfo[] files = directoryInfo.GetFiles();
                            files.ToList().ForEach(file =>
                            {
                                // csv文件读取
                                using (var reader = new StreamReader(file.FullName))
                                {
                                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                                    {
                                        var records = csv.GetRecords<CTPMarketData>();
                                        if (records.Any())
                                        {
                                            // 加入队列

                                        }
                                    }
                                }
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mdReceiver"></param>
        public CTPMarketDataManager(CTPMdReceiver mdReceiver, IConfiguration configuration, IStringEncryptionService stringEncryptionService, IHostEnvironment hostEnvironment)
        {
            Logger = NullLogger<CTPMarketDataManager>.Instance;
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

            _mdReceiver.Initialize(_ctpSettings,DoCTPSettings);

            Initialize();
        }

        #endregion

        #region public 



        #endregion
    }
}

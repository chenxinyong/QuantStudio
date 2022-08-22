using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio.CTP.Data.Market
{
    //
    // 摘要:
    //     深度行情
    [StructLayout(LayoutKind.Sequential)]
    public class MarketData
    {
        //
        // 摘要:
        //     交易日
        public string TradingDay { get; internal set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string InstrumentID { get; internal set; }

        //
        // 摘要:
        //     交易所代码
        public string ExchangeID { get; internal set; }

        /// <summary>
        /// 交易所证券代码
        /// </summary>
        public string ExchangeInstID { get; internal set; }

        //
        // 摘要:
        //     最新价
        public decimal LastPrice { get; internal set; }

        //
        // 摘要:
        //     上次结算价
        public decimal PreSettlementPrice { get; internal set; }

        //
        // 摘要:
        //     昨收盘
        public decimal PreClosePrice { get; internal set; }

        //
        // 摘要:
        //     昨持仓量
        public decimal PreOpenInterest { get; internal set; }

        //
        // 摘要:
        //     今开盘
        public decimal OpenPrice { get; internal set; }

        //
        // 摘要:
        //     最高价
        public decimal HighestPrice { get; internal set; }

        //
        // 摘要:
        //     最低价
        public decimal LowestPrice { get; internal set; }

        //
        // 摘要:
        //     数量
        public int Volume { get; internal set; }

        //
        // 摘要:
        //     成交金额
        public decimal Turnover { get; internal set; }

        //
        // 摘要:
        //     持仓量
        public decimal OpenInterest { get; internal set; }

        //
        // 摘要:
        //     今收盘
        public decimal ClosePrice { get; internal set; }

        //
        // 摘要:
        //     本次结算价
        public decimal SettlementPrice { get; internal set; }

        //
        // 摘要:
        //     涨停板价
        public decimal UpperLimitPrice { get; internal set; }

        //
        // 摘要:
        //     跌停板价
        public decimal LowerLimitPrice { get; internal set; }

        //
        // 摘要:
        //     昨虚实度
        public decimal PreDelta { get; internal set; }

        //
        // 摘要:
        //     今虚实度
        public decimal CurrDelta { get; internal set; }

        //
        // 摘要:
        //     最后修改时间
        public DateTime UpdateTime { get; internal set; }

        //
        // 摘要:
        //     最后修改毫秒
        public int UpdateMillisec { get; internal set; }

        //
        // 摘要:
        //     申买价一
        public decimal BidPrice1 { get; internal set; }

        //
        // 摘要:
        //     申买量一
        public int BidVolume1 { get; internal set; }

        //
        // 摘要:
        //     申卖价一
        public decimal AskPrice1 { get; internal set; }

        //
        // 摘要:
        //     申卖量一
        public int AskVolume1 { get; internal set; }

        //
        // 摘要:
        //     申买价二
        public decimal BidPrice2 { get; internal set; }

        //
        // 摘要:
        //     申买量二
        public int BidVolume2 { get; internal set; }

        //
        // 摘要:
        //     申卖价二
        public decimal AskPrice2 { get; internal set; }

        //
        // 摘要:
        //     申卖量二
        public int AskVolume2 { get; internal set; }

        //
        // 摘要:
        //     申买价三
        public decimal BidPrice3 { get; internal set; }

        //
        // 摘要:
        //     申买量三
        public int BidVolume3 { get; internal set; }

        //
        // 摘要:
        //     申卖价三
        public decimal AskPrice3 { get; internal set; }

        //
        // 摘要:
        //     申卖量三
        public int AskVolume3 { get; internal set; }

        //
        // 摘要:
        //     申买价四
        public decimal BidPrice4 { get; internal set; }

        //
        // 摘要:
        //     申买量四
        public int BidVolume4 { get; internal set; }

        //
        // 摘要:
        //     申卖价四
        public decimal AskPrice4 { get; internal set; }

        //
        // 摘要:
        //     申卖量四
        public int AskVolume4 { get; internal set; }

        //
        // 摘要:
        //     申买价五
        public decimal BidPrice5 { get; internal set; }

        //
        // 摘要:
        //     申买量五
        public int BidVolume5 { get; internal set; }

        //
        // 摘要:
        //     申卖价五
        public decimal AskPrice5 { get; internal set; }

        //
        // 摘要:
        //     申卖量五
        public int AskVolume5 { get; internal set; }

        //
        // 摘要:
        //     当日均价
        public decimal AveragePrice { get; internal set; }

        //
        // 摘要:
        //     业务日期
        public string ActionDay { get; internal set; }

        public MarketData(string tradingDay, string instrumentID, string exchangeID, string exchangeInstID, decimal lastPrice, decimal preSettlementPrice, decimal preClosePrice, decimal preOpenInterest,
            decimal openPrice, decimal highestPrice, decimal lowestPrice, int volume, decimal turnover, decimal openInterest,
            decimal closePrice, decimal settlementPrice, decimal upperLimitPrice, decimal lowerLimitPrice, decimal preDelta, decimal currDelta, DateTime updateTime, int updateMillisec,
            decimal bidPrice1, int bidVolume1, decimal askPrice1, int askVolume1, decimal bidPrice2, int bidVolume2, decimal askPrice2, int askVolume2, decimal bidPrice3, int bidVolume3, decimal askPrice3, int askVolume3,
            decimal bidPrice4, int bidVolume4, decimal askPrice4, int askVolume4, decimal bidPrice5, int bidVolume5, decimal askPrice5, int askVolume5, decimal averagePrice, string actionDay
            )
        {
            TradingDay = tradingDay;
            InstrumentID = instrumentID;
            ExchangeID = exchangeID;
            ExchangeInstID = exchangeInstID;
            LastPrice = lastPrice;
            PreSettlementPrice = preSettlementPrice;
            PreClosePrice = preClosePrice;
            PreOpenInterest = preOpenInterest;
            OpenPrice = openPrice;
            HighestPrice = highestPrice;
            LowestPrice = lowestPrice;
            Volume = volume;
            Turnover = turnover;
            OpenInterest = openInterest;
            ClosePrice = closePrice;
            SettlementPrice = settlementPrice;
            UpperLimitPrice = upperLimitPrice;
            LowerLimitPrice = lowerLimitPrice;
            PreDelta = preDelta;
            CurrDelta = currDelta;

            UpdateTime = updateTime;
            UpdateMillisec = updateMillisec;

            BidPrice1 = bidPrice1;
            BidVolume1 = bidVolume1;
            AskPrice1 = askPrice1;
            AskVolume1 = askVolume1;

            BidPrice2 = bidPrice2;
            BidVolume2 = bidVolume2;
            AskPrice2 = askPrice2;
            AskVolume2 = askVolume2;

            BidPrice3 = bidPrice3;
            BidVolume3 = bidVolume3;
            AskPrice3 = askPrice3;
            AskVolume3 = askVolume3;

            BidPrice4 = bidPrice4;
            BidVolume4 = bidVolume4;
            AskPrice4 = askPrice4;
            AskVolume4 = askVolume4;

            BidPrice5 = bidPrice5;
            BidVolume5 = bidVolume5;
            AskPrice5 = askPrice5;
            AskVolume5 = askVolume5;

            AveragePrice = averagePrice;
            ActionDay = actionDay;
        }
    }
}

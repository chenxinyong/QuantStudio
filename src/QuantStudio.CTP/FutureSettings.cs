using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Studio.CTP
{
    [Serializable]
    public struct TradingTime
    {
        private TimeOnly _begin;
        private TimeOnly _end;

        public TradingTime(TimeOnly begin,TimeOnly end)
        {
            _begin = begin;
            _end = end;
        }
        public TimeOnly Begin { 
            get { return _begin; }
            set { _begin = value;} 
        }

        public TimeOnly End
        {
            get { return _end; }
            set { _end = value; }
        }
    }

    /// <summary>
    /// 期货品种
    /// </summary>
    [Serializable]
    public class FutureInfo
    {
        private List<TradingTime> _tradingTimes = new List<TradingTime>();

        public string Symbol { get; set; }

        public string Name { get; set; }

        public List<int> ContractMonths { get; set; } = new List<int>();

        public int LastTradingDay { get; set; } = 10;

        public List<string> TradingTimeFrame { get; set; } = new List<string>();

        /// <summary>
        /// 是否交易时间段
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public bool IsTradingTimeFrame(TimeOnly currentTime)
        {
            bool isTradingTimeFrame = false;
            if(TradingTimeFrame.Any() && !_tradingTimes.Any())
            {
                // 交易时间段
                foreach (string tradingTime in TradingTimeFrame)
                {
                    string[] tradingTimeFrames = tradingTime.Split('-');
                    if (tradingTimeFrames.Length == 2)
                    {
                        TimeOnly begin = new TimeOnly(9,0);
                        TimeOnly end = new TimeOnly(15, 0);
                        if (TimeOnly.TryParse(tradingTimeFrames[0],out begin) && TimeOnly.TryParse(tradingTimeFrames[1],out end))
                        {
                            _tradingTimes.Add(new TradingTime( begin, end));
                        }
                    }
                }
            }

            for(int i = 0; i < _tradingTimes.Count; i++)
            {
                TradingTime item = _tradingTimes[i];
                isTradingTimeFrame = currentTime >= item.Begin && currentTime < item.End;

                if(isTradingTimeFrame)
                {
                    return isTradingTimeFrame;
                }
            }

            return isTradingTimeFrame;
        }
    }

    /// <summary>
    /// 期货品种配置信息
    /// </summary>
    [Serializable]
    public class FutureSettings
    {
        /// <summary>
        /// 上海金融交易所
        /// </summary>
        public List<FutureInfo> CFFEX { get; set; } = new List<FutureInfo>();

        /// <summary>
        /// 上海商品交易所
        /// </summary>
        public List<FutureInfo> SHFE { get; set; } = new List<FutureInfo>();

        /// <summary>
        /// 大连商品交易所
        /// </summary>
        public List<FutureInfo> DCE { get; set; } = new List<FutureInfo>();

        /// <summary>
        /// 郑州商品交易所
        /// </summary>
        public List<FutureInfo> CZCE { get; set; } = new List<FutureInfo>();

        /// <summary>
        /// 上海能源交易所
        /// </summary>
        public List<FutureInfo> INE { get; set; } = new List<FutureInfo>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio
{
    /// <summary>
    /// 交易时间段
    /// </summary>
    public struct TradingTimeFrame
    {
        private readonly TimeOnly _begin;
        private readonly TimeOnly _end;

        public TimeOnly Begin { get { return _begin; } }

        public TimeOnly End { get { return _end; } }

        public TradingTimeFrame(TimeOnly begin,TimeOnly end)
        {
            _begin = begin;
            _end = end;
        }
    }

    /// <summary>
    /// CTP交易时间段定义
    /// </summary>
    public class CTPTradingTimeFrames
    {
        /// <summary>
        /// CTP行情及交易日间在线时间段
        /// </summary>
        public static TradingTimeFrame CTPOnlineDayOnlyTradingTime { get; private set; } = new TradingTimeFrame(new TimeOnly(8, 30), new TimeOnly(15, 30));

        /// <summary>
        /// 股票指数期货日盘交易时间段 - [ "09:30-11:30", "13:00-15:00" ]
        /// </summary>
        public static IReadOnlyList<TradingTimeFrame> IndexsDayOnly { get; private set; } = new List<TradingTimeFrame>() {
            new TradingTimeFrame(new TimeOnly(9,30),new TimeOnly(11,30)),
            new TradingTimeFrame(new TimeOnly(13,0),new TimeOnly(15,0))
        };

        /// <summary>
        /// 期货日盘 - [ "09:00-10:15", "10:30-11:30", "13:30-15:00" ]
        /// </summary>
        public IReadOnlyList<TradingTimeFrame> FuturesDayOnly { get; private set; } = new List<TradingTimeFrame>() {
            new TradingTimeFrame(new TimeOnly(9,0),new TimeOnly(10,15)),
            new TradingTimeFrame(new TimeOnly(10,30),new TimeOnly(11,30)),
            new TradingTimeFrame(new TimeOnly(13,30),new TimeOnly(15,0))
        };

        /// <summary>
        /// 期货夜盘 - [ "21:00-23:00", "09:00-10:15", "10:30-11:30", "13:30-15:00" ]
        /// </summary>
        public IReadOnlyList<TradingTimeFrame> FuturesDayNight { get; private set; } = new List<TradingTimeFrame>() {
            new TradingTimeFrame(new TimeOnly(21,0),new TimeOnly(23,0)),
            new TradingTimeFrame(new TimeOnly(9,0),new TimeOnly(10,15)),
            new TradingTimeFrame(new TimeOnly(10,30),new TimeOnly(11,30)),
            new TradingTimeFrame(new TimeOnly(13,30),new TimeOnly(15,0))
        };

        /// <summary>
        /// 期货隔夜夜盘 -  [ "21:00-01:00", "09:00-10:15", "10:30-11:30", "13:30-15:00" ]
        /// </summary>
        public IReadOnlyList<TradingTimeFrame> FuturesDayOvernight { get; private set; } = new List<TradingTimeFrame>() {
            new TradingTimeFrame(new TimeOnly(21,0),new TimeOnly(1,0)),
            new TradingTimeFrame(new TimeOnly(9,0),new TimeOnly(10,15)),
            new TradingTimeFrame(new TimeOnly(10,30),new TimeOnly(11,30)),
            new TradingTimeFrame(new TimeOnly(13,30),new TimeOnly(15,0))
        };

        /// <summary>
        /// 期货隔夜夜盘长时间段 -  [ "21:00-02:30", "09:00-10:15", "10:30-11:30", "13:30-15:00" ]
        /// </summary>
        public IReadOnlyList<TradingTimeFrame> FuturesDayOvernightLong { get; private set; } = new List<TradingTimeFrame>() {
            new TradingTimeFrame(new TimeOnly(21,0,0),new TimeOnly(2,30)),
            new TradingTimeFrame(new TimeOnly(9,0),new TimeOnly(10,15)),
            new TradingTimeFrame(new TimeOnly(10,30),new TimeOnly(11,30)),
            new TradingTimeFrame(new TimeOnly(13,30),new TimeOnly(15,0))
        };
    }
}

using CTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio.CTP
{

    /// <summary>
    /// CTP心跳事件参数
    /// </summary>
    public class HeartBeatEventArgs : EventArgs
    {

    }

    public class DepthMarketDataArgs : EventArgs
    {
        private ThostFtdcDepthMarketDataField _marketDataField { get; set; }

        public DepthMarketDataArgs(ThostFtdcDepthMarketDataField marketDataField)
        {
            this._marketDataField = marketDataField;
        }

        public ThostFtdcDepthMarketDataField MarketData { get { return _marketDataField; } }
    }

    /// <summary>
    /// 收盘事件
    /// </summary>
    public class CloseTradingArgs
    {

    }
}

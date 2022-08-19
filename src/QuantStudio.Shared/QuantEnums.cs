using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio
{
    /// <summary>
    /// 账户类型
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// 保证金
        /// </summary>
        Margin,

        /// <summary>
        /// 现金
        /// </summary>
        Cash
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SecurityType
    {
        /// <summary>
        /// Base class for all security types:
        /// </summary>
        Base,

        /// <summary>
        /// US Equity Security
        /// </summary>
        Equity,

        /// <summary>
        /// Option Security Type
        /// </summary>
        Option,

        /// <summary>
        /// Commodity Security Type
        /// </summary>
        Commodity,

        /// <summary>
        /// FOREX Security
        /// </summary>
        Forex,

        /// <summary>
        /// Future Security Type
        /// </summary>
        Future,

        /// <summary>
        /// Contract For a Difference Security Type.
        /// </summary>
        Cfd,

        /// <summary>
        /// Cryptocurrency Security Type.
        /// </summary>
        Crypto,

        /// <summary>
        /// Futures Options Security Type.
        /// </summary>
        /// <remarks>
        /// Futures options function similar to equity options, but with a few key differences.
        /// Firstly, the contract unit of trade is 1x, rather than 100x. This means that each
        /// option represents the right to buy or sell 1 future contract at expiry/exercise.
        /// The contract multiplier for Futures Options plays a big part in determining the premium
        /// of the option, which can also differ from the underlying future's multiplier.
        /// </remarks>
        FutureOption,

        /// <summary>
        /// Index Security Type.
        /// </summary>
        Index,

        /// <summary>
        /// Index Option Security Type.
        /// </summary>
        /// <remarks>
        /// For index options traded on American markets, they tend to be European-style options and are Cash-settled.
        /// </remarks>
        IndexOption,
    }

    /// <summary>
    /// 期货品种交易时间段
    /// </summary>
    public enum TradingTimeFrameType
    {
        /// <summary>
        /// 期货指数，股票日盘
        /// </summary>
        IndexsDayOnly,

        /// <summary>
        /// 期货日盘
        /// </summary>
        FuturesDayOnly,

        /// <summary>
        /// 期货夜盘
        /// </summary>
        FuturesDayNight,

        /// <summary>
        /// 期货夜盘隔夜
        /// </summary>
        FuturesDayOvernight,

        /// <summary>
        /// 期货夜盘隔夜最长
        /// </summary>
        FuturesDayOvernightLong
    }

    /// <summary>
    /// 市场数据类型
    /// </summary>
    public enum MarketDataType
    {
        /// OHLC
        Base,

        /// TradeBar (OHLC summary bar)
        TradeBar,

        /// Tick market data type (price-time pair)
        Tick,

        /// QuoteBar market data type [Bid(OHLC), Ask(OHLC) and Mid(OHLC) summary bar]
        QuoteBar
    }

    /// <summary>
    /// Tick类型
    /// </summary>
    public enum TickType
    {
        /// Trade type tick object.
        Trade,
        /// Quote type tick object.
        Quote,
        /// Open Interest type tick object (for options, futures)
        OpenInterest
    }

    /// <summary>
    /// 数据粒度
    /// </summary>
    public enum Resolution
    {
        /// Tick Resolution (1)
        Tick,
        /// Second Resolution (2)
        Second,
        /// Minute Resolution (3)
        Minute,
        /// Hour Resolution (4)
        Hour,
        /// Daily Resolution (5)
        Daily
    }

    /// <summary>
    /// 头寸方向
    /// </summary>
    public enum PositionSide
    {
        /// <summary>
        /// 空，数量小于0
        /// </summary>
        Short = -1,

        /// <summary>
        /// 没有方向
        /// </summary>
        None = 0,

        /// <summary>
        /// 多, 数据大于0
        /// </summary>
        Long = 1
    }

    /// <summary>
    /// 结算方式
    /// </summary>
    public enum SettlementType
    {
        /// <summary>
        /// 实物结算
        /// </summary>
        PhysicalDelivery,

        /// <summary>
        /// 结算时支付 / 收到现金
        /// </summary>
        Cash
    }

    /// <summary>
    /// 策略运行状态
    /// </summary>
    public enum AlgorithmStatus
    {
        /// Error compiling algorithm at start
        DeployError,    //1
        /// Waiting for a server
        InQueue,        //2
        /// Running algorithm
        Running,        //3
        /// Stopped algorithm or exited with runtime errors
        Stopped,        //4
        /// Liquidated algorithm
        Liquidated,     //5
        /// Algorithm has been deleted
        Deleted,        //6
        /// Algorithm completed running
        Completed,      //7
        /// Runtime Error Stoped Algorithm
        RuntimeError,    //8
        /// Error in the algorithm id (not used).
        Invalid,
        /// The algorithm is logging into the brokerage
        LoggingIn,
        /// The algorithm is initializing
        Initializing,
        /// History status update
        History
    }

    /// <summary>
    /// 订阅数据的来源类型
    /// </summary>
    public enum SubscriptionTransportMedium
    {
        /// <summary>
        /// The subscription's data comes from disk
        /// </summary>
        LocalFile,

        /// <summary>
        /// The subscription's data is downloaded from a remote source
        /// </summary>
        RemoteFile,

        /// <summary>
        /// The subscription's data comes from a rest call that is polled and returns a single line/data point of information
        /// </summary>
        Rest,

        /// <summary>
        /// The subscription's data is streamed
        /// </summary>
        Streaming
    }

    /// <summary>
    /// 数据文件更新方式
    /// </summary>
    public enum WritePolicy
    {
        /// <summary>
        /// Will overwrite any existing file or zip entry with the new content
        /// </summary>
        Overwrite = 0,

        /// <summary>
        /// Will inject and merge new content with the existings file content
        /// </summary>
        Merge,

        /// <summary>
        /// Will append new data to the end of the file or zip entry
        /// </summary>
        Append
    }

    /// <summary>
    /// 指定数据在发送到算法之前如何规范化
    /// </summary>
    public enum DataNormalizationMode
    {
        /// <summary>
        /// The raw price with dividends added to cash book
        /// </summary>
        Raw,
        /// <summary>
        /// The adjusted prices with splits and dividends factored in
        /// </summary>
        Adjusted,
        /// <summary>
        /// The adjusted prices with only splits factored in, dividends paid out to the cash book
        /// </summary>
        SplitAdjusted,
        /// <summary>
        /// The split adjusted price plus dividends
        /// </summary>
        TotalReturn,
        /// <summary>
        /// Eliminates price jumps between two consecutive contracts, adding a factor based on the difference of their prices.
        /// </summary>
        /// <remarks>First contract is the true one, factor 0</remarks>
        ForwardPanamaCanal,
        /// <summary>
        /// Eliminates price jumps between two consecutive contracts, adding a factor based on the difference of their prices.
        /// </summary>
        /// <remarks>Last contract is the true one, factor 0</remarks>
        BackwardsPanamaCanal,
        /// <summary>
        /// Eliminates price jumps between two consecutive contracts, multiplying the prices by their ratio.
        /// </summary>
        /// <remarks>Last contract is the true one, factor 1</remarks>
        BackwardsRatio
    }

    /// <summary>
    /// 连续合约映射模式
    /// </summary>
    public enum DataMappingMode
    {
        /// <summary>
        /// The contract maps on the previous day of expiration of the front month.
        /// </summary>
        LastTradingDay,
        /// <summary>
        /// The contract maps on the first date of the delivery month of the front month. If the contract expires prior to this date,
        /// then it rolls on the contract's last trading date instead.
        /// </summary>
        /// <remarks>For example Crude Oil WTI (CL) 'DEC 2021 CLZ1' contract expires on Nov 19 2021, so mapping date will be it's expiration date</remarks>
        /// <remarks>Another example Corn 'DEC 2021 ZCZ1' contract expires on Dec 14 2021, so mapping date will be Dec 1st</remarks>
        FirstDayMonth,
        /// <summary>
        /// The contract maps when the back month contract has a higher volume that the current front month.
        /// </summary>
        OpenInterest
    }

}

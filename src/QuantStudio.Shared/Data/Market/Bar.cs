

using System.Runtime.CompilerServices;
using ProtoBuf;

namespace QuantStudio.Data.Market
{
    /// <summary>
    /// Base Bar Class: Open, High, Low, Close and Period.
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Bar : IBar
    {
        private bool _openSet;

        /// <summary>
        /// Opening price of the bar: Defined as the price at the start of the time period.
        /// </summary>
        [ProtoMember(1)]
        public decimal Open { get; set; }

        /// <summary>
        /// High price of the bar during the time period.
        /// </summary>
        [ProtoMember(2)]
        public decimal High { get; set; }

        /// <summary>
        /// Low price of the bar during the time period.
        /// </summary>
        [ProtoMember(3)]
        public decimal Low { get; set; }

        /// <summary>
        /// Closing price of the bar. Defined as the price at Start Time + TimeSpan.
        /// </summary>
        [ProtoMember(4)]
        public decimal Close { get; set; }

        /// <summary>
        /// Volume
        /// </summary>
        [ProtoMember(5)]
        public int Volume { get; set; }

        /// <summary>
        /// Turnover
        /// </summary>
        [ProtoMember(6)]
        public decimal Turnover { get; set; }

        /// <summary>
        /// Default initializer to setup an empty bar.
        /// </summary>
        public Bar()
        {
        }

        /// <summary>
        /// Initializer to setup a bar with a given information.
        /// </summary>
        /// <param name="open">Decimal Opening Price</param>
        /// <param name="high">Decimal High Price of this bar</param>
        /// <param name="low">Decimal Low Price of this bar</param>
        /// <param name="close">Decimal Close price of this bar</param>
        public Bar(decimal open, decimal high, decimal low, decimal close,int volume,decimal turnover)
        {
            _openSet = open != 0;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            Turnover = turnover;
        }

        /// <summary>
        /// Updates the bar with a new value. This will aggregate the OHLC bar
        /// </summary>
        /// <param name="value">The new value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(decimal value,int volume, decimal turnover)
        {
            Update(ref value,volume,turnover);
        }

        /// <summary>
        /// Updates the bar with a new value. This will aggregate the OHLC bar
        /// </summary>
        /// <param name="value">The new value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ref decimal value,int volume,decimal turnover)
        {
            // Do not accept zero as a new value
            if (value == 0) return;

            if (!_openSet)
            {
                Open = High = Low = Close = value;
                _openSet = true;
            }
            else if (value > High) High = value;
            else if (value < Low) Low = value;
            Close = value;
        }

        /// <summary>
        /// Returns a clone of this bar
        /// </summary>
        public Bar Clone()
        {
            return new Bar(Open, High, Low, Close,Volume,Turnover);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return $"O: {Open.SmartRounding()} " +
                   $"H: {High.SmartRounding()} " +
                   $"L: {Low.SmartRounding()} " +
                   $"C: {Close.SmartRounding()}" +
                   $"V: {Volume}" +
                   $"T: {Turnover.SmartRounding()}"
                   ;
        }
    }
}


using System;
using System.Linq;
using System.Collections.Generic;

namespace QuantStudio
{
    /// <summary>
    /// Markets Collection: Soon to be expanded to a collection of items specifying the market hour, timezones and country codes.
    /// </summary>
    public static class Market
    {
        // the upper bound (non-inclusive) for market identifiers
        private const int MaxMarketIdentifier = 1000;

        private static Dictionary<string, int> Markets = new Dictionary<string, int>();
        private static Dictionary<int, string> ReverseMarkets = new Dictionary<int, string>();
        private static readonly IEnumerable<Tuple<string, int>> HardcodedMarkets = new List<Tuple<string, int>>
        {
            Tuple.Create("empty", 0),
            Tuple.Create(SHFE, 1),
            Tuple.Create(DCE, 2),
            Tuple.Create(CZCE, 3),
            Tuple.Create(CFFEX, 4),
            Tuple.Create(INE, 5),
        };

        static Market()
        {
            // initialize our maps
            foreach (var market in HardcodedMarkets)
            {
                Markets[market.Item1] = market.Item2;
                ReverseMarkets[market.Item2] = market.Item1;
            }
        }

        /// <summary>
        /// SHFE MarketCode
        /// </summary>
        public const string SHFE = "SHFE";

        /// <summary>
        /// DCE MarketCode
        /// </summary>
        public const string DCE = "DCE";

        /// <summary>
        /// CZCE MarketCode Hours
        /// </summary>
        public const string CZCE = "CZCE";

        /// <summary>
        /// CFFEX MarketCode
        /// </summary>
        public const string CFFEX = "CFFEX";

        /// <summary>
        /// INE market
        /// </summary>
        public const string INE = "INE";

        /// <summary>
        /// Adds the specified market to the map of available markets with the specified identifier.
        /// </summary>
        /// <param name="market">The market string to add</param>
        /// <param name="identifier">The identifier for the market, this value must be positive and less than 1000</param>
        public static void Add(string market, int identifier)
        {
            if (identifier >= MaxMarketIdentifier)
            {
                throw new ArgumentOutOfRangeException(nameof(identifier),
                    $"The market identifier is limited to positive values less than {MaxMarketIdentifier.ToStringInvariant()}."
                );
            }

            market = market.ToLowerInvariant();

            int marketIdentifier;
            if (Markets.TryGetValue(market, out marketIdentifier) && identifier != marketIdentifier)
            {
                throw new ArgumentException(
                    $"Attempted to add an already added market with a different identifier. MarketCode: {market}"
                );
            }

            string existingMarket;
            if (ReverseMarkets.TryGetValue(identifier, out existingMarket))
            {
                throw new ArgumentException(
                    "Attempted to add a market identifier that is already in use. " +
                    $"New MarketCode: {market} Existing MarketCode: {existingMarket}"
                );
            }

            // update our maps.
            // We make a copy and update the copy, later swap the references so it's thread safe with no lock
            var newMarketDictionary = Markets.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            newMarketDictionary[market] = identifier;

            var newReverseMarketDictionary = ReverseMarkets.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            newReverseMarketDictionary[identifier] = market;

            Markets = newMarketDictionary;
            ReverseMarkets = newReverseMarketDictionary;
        }

        /// <summary>
        /// Gets the market code for the specified market. Returns <c>null</c> if the market is not found
        /// </summary>
        /// <param name="market">The market to check for (case sensitive)</param>
        /// <returns>The internal code used for the market. Corresponds to the value used when calling <see cref="Add"/></returns>
        public static int? Encode(string market)
        {
            return !Markets.TryGetValue(market, out var code) ? null : code;
        }

        /// <summary>
        /// Gets the market string for the specified market code.
        /// </summary>
        /// <param name="code">The market code to be decoded</param>
        /// <returns>The string representation of the market, or null if not found</returns>
        public static string Decode(int code)
        {
            return !ReverseMarkets.TryGetValue(code, out var market) ? null : market;
        }

        /// <summary>
        /// Returns a list of the supported markets
        /// </summary>
        public static List<string> SupportedMarkets()
        {
            return Markets.Keys.ToList();
        }
    }
}

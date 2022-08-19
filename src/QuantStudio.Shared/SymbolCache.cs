

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QuantStudio
{
    /// <summary>
    /// Provides a string->Symbol mapping to allow for user defined strings to be lifted into a Symbol
    /// This is mainly used via the Symbol implicit operator, but also functions that create securities
    /// should also call Set to add new mappings
    /// </summary>
    public static class SymbolCache
    {
        // we aggregate the two maps into a class so we can assign a new one as an atomic operation
        private static Cache _cache = new Cache();

        /// <summary>
        /// Adds a mapping for the specified ticker
        /// </summary>
        /// <param name="ticker">The string ticker symbol</param>
        /// <param name="symbol">The symbol object that maps to the string ticker symbol</param>
        public static void Set(string ticker, Symbol symbol)
        {
            _cache.Symbols[ticker] = symbol;
            _cache.Tickers[symbol] = ticker;
        }

        /// <summary>
        /// Gets the Symbol object that is mapped to the specified string ticker symbol
        /// </summary>
        /// <param name="ticker">The string ticker symbol</param>
        /// <returns>The symbol object that maps to the specified string ticker symbol</returns>
        public static Symbol GetSymbol(string ticker)
        {
            var result = TryGetSymbol(ticker);
            if (result.Item3 != null)
            {
                throw result.Item3;
            }

            return result.Item2;
        }

        /// <summary>
        /// Gets the Symbol object that is mapped to the specified string ticker symbol
        /// </summary>
        /// <param name="ticker">The string ticker symbol</param>
        /// <param name="symbol">The output symbol object</param>
        /// <returns>The symbol object that maps to the specified string ticker symbol</returns>
        public static bool TryGetSymbol(string ticker, out Symbol symbol)
        {
            var result = TryGetSymbol(ticker);
            // ignore errors
            if (result.Item1)
            {
                symbol = result.Item2;
                return true;
            }

            symbol = null;
            return result.Item1;
        }

        /// <summary>
        /// Gets the string ticker symbol that is mapped to the specified Symbol
        /// </summary>
        /// <param name="symbol">The symbol object</param>
        /// <returns>The string ticker symbol that maps to the specified symbol object</returns>
        public static string GetTicker(Symbol symbol)
        {
            string ticker;
            return _cache.Tickers.TryGetValue(symbol, out ticker) ? ticker : symbol.ID.ToString();
        }

        /// <summary>
        /// Gets the string ticker symbol that is mapped to the specified Symbol
        /// </summary>
        /// <param name="symbol">The symbol object</param>
        /// <param name="ticker">The output string ticker symbol</param>
        /// <returns>The string ticker symbol that maps to the specified symbol object</returns>
        public static bool TryGetTicker(Symbol symbol, out string ticker)
        {
            return _cache.Tickers.TryGetValue(symbol, out ticker);
        }

        /// <summary>
        /// Removes the mapping for the specified symbol from the cache
        /// </summary>
        /// <param name="symbol">The symbol whose mappings are to be removed</param>
        /// <returns>True if the symbol mapping were removed from the cache</returns>
        public static bool TryRemove(Symbol symbol)
        {
            string ticker;
            return _cache.Tickers.TryRemove(symbol, out ticker) && _cache.Symbols.TryRemove(ticker, out symbol);
        }

        /// <summary>
        /// Removes the mapping for the specified symbol from the cache
        /// </summary>
        /// <param name="ticker">The ticker whose mappings are to be removed</param>
        /// <returns>True if the symbol mapping were removed from the cache</returns>
        public static bool TryRemove(string ticker)
        {
            Symbol symbol;
            return _cache.Symbols.TryRemove(ticker, out symbol) && _cache.Tickers.TryRemove(symbol, out ticker);
        }

        /// <summary>
        /// Clears the current caches
        /// </summary>
        public static void Clear()
        {
            _cache = new Cache();
        }

        private static Tuple<bool, Symbol, InvalidOperationException> TryGetSymbol(string ticker)
        {
            Symbol symbol;
            InvalidOperationException error = null;
            if (!_cache.TryGetSymbol(ticker, out symbol))
            {
                // fall-back full-text search as a back-shim for custom data symbols.
                // permitting a user to use BTC to resolve to BTC.Bitcoin
                var search = $"{ticker.ToUpperInvariant()}.";
                var match = _cache.Symbols.Where(kvp => kvp.Key.StartsWith(search)).ToList();

                if (match.Count == 0)
                {
                    // no matches
                    error = new InvalidOperationException($"We were unable to locate the ticker '{ticker}'.");
                }
                else if (match.Count == 1)
                {
                    // exactly one match
                    symbol = match.Single().Value;
                }
                else if (match.Count > 1)
                {
                    // too many matches
                    error = new InvalidOperationException(
                        "We located multiple potentially matching tickers. For custom data, be sure to " +
                        "append a dot followed by the custom data type name. For example: 'BTC.Bitcoin'. " +
                        $"Potential Matches: {string.Join(", ", match.Select(kvp => kvp.Key))}"
                    );
                }
            }

            return Tuple.Create(symbol != null, symbol, error);
        }

        class Cache
        {
            public readonly ConcurrentDictionary<string, Symbol> Symbols = new ConcurrentDictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
            public readonly ConcurrentDictionary<Symbol, string> Tickers = new ConcurrentDictionary<Symbol, string>();

            /// <summary>
            /// Attempts to resolve the ticker to a Symbol via the cache. If not found in the
            /// cache then
            /// </summary>
            /// <param name="ticker">The ticker to resolver to a symbol</param>
            /// <param name="symbol">The resolves symbol</param>
            /// <returns>True if we successfully resolved a symbol, false otherwise</returns>
            public bool TryGetSymbol(string ticker, out Symbol symbol)
            {
                if (Symbols.TryGetValue(ticker, out symbol))
                {
                    return true;
                }
                SecurityIdentifier sid;
                if (SecurityIdentifier.TryParse(ticker, out sid))
                {
                    symbol = new Symbol(sid);
                    return true;
                }
                return false;
            }
        }
    }
}
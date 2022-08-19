/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using ProtoBuf;
using static QuantStudio.StringExtensions;

namespace QuantStudio
{
    /// <summary>
    /// Represents a unique security identifier. This is made of two components,
    /// the unique SID and the Value. The value is the current ticker symbol while
    /// the SID is constant over the life of a security
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public sealed class Symbol : IEquatable<Symbol>, IComparable
    {
        private Symbol _canonical;
        // for performance we register how we compare with empty
        private bool? _isEmpty;

        /// <summary>
        /// Provides a convenience method for creating a Symbol for most security types.
        /// This method currently does not support Commodities
        /// </summary>
        /// <param name="ticker">The string ticker symbol</param>
        /// <param name="market">The market the ticker resides in</param>
        /// <param name="alias">An alias to be used for the symbol cache. Required when
        /// adding the same security from different markets</param>
        /// <param name="baseDataType">Optional for <see cref="SecurityType.Base"/> and used for generating the base data SID</param>
        /// <returns>A new Symbol object for the specified ticker</returns>
        public static Symbol Create(string ticker,string market, DateTime date)
        {
            SecurityIdentifier sid;

            sid = SecurityIdentifier.GenerateFuture(market, ticker, date);

            return new Symbol(sid);
        }

        

        /// <summary>
        /// Provides a convenience method for creating a future Symbol.
        /// </summary>
        /// <param name="ticker">The ticker</param>
        /// <param name="market">The market the future resides in</param>
        /// <param name="expiry">The future expiry date</param>
        /// <param name="alias">An alias to be used for the symbol cache. Required when
        /// adding the same security from different markets</param>
        /// <returns>A new Symbol object for the specified future contract</returns>
        public static Symbol CreateFuture(string ticker, string market, DateTime expiry, string alias = null)
        {
            var sid = SecurityIdentifier.GenerateFuture(market ,ticker, expiry);

            return new Symbol(sid);
        }

        /// <summary>
        /// Method returns true, if symbol is a derivative canonical symbol
        /// </summary>
        /// <returns>true, if symbol is a derivative canonical symbol</returns>
        public bool IsCanonical()
        {
            return
                (ID.SecurityType == SecurityType.Future ||
                (ID.SecurityType.IsOption() && HasUnderlying)) &&
                ID.Date == SecurityIdentifier.DefaultDate;
        }

        /// <summary>
        /// Get's the canonical representation of this symbol
        /// </summary>
        /// <remarks>This is useful for access and performance</remarks>
        public Symbol Canonical
        {
            get
            {
                if (_canonical != null)
                {
                    return _canonical;
                }

                _canonical = this;
                if (!IsCanonical())
                {
                    if (SecurityType == SecurityType.Future)
                    {
                        _canonical = Create(ID.Symbol,ID.Market,ID.Date);
                    }
                    else
                    {
                        throw new InvalidOperationException("Canonical is only defined for SecurityType.Option, SecurityType.Future, SecurityType.FutureOption");
                    }
                }
                return _canonical;
            }
        }

        /// <summary>
        /// Determines if the specified <paramref name="symbol"/> is an underlying of this symbol instance
        /// </summary>
        /// <param name="symbol">The underlying to check for</param>
        /// <returns>True if the specified <paramref name="symbol"/> is an underlying of this symbol instance</returns>
        public bool HasUnderlyingSymbol(Symbol symbol)
        {
            var current = this;
            while (current.HasUnderlying)
            {
                if (current.Underlying == symbol)
                {
                    return true;
                }

                current = current.Underlying;
            }

            return false;
        }

        #region Properties

        /// <summary>
        /// Gets the current symbol for this ticker
        /// </summary>
        [ProtoMember(1)]
        public string Value { get; private set; }

        /// <summary>
        /// Gets the security identifier for this symbol
        /// </summary>
        [ProtoMember(2)]
        public SecurityIdentifier ID { get; private set; }

        /// <summary>
        /// Gets whether or not this <see cref="Symbol"/> is a derivative,
        /// that is, it has a valid <see cref="Underlying"/> property
        /// </summary>
        public bool HasUnderlying
        {
            get { return !ReferenceEquals(Underlying, null); }
        }

        /// <summary>
        /// Gets the security underlying symbol, if any
        /// </summary>
        [ProtoMember(3)]
        public Symbol Underlying { get; private set; }


        /// <summary>
        /// Gets the security type of the symbol
        /// </summary>
        public SecurityType SecurityType
        {
            get { return ID.SecurityType; }
        }


        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class
        /// </summary>
        /// <param name="sid">The security identifier for this symbol</param>
        /// <param name="value">The current ticker symbol value</param>
        public Symbol(SecurityIdentifier sid)
        {

            ID = sid;
        }

        /// <summary>
        /// Private constructor initializes a new instance of the <see cref="Symbol"/> class with underlying
        /// </summary>
        /// <param name="sid">The security identifier for this symbol</param>
        /// <param name="value">The current ticker symbol value</param>
        /// <param name="underlying">The underlying symbol</param>
        internal Symbol(SecurityIdentifier sid, string value, Symbol underlying)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            ID = sid;
            Value = value.LazyToUpper();
            Underlying = underlying;
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            // compare a sid just as you would a symbol object
            if (obj is SecurityIdentifier)
            {
                return ID.Equals((SecurityIdentifier) obj);
            }

            if (obj.GetType() != GetType()) return false;
            return Equals((Symbol)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            // only SID is used for comparisons
            unchecked { return ID.GetHashCode(); }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj"/> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj"/>. Greater than zero This instance follows <paramref name="obj"/> in the sort order.
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param><exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            var str = obj as string;
            if (str != null)
            {
                return string.Compare(Value, str, StringComparison.OrdinalIgnoreCase);
            }
            var sym = obj as Symbol;
            if (sym != null)
            {
                return string.Compare(Value, sym.Value, StringComparison.OrdinalIgnoreCase);
            }

            throw new ArgumentException("Object must be of type Symbol or string.");
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return SymbolCache.GetTicker(this);
        }

        #endregion

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Symbol other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (ReferenceEquals(other, null))
            {
                return _isEmpty.Value;
            }

            // only SID is used for comparisons
            return ID.Equals(other.ID);
        }

        #endregion

        #region Implicit operators

        /// <summary>
        /// Returns the symbol's string ticker
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <returns>The string ticker</returns>
        [Obsolete("Symbol implicit operator to string is provided for algorithm use only.")]
        public static implicit operator string(Symbol symbol)
        {
            return symbol.ToString();
        }

        #endregion

        #region String methods

        // in order to maintain better compile time backwards compatibility,
        // we'll redirect a few common string methods to Value, but mark obsolete
#pragma warning disable 1591
        [Obsolete("Symbol.Contains is a pass-through for Symbol.Value.Contains")]
        public bool Contains(string value) { return Value.Contains(value); }
        [Obsolete("Symbol.EndsWith is a pass-through for Symbol.Value.EndsWith")]
        public bool EndsWith(string value) { return Value.EndsWithInvariant(value); }
        [Obsolete("Symbol.StartsWith is a pass-through for Symbol.Value.StartsWith")]
        public bool StartsWith(string value) { return Value.StartsWithInvariant(value); }
        [Obsolete("Symbol.ToLower is a pass-through for Symbol.Value.ToLower")]
        public string ToLower() { return Value.ToLowerInvariant(); }
        [Obsolete("Symbol.ToUpper is a pass-through for Symbol.Value.ToUpper")]
        public string ToUpper() { return Value.LazyToUpper(); }
#pragma warning restore 1591

        #endregion
    }
}

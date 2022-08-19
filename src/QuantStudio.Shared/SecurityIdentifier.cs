
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ProtoBuf;
using ProtoBuf.WellKnownTypes;
using static QuantStudio.StringExtensions;

namespace QuantStudio
{
    /// <summary>
    /// Defines a unique identifier for securities
    /// </summary>
    /// <remarks>
    /// The SecurityIdentifier contains information about a specific security.
    /// This includes the symbol and other data specific to the SecurityType.
    /// The symbol is limited to 12 characters
    /// </remarks>
    [ProtoContract(SkipConstructor = true)]
    public class SecurityIdentifier : IEquatable<SecurityIdentifier>, IComparable<SecurityIdentifier>, IComparable
    {
        #region Empty, DefaultDate Fields

        private static readonly Dictionary<string, SecurityIdentifier> SecurityIdentifierCache = new();
        private static readonly char[] InvalidCharacters = {'|', ' '};

        /// <summary>
        /// Gets an instance of <see cref="SecurityIdentifier"/> that is empty, that is, one with no symbol specified
        /// </summary>
        public static readonly SecurityIdentifier Empty = new SecurityIdentifier(string.Empty,string.Empty,DefaultDate);

        /// <summary>
        /// Gets the date to be used when it does not apply.
        /// </summary>
        public static readonly DateTime DefaultDate = DateTime.FromOADate(0);

        /// <summary>
        /// Gets the set of invalids symbol characters
        /// </summary>
        public static readonly HashSet<char> InvalidSymbolCharacters = new HashSet<char>(InvalidCharacters);

        #endregion

        #region Member variables

        [ProtoMember(1)]
        private string _symbol;
        [ProtoMember(2)]
        private string _market;
        private SecurityIdentifier _underlying;
        private bool _hashCodeSet;
        private int _hashCode;
        private DateTime? _date;
        private string _stringRep;

        #endregion

        #region Properties


        /// <summary>
        /// Gets the date component of this identifier. For equities this
        /// is the first date the security traded. Technically speaking,
        /// in LEAN, this is the first date mentioned in the map_files.
        /// For futures and options this is the expiry date of the contract.
        /// For other asset classes, this property will throw an
        /// exception as the field is not specified.
        /// </summary>
        public DateTime Date
        {
            get
            {
                if (_date.HasValue)
                {
                    return _date.Value;
                }

                switch (SecurityType)
                {
                    case SecurityType.Base:
                    case SecurityType.Equity:
                    case SecurityType.Option:
                    case SecurityType.Future:
                    case SecurityType.Index:
                    case SecurityType.FutureOption:
                    case SecurityType.IndexOption:
                        return _date.Value;
                    default:
                        throw new InvalidOperationException("Date is only defined for SecurityType.Equity, SecurityType.Option, SecurityType.Future, SecurityType.FutureOption, SecurityType.IndexOption, and SecurityType.Base");
                }
            }
        }

        /// <summary>
        /// Gets the original symbol used to generate this security identifier.
        /// For equities, by convention this is the first ticker symbol for which
        /// the security traded
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
        }

        /// <summary>
        /// Gets the market component of this security identifier. If located in the
        /// internal mappings, the full string is returned. If the value is unknown,
        /// the integer value is returned as a string.
        /// </summary>
        public string Market
        {
            get
            {
                return _market;
            }
        }

        /// <summary>
        /// Gets the security type component of this security identifier.
        /// </summary>
        public SecurityType SecurityType { get; }


        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityIdentifier"/> class
        /// </summary>
        /// <param name="symbol">The base36 string encoded as a long using alpha [0-9A-Z]</param>
        /// <param name="properties">Other data defining properties of the symbol including market,
        /// security type, listing or expiry date, strike/call/put/style for options, ect...</param>
        public SecurityIdentifier(string symbol,string market, DateTime date)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol), "SecurityIdentifier requires a non-null string 'symbol'");
            }

            if (symbol.IndexOfAny(InvalidCharacters) != -1)
            {
                throw new ArgumentException("symbol must not contain the characters '|' or ' '.", nameof(symbol));
            }
            _symbol = symbol;
            _market = market;
            _date = date;

            if (!SecurityType.IsValid())
            {
                throw new ArgumentException($"The provided properties do not match with a valid {nameof(SecurityType)}", "properties");
            }
            _hashCode = unchecked(symbol.GetHashCode() * 397);
            _hashCodeSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityIdentifier"/> class
        /// </summary>
        /// <param name="symbol">The base36 string encoded as a long using alpha [0-9A-Z]</param>
        /// <param name="properties">Other data defining properties of the symbol including market,
        /// security type, listing or expiry date, strike/call/put/style for options, ect...</param>
        /// <param name="underlying">Specifies a <see cref="SecurityIdentifier"/> that represents the underlying security</param>
        public SecurityIdentifier(string symbol, string market ,DateTime date, SecurityIdentifier underlying)
            : this(symbol,market,date)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol), "SecurityIdentifier requires a non-null string 'symbol'");
            }

            _symbol = symbol;
            // performance: directly call Equals(SecurityIdentifier other), shortcuts Equals(object other)
            if (!underlying.Equals(Empty))
            {
                _underlying = underlying;
            }
        }

        #endregion

        #region Parsing routines

        /// <summary>
        /// Parses the specified string into a <see cref="SecurityIdentifier"/>
        /// The string must be a 40 digit number. The first 20 digits must be parseable
        /// to a 64 bit unsigned integer and contain ancillary data about the security.
        /// The second 20 digits must also be parseable as a 64 bit unsigned integer and
        /// contain the symbol encoded from base36, this provides for 12 alpha numeric case
        /// insensitive characters.
        /// </summary>
        /// <param name="value">The string value to be parsed</param>
        /// <returns>A new <see cref="SecurityIdentifier"/> instance if the <paramref name="value"/> is able to be parsed.</returns>
        /// <exception cref="FormatException">This exception is thrown if the string's length is not exactly 40 characters, or
        /// if the components are unable to be parsed as 64 bit unsigned integers</exception>
        public static SecurityIdentifier Parse(string value)
        {
            Exception exception;
            SecurityIdentifier identifier;
            if (!TryParse(value, out identifier, out exception))
            {
                throw exception;
            }

            return identifier;
        }

        /// <summary>
        /// Attempts to parse the specified <see paramref="value"/> as a <see cref="SecurityIdentifier"/>.
        /// </summary>
        /// <param name="value">The string value to be parsed</param>
        /// <param name="identifier">The result of parsing, when this function returns true, <paramref name="identifier"/>
        /// was properly created and reflects the input string, when this function returns false <paramref name="identifier"/>
        /// will equal default(SecurityIdentifier)</param>
        /// <returns>True on success, otherwise false</returns>
        public static bool TryParse(string value, out SecurityIdentifier identifier)
        {
            Exception exception;
            return TryParse(value, out identifier, out exception);
        }

        /// <summary>
        /// Helper method impl to be used by parse and tryparse
        /// </summary>
        private static bool TryParse(string value, out SecurityIdentifier identifier, out Exception exception)
        {
            if (!TryParseProperties(value, out exception, out identifier))
            {
                return false;
            }

            return true;
        }

        private static readonly char[] SplitSpace = { '.'};

        /// <summary>
        /// Parses the string into its component ulong pieces
        /// </summary>
        private static bool TryParseProperties(string value, out Exception exception, out SecurityIdentifier identifier)
        {
            exception = null;

            if (value == null)
            {
                identifier = Empty;
                return true;
            }

            lock (SecurityIdentifierCache)
            {
                // for performance, we first verify if we already have parsed this SecurityIdentifier
                if (SecurityIdentifierCache.TryGetValue(value, out identifier))
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(value) || value == " 0")
                {
                    // we know it's not null already let's cache it
                    SecurityIdentifierCache[value] = identifier = Empty;
                    return true;
                }

                // after calling TryGetValue because if it failed it will set identifier to default
                identifier = Empty;

                try
                {
                    var sids = value.Split('|');
                    for (var i = sids.Length - 1; i > -1; i--)
                    {
                        var current = sids[i];
                        var parts = current.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2)
                        {
                            exception = new FormatException("The string must be splittable on space into two parts.");
                            return false;
                        }

                        string symbol = parts[0];
                        string market = parts[1];
                        
                        // toss the previous in as the underlying, if Empty, ignored by ctor
                        identifier = new SecurityIdentifier(symbol, market, DefaultDate, identifier);
                    }
                }
                catch (Exception error)
                {
                    exception = error;
                    // Log.Error($"SecurityIdentifier.TryParseProperties(): Error parsing SecurityIdentifier: '{value}', Exception: {exception}");
                    return false;
                }

                SecurityIdentifierCache[value] = identifier;
                return true;
            }
        }

        #endregion

        #region AddMarket, GetMarketCode, and Generate

        /// <summary>
        /// Generates a new <see cref="SecurityIdentifier"/> for a future
        /// </summary>
        /// <param name="expiry">The date the future expires</param>
        /// <param name="symbol">The security's symbol</param>
        /// <param name="market">The market</param>
        /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified futures security</returns>
        public static SecurityIdentifier GenerateFuture(string market, string symbol, DateTime date)
        {
            return Generate(market ,symbol, date);
        }


        /// <summary>
        /// Generic generate method. This method should be used carefully as some parameters are not required and
        /// some parameters mean different things for different security types
        /// </summary>
        private static SecurityIdentifier Generate(
            string symbol,
            string market,
            DateTime date)
        {

            // normalize input strings
            var result = new SecurityIdentifier(symbol,market, date);

            return result;
        }

        #endregion

        #region Equality members and ToString

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(SecurityIdentifier other)
        {
            return ReferenceEquals(this, other)
                && _symbol == other._symbol
                && _underlying == other._underlying;
        }

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
            if (obj.GetType() != GetType()) return false;
            return Equals((SecurityIdentifier)obj);
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
            if (!_hashCodeSet)
            {
                _hashCode = unchecked(_symbol.GetHashCode() * 397);
                _hashCodeSet = true;
            }
            return _hashCode;
        }

        /// <summary>
        /// Override equals operator
        /// </summary>
        public static bool operator ==(SecurityIdentifier left, SecurityIdentifier right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Override not equals operator
        /// </summary>
        public static bool operator !=(SecurityIdentifier left, SecurityIdentifier right)
        {
            return !Equals(left, right);
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
            if (_stringRep == null)
            {
                _stringRep = $"{_symbol}.{_market}";
            }

            return _stringRep;
        }

        public int CompareTo(SecurityIdentifier? other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

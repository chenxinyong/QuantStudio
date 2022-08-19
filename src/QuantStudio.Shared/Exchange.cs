

using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuantStudio
{
    /// <summary>
    /// Lean exchange definition
    /// </summary>
    public class Exchange
    {
        private static Exchange _shfe = new Exchange("SHFE", "SHFE", "SHFE" ,SecurityType.Future, SecurityType.FutureOption);

        /// <summary>
        /// SHFE
        /// </summary>
        public static Exchange SHFE { get { return _shfe; } }

        private static Exchange _dce = new Exchange("DCE", "DCE", "DCE", SecurityType.Future, SecurityType.FutureOption);

        /// <summary>
        /// DCE
        /// </summary>
        public static Exchange DCE { get { return _dce; } }

        private static Exchange _czce = new Exchange("CZCE", "CZCE", "CZCE", SecurityType.Future, SecurityType.FutureOption);

        /// <summary>
        /// CZCE
        /// </summary>
        public static Exchange CZCE { get { return _czce; } }

        /// <summary>
        /// CFFEX
        /// </summary>
        public static Exchange CFFEX { get { return new Exchange("CFFEX", "CFFEX", "CFFEX", SecurityType.Future, SecurityType.FutureOption); } }


        /// <summary>
        /// Exchange description
        /// </summary>
        [JsonIgnore]
        public string Description { get; }

        /// <summary>
        /// The exchange short code
        /// </summary>
        public string Code { get; init; }

        /// <summary>
        /// The exchange name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The associated lean marketCode <see cref="MarketCode"/>
        /// </summary>
        public string MarketCode { get; init; }

        public bool IsShortYearMonth { get; set; }

        /// <summary>
        /// Security types traded in this exchange
        /// </summary>
        public IReadOnlyList<SecurityType> SecurityTypes { get; init; } = new List<SecurityType>();

        /// <summary>
        /// Creates a new empty exchange instance
        /// </summary>
        /// <remarks>For json round trip serialization</remarks>
        private Exchange()
        {
        }

        /// <summary>
        /// Creates a new exchange instance
        /// </summary>
        private Exchange(string name, string code, string description, params SecurityType[] securityTypes)
        {
            Name = name;
            Description = description;
            SecurityTypes = securityTypes?.ToList() ?? new List<SecurityType>();
            Code = string.IsNullOrEmpty(code) ? name : code;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns the string representation of this exchange
        /// </summary>
        public static implicit operator string(Exchange exchange)
        {
            return ReferenceEquals(exchange, null) ? string.Empty : exchange.ToString();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var exchange = obj as Exchange;
            return Code == exchange.Code
                && MarketCode == exchange.MarketCode
                && SecurityTypes.All(exchange.SecurityTypes.Contains)
                && SecurityTypes.Count == exchange.SecurityTypes.Count;
        }

        /// <summary>
        /// Equals operator
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>True if both symbols are equal, otherwise false</returns>
        public static bool operator ==(Exchange left, Exchange right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Not equals operator
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>True if both symbols are not equal, otherwise false</returns>
        public static bool operator !=(Exchange left, Exchange right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Code.GetHashCode();
                hashCode = (hashCode * 397) ^ MarketCode.GetHashCode();
                for (var i = 0; i < SecurityTypes.Count; i++)
                {
                    hashCode = (hashCode * 397) ^ SecurityTypes[i].GetHashCode();
                }
                return hashCode;
            }
        }
    }
}


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using static QuantStudio.StringExtensions;
using Microsoft.IO;

namespace QuantStudio
{
    /// <summary>
    /// Extensions function collections - group all static extensions functions here.
    /// </summary>
    public static class Extensions
    {
        private static readonly HashSet<string> InvalidSecurityTypes = new HashSet<string>();
        private static readonly Regex DateCheck = new Regex(@"\d{8}", RegexOptions.Compiled);
        private static RecyclableMemoryStreamManager MemoryManager = new RecyclableMemoryStreamManager();

        /// <summary>
        /// The offset span from the market close to liquidate or exercise a security on the delisting date
        /// </summary>
        /// <remarks>Will no be used in live trading</remarks>
        /// <remarks>By default span is negative 15 minutes. We want to liquidate before market closes if not, in some cases
        /// like future options the market close would match the delisted event time and would cancel all orders and mark the security
        /// as non tradable and delisted.</remarks>
        public static TimeSpan DelistingMarketCloseOffsetSpan { get; set; } = TimeSpan.FromMinutes(-15);


        /// <summary>
        /// Tries to fetch the custom data type associated with a symbol
        /// </summary>
        /// <remarks>Custom data type <see cref="SecurityIdentifier"/> symbol value holds their data type</remarks>
        public static bool TryGetCustomDataType(this string symbol, out string type)
        {
            type = null;
            if (symbol != null)
            {
                var index = symbol.LastIndexOf('.');
                if (index != -1 && symbol.Length > index + 1)
                {
                    type = symbol.Substring(index + 1);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Safe multiplies a decimal by 100
        /// </summary>
        /// <param name="value">The decimal to multiply</param>
        /// <returns>The result, maxed out at decimal.MaxValue</returns>
        public static decimal SafeMultiply100(this decimal value)
        {
            const decimal max = decimal.MaxValue / 100m;
            if (value >= max) return decimal.MaxValue;
            return value * 100m;
        }

        /// <summary>
        /// Will return a memory stream using the <see cref="RecyclableMemoryStreamManager"/> instance.
        /// </summary>
        /// <param name="guid">Unique guid</param>
        /// <returns>A memory stream</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryStream GetMemoryStream(Guid guid)
        {
            return MemoryManager.GetStream(guid);
        }


        /// <summary>
        /// Converts the provided string into camel case notation
        /// </summary>
        public static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length == 1)
            {
                return value.ToLowerInvariant();
            }
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// Extension to move one element from list from A to position B.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List we're operating on.</param>
        /// <param name="oldIndex">Index of variable we want to move.</param>
        /// <param name="newIndex">New location for the variable</param>
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            var oItem = list[oldIndex];
            list.RemoveAt(oldIndex);
            if (newIndex > oldIndex) newIndex--;
            list.Insert(newIndex, oItem);
        }

        /// <summary>
        /// Extension method to convert a string into a byte array
        /// </summary>
        /// <param name="str">String to convert to bytes.</param>
        /// <returns>Byte array</returns>
        public static byte[] GetBytes(this string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Extentsion method to clear all items from a thread safe queue
        /// </summary>
        /// <remarks>Small risk of race condition if a producer is adding to the list.</remarks>
        /// <typeparam name="T">Queue type</typeparam>
        /// <param name="queue">queue object</param>
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item)) {
                // NOP
            }
        }

        /// <summary>
        /// Extension method to convert a byte array into a string.
        /// </summary>
        /// <param name="bytes">Byte array to convert.</param>
        /// <param name="encoding">The encoding to use for the conversion. Defaults to Encoding.ASCII</param>
        /// <returns>String from bytes.</returns>
        public static string GetString(this byte[] bytes, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.ASCII;

            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Extension method to convert a string to a MD5 hash.
        /// </summary>
        /// <param name="str">String we want to MD5 encode.</param>
        /// <returns>MD5 hash of a string</returns>
        public static string ToMD5(this string str)
        {
            var builder = new StringBuilder();
            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
                foreach (var t in data) builder.Append(t.ToStringInvariant("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Encrypt the token:time data to make our API hash.
        /// </summary>
        /// <param name="data">Data to be hashed by SHA256</param>
        /// <returns>Hashed string.</returns>
        public static string ToSHA256(this string data)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(data), 0, Encoding.UTF8.GetByteCount(data));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToStringInvariant("x2"));
            }
            return hash.ToString();
        }

        /// <summary>
        /// Converts a long to an uppercase alpha numeric string
        /// </summary>
        public static string EncodeBase36(this ulong data)
        {
            var stack = new Stack<char>(15);
            while (data != 0)
            {
                var value = data % 36;
                var c = value < 10
                    ? (char)(value + '0')
                    : (char)(value - 10 + 'A');

                stack.Push(c);
                data /= 36;
            }
            return new string(stack.ToArray());
        }

        /// <summary>
        /// Converts an upper case alpha numeric string into a long
        /// </summary>
        public static ulong DecodeBase36(this string symbol)
        {
            var result = 0ul;
            var baseValue = 1ul;
            for (var i = symbol.Length - 1; i > -1; i--)
            {
                var c = symbol[i];

                // assumes alpha numeric upper case only strings
                var value = (uint)(c <= 57
                    ? c - '0'
                    : c - 'A' + 10);

                result += baseValue * value;
                baseValue *= 36;
            }

            return result;
        }

        /// <summary>
        /// Convert a string to Base64 Encoding
        /// </summary>
        /// <param name="text">Text to encode</param>
        /// <returns>Encoded result</returns>
        public static string EncodeBase64(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(textBytes);
        }

        /// <summary>
        /// Decode a Base64 Encoded string
        /// </summary>
        /// <param name="base64EncodedText">Text to decode</param>
        /// <returns>Decoded result</returns>
        public static string DecodeBase64(this string base64EncodedText)
        {
            if (string.IsNullOrEmpty(base64EncodedText))
            {
                return base64EncodedText;
            }

            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedText);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Lazy string to upper implementation.
        /// Will first verify the string is not already upper and avoid
        /// the call to <see cref="string.ToUpperInvariant()"/> if possible.
        /// </summary>
        /// <param name="data">The string to upper</param>
        /// <returns>The upper string</returns>
        public static string LazyToUpper(this string data)
        {
            // for performance only call to upper if required
            var alreadyUpper = true;
            for (int i = 0; i < data.Length && alreadyUpper; i++)
            {
                alreadyUpper = char.IsUpper(data[i]);
            }
            return alreadyUpper ? data : data.ToUpperInvariant();
        }

        /// <summary>
        /// Lazy string to lower implementation.
        /// Will first verify the string is not already lower and avoid
        /// the call to <see cref="string.ToLowerInvariant()"/> if possible.
        /// </summary>
        /// <param name="data">The string to lower</param>
        /// <returns>The lower string</returns>
        public static string LazyToLower(this string data)
        {
            // for performance only call to lower if required
            var alreadyLower = true;
            for (int i = 0; i < data.Length && alreadyLower; i++)
            {
                alreadyLower = char.IsLower(data[i]);
            }
            return alreadyLower ? data : data.ToLowerInvariant();
        }

        /// <summary>
        /// Extension method to automatically set the update value to same as "add" value for TryAddUpdate.
        /// This makes the API similar for traditional and concurrent dictionaries.
        /// </summary>
        /// <typeparam name="K">Key type for dictionary</typeparam>
        /// <typeparam name="V">Value type for dictonary</typeparam>
        /// <param name="dictionary">Dictionary object we're operating on</param>
        /// <param name="key">Key we want to add or update.</param>
        /// <param name="value">Value we want to set.</param>
        public static void AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }

        /// <summary>
        /// Extension method to automatically add/update lazy values in concurrent dictionary.
        /// </summary>
        /// <typeparam name="TKey">Key type for dictionary</typeparam>
        /// <typeparam name="TValue">Value type for dictonary</typeparam>
        /// <param name="dictionary">Dictionary object we're operating on</param>
        /// <param name="key">Key we want to add or update.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, Lazy<TValue>> dictionary, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            var result = dictionary.AddOrUpdate(key, new Lazy<TValue>(() => addValueFactory(key)), (key2, old) => new Lazy<TValue>(() => updateValueFactory(key2, old.Value)));
            return result.Value;
        }

        /// <summary>
        /// Adds the specified element to the collection with the specified key. If an entry does not exist for the
        /// specified key then one will be created.
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TElement">The collection element type</typeparam>
        /// <typeparam name="TCollection">The collection type</typeparam>
        /// <param name="dictionary">The source dictionary to be added to</param>
        /// <param name="key">The key</param>
        /// <param name="element">The element to be added</param>
        public static void Add<TKey, TElement, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TElement element)
            where TCollection : ICollection<TElement>, new()
        {
            TCollection list;
            if (!dictionary.TryGetValue(key, out list))
            {
                list = new TCollection();
                dictionary.Add(key, list);
            }
            list.Add(element);
        }

        /// <summary>
        /// Adds the specified element to the collection with the specified key. If an entry does not exist for the
        /// specified key then one will be created.
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TElement">The collection element type</typeparam>
        /// <param name="dictionary">The source dictionary to be added to</param>
        /// <param name="key">The key</param>
        /// <param name="element">The element to be added</param>
        public static ImmutableDictionary<TKey, ImmutableHashSet<TElement>> Add<TKey, TElement>(
            this ImmutableDictionary<TKey, ImmutableHashSet<TElement>> dictionary,
            TKey key,
            TElement element
            )
        {
            ImmutableHashSet<TElement> set;
            if (!dictionary.TryGetValue(key, out set))
            {
                set = ImmutableHashSet<TElement>.Empty.Add(element);
                return dictionary.Add(key, set);
            }

            return dictionary.SetItem(key, set.Add(element));
        }

        /// <summary>
        /// Adds the specified element to the collection with the specified key. If an entry does not exist for the
        /// specified key then one will be created.
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TElement">The collection element type</typeparam>
        /// <param name="dictionary">The source dictionary to be added to</param>
        /// <param name="key">The key</param>
        /// <param name="element">The element to be added</param>
        public static ImmutableSortedDictionary<TKey, ImmutableHashSet<TElement>> Add<TKey, TElement>(
            this ImmutableSortedDictionary<TKey, ImmutableHashSet<TElement>> dictionary,
            TKey key,
            TElement element
            )
        {
            ImmutableHashSet<TElement> set;
            if (!dictionary.TryGetValue(key, out set))
            {
                set = ImmutableHashSet<TElement>.Empty.Add(element);
                return dictionary.Add(key, set);
            }

            return dictionary.SetItem(key, set.Add(element));
        }

        /// <summary>
        /// Removes the specified element to the collection with the specified key. If the entry's count drops to
        /// zero, then the entry will be removed.
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TElement">The collection element type</typeparam>
        /// <param name="dictionary">The source dictionary to be added to</param>
        /// <param name="key">The key</param>
        /// <param name="element">The element to be added</param>
        public static ImmutableDictionary<TKey, ImmutableHashSet<TElement>> Remove<TKey, TElement>(
            this ImmutableDictionary<TKey, ImmutableHashSet<TElement>> dictionary,
            TKey key,
            TElement element
            )
        {
            ImmutableHashSet<TElement> set;
            if (!dictionary.TryGetValue(key, out set))
            {
                return dictionary;
            }

            set = set.Remove(element);
            if (set.Count == 0)
            {
                return dictionary.Remove(key);
            }

            return dictionary.SetItem(key, set);
        }

        /// <summary>
        /// Removes the specified element to the collection with the specified key. If the entry's count drops to
        /// zero, then the entry will be removed.
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TElement">The collection element type</typeparam>
        /// <param name="dictionary">The source dictionary to be added to</param>
        /// <param name="key">The key</param>
        /// <param name="element">The element to be added</param>
        public static ImmutableSortedDictionary<TKey, ImmutableHashSet<TElement>> Remove<TKey, TElement>(
            this ImmutableSortedDictionary<TKey, ImmutableHashSet<TElement>> dictionary,
            TKey key,
            TElement element
            )
        {
            ImmutableHashSet<TElement> set;
            if (!dictionary.TryGetValue(key, out set))
            {
                return dictionary;
            }

            set = set.Remove(element);
            if (set.Count == 0)
            {
                return dictionary.Remove(key);
            }

            return dictionary.SetItem(key, set);
        }

        /// <summary>
        /// Extension method to round a double value to a fixed number of significant figures instead of a fixed decimal places.
        /// </summary>
        /// <param name="d">Double we're rounding</param>
        /// <param name="digits">Number of significant figures</param>
        /// <returns>New double rounded to digits-significant figures</returns>
        public static decimal RoundToSignificantDigits(this decimal d, int digits)
        {
            if (d == 0) return 0;
            var scale = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double) Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }

        /// <summary>
        /// Converts a decimal into a rounded number ending with K (thousands), M (millions), B (billions), etc.
        /// </summary>
        /// <param name="number">Number to convert</param>
        /// <returns>Formatted number with figures written in shorthand form</returns>
        public static string ToFinancialFigures(this decimal number)
        {
            if (number < 1000)
            {
                return number.ToStringInvariant();
            }

            // Subtract by multiples of 5 to round down to nearest round number
            if (number < 10000)
            {
                return $"{number - 5m:#,.##}K";
            }

            if (number < 100000)
            {
                return $"{number - 50m:#,.#}K";
            }

            if (number < 1000000)
            {
                return $"{number - 500m:#,.}K";
            }

            if (number < 10000000)
            {
                return $"{number - 5000m:#,,.##}M";
            }

            if (number < 100000000)
            {
                return $"{number - 50000m:#,,.#}M";
            }

            if (number < 1000000000)
            {
                return $"{number - 500000m:#,,.}M";
            }

            return $"{number - 5000000m:#,,,.##}B";
        }

        /// <summary>
        /// Discretizes the <paramref name="value"/> to a maximum precision specified by <paramref name="quanta"/>. Quanta
        /// can be an arbitrary positive number and represents the step size. Consider a quanta equal to 0.15 and rounding
        /// a value of 1.0. Valid values would be 0.9 (6 quanta) and 1.05 (7 quanta) which would be rounded up to 1.05.
        /// </summary>
        /// <param name="value">The value to be rounded by discretization</param>
        /// <param name="quanta">The maximum precision allowed by the value</param>
        /// <param name="mode">Specifies how to handle the rounding of half value, defaulting to away from zero.</param>
        /// <returns></returns>
        public static decimal DiscretelyRoundBy(this decimal value, decimal quanta, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            if (quanta == 0m)
            {
                return value;
            }

            // away from zero is the 'common sense' rounding.
            // +0.5 rounded by 1 yields +1
            // -0.5 rounded by 1 yields -1
            var multiplicand = Math.Round(value / quanta, mode);
            return quanta * multiplicand;
        }

        /// <summary>
        /// Will truncate the provided decimal, without rounding, to 3 decimal places
        /// </summary>
        /// <param name="value">The value to truncate</param>
        /// <returns>New instance with just 3 decimal places</returns>
        public static decimal TruncateTo3DecimalPlaces(this decimal value)
        {
            // we will multiply by 1k bellow, if its bigger it will stack overflow
            if (value >= decimal.MaxValue / 1000
                || value <= decimal.MinValue / 1000
                || value == 0)
            {
                return value;
            }

            return Math.Truncate(1000 * value) / 1000;
        }

        /// <summary>
        /// Provides global smart rounding, numbers larger than 1000 will round to 4 decimal places,
        /// while numbers smaller will round to 7 significant digits
        /// </summary>
        public static decimal SmartRounding(this decimal input)
        {
            input = Normalize(input);

            // any larger numbers we still want some decimal places
            if (input > 1000)
            {
                return Math.Round(input, 4);
            }

            // this is good for forex and other small numbers
            return input.RoundToSignificantDigits(7).Normalize();
        }

        /// <summary>
        /// Casts the specified input value to a decimal while acknowledging the overflow conditions
        /// </summary>
        /// <param name="input">The value to be cast</param>
        /// <returns>The input value as a decimal, if the value is too large or to small to be represented
        /// as a decimal, then the closest decimal value will be returned</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal SafeDecimalCast(this double input)
        {
            if (input.IsNaNOrInfinity())
            {
                throw new ArgumentException(
                    $"It is not possible to cast a non-finite floating-point value ({input}) as decimal. Please review math operations and verify the result is valid.",
                    nameof(input),
                    new NotFiniteNumberException(input)
                );
            }

            if (input <= (double) decimal.MinValue) return decimal.Zero;
            if (input >= (double) decimal.MaxValue) return decimal.Zero;
            return (decimal) input;
        }

        /// <summary>
        /// Will remove any trailing zeros for the provided decimal input
        /// </summary>
        /// <param name="input">The <see cref="decimal"/> to remove trailing zeros from</param>
        /// <returns>Provided input with no trailing zeros</returns>
        /// <remarks>Will not have the expected behavior when called from Python,
        /// since the returned <see cref="decimal"/> will be converted to python float,
        /// <see cref="NormalizeToStr"/></remarks>
        public static decimal Normalize(this decimal input)
        {
            // http://stackoverflow.com/a/7983330/1582922
            return input / 1.000000000000000000000000000000000m;
        }

        /// <summary>
        /// Will remove any trailing zeros for the provided decimal and convert to string.
        /// Uses <see cref="Normalize(decimal)"/>.
        /// </summary>
        /// <param name="input">The <see cref="decimal"/> to convert to <see cref="string"/></param>
        /// <returns>Input converted to <see cref="string"/> with no trailing zeros</returns>
        public static string NormalizeToStr(this decimal input)
        {
            return Normalize(input).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Extension method for faster string to decimal conversion.
        /// </summary>
        /// <param name="str">String to be converted to positive decimal value</param>
        /// <remarks>
        /// Leading and trailing whitespace chars are ignored
        /// </remarks>
        /// <returns>Decimal value of the string</returns>
        public static decimal ToDecimal(this string str)
        {
            long value = 0;
            var decimalPlaces = 0;
            var hasDecimals = false;
            var index = 0;
            var length = str.Length;

            while (index < length && char.IsWhiteSpace(str[index]))
            {
                index++;
            }

            var isNegative = index < length && str[index] == '-';
            if (isNegative)
            {
                index++;
            }

            while (index < length)
            {
                var ch = str[index++];
                if (ch == '.')
                {
                    hasDecimals = true;
                    decimalPlaces = 0;
                }
                else if (char.IsWhiteSpace(ch))
                {
                    break;
                }
                else
                {
                    value = value * 10 + (ch - '0');
                    decimalPlaces++;
                }
            }

            var lo = (int)value;
            var mid = (int)(value >> 32);
            return new decimal(lo, mid, 0, isNegative, (byte)(hasDecimals ? decimalPlaces : 0));
        }

        /// <summary>
        /// Extension method for faster string to normalized decimal conversion, i.e. 20.0% should be parsed into 0.2
        /// </summary>
        /// <param name="str">String to be converted to positive decimal value</param>
        /// <remarks>
        /// Leading and trailing whitespace chars are ignored
        /// </remarks>
        /// <returns>Decimal value of the string</returns>
        public static decimal ToNormalizedDecimal(this string str)
        {
            var trimmed = str.Trim();
            var value = str.TrimEnd('%').ToDecimal();
            if (trimmed.EndsWith("%"))
            {
                value /= 100;
            }

            return value;
        }

        /// <summary>
        /// Extension method for string to decimal conversion where string can represent a number with exponent xe-y
        /// </summary>
        /// <param name="str">String to be converted to decimal value</param>
        /// <returns>Decimal value of the string</returns>
        public static decimal ToDecimalAllowExponent(this string str)
        {
            return decimal.Parse(str, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Extension method for faster string to Int32 conversion.
        /// </summary>
        /// <param name="str">String to be converted to positive Int32 value</param>
        /// <remarks>Method makes some assuptions - always numbers, no "signs" +,- etc.</remarks>
        /// <returns>Int32 value of the string</returns>
        public static int ToInt32(this string str)
        {
            int value = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == '.')
                    break;

                value = value * 10 + (str[i] - '0');
            }
            return value;
        }

        /// <summary>
        /// Extension method for faster string to Int64 conversion.
        /// </summary>
        /// <param name="str">String to be converted to positive Int64 value</param>
        /// <remarks>Method makes some assuptions - always numbers, no "signs" +,- etc.</remarks>
        /// <returns>Int32 value of the string</returns>
        public static long ToInt64(this string str)
        {
            long value = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == '.')
                    break;

                value = value * 10 + (str[i] - '0');
            }
            return value;
        }

        /// <summary>
        /// Breaks the specified string into csv components, all commas are considered separators
        /// </summary>
        /// <param name="str">The string to be broken into csv</param>
        /// <param name="size">The expected size of the output list</param>
        /// <returns>A list of the csv pieces</returns>
        public static List<string> ToCsv(this string str, int size = 4)
        {
            int last = 0;
            var csv = new List<string>(size);
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ',')
                {
                    if (last != 0) last = last + 1;
                    csv.Add(str.Substring(last, i - last));
                    last = i;
                }
            }
            if (last != 0) last = last + 1;
            csv.Add(str.Substring(last));
            return csv;
        }

        /// <summary>
        /// Breaks the specified string into csv components, works correctly with commas in data fields
        /// </summary>
        /// <param name="str">The string to be broken into csv</param>
        /// <param name="size">The expected size of the output list</param>
        /// <param name="delimiter">The delimiter used to separate entries in the line</param>
        /// <returns>A list of the csv pieces</returns>
        public static List<string> ToCsvData(this string str, int size = 4, char delimiter = ',')
        {
            var csv = new List<string>(size);

            var last = -1;
            var count = 0;
            var textDataField = false;

            for (var i = 0; i < str.Length; i++)
            {
                var current = str[i];
                if (current == '"')
                {
                    textDataField = !textDataField;
                }
                else if (!textDataField && current == delimiter)
                {
                    csv.Add(str.Substring(last + 1, (i - last)).Trim(' ', ','));
                    last = i;
                    count++;
                }
            }

            if (last != 0)
            {
                csv.Add(str.Substring(last + 1).Trim());
            }

            return csv;
        }

        /// <summary>
        /// Check if a number is NaN or infinity
        /// </summary>
        /// <param name="value">The double value to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNaNOrInfinity(this double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value);
        }

        /// <summary>
        /// Check if a number is NaN or equal to zero
        /// </summary>
        /// <param name="value">The double value to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNaNOrZero(this double value)
        {
            return double.IsNaN(value) || Math.Abs(value) < double.Epsilon;
        }

        /// <summary>
        /// Gets the smallest positive number that can be added to a decimal instance and return
        /// a new value that does not == the old value
        /// </summary>
        public static decimal GetDecimalEpsilon()
        {
            return new decimal(1, 0, 0, false, 27); //1e-27m;
        }

        /// <summary>
        /// Extension method to extract the extension part of this file name if it matches a safe list, or return a ".custom" extension for ones which do not match.
        /// </summary>
        /// <param name="str">String we're looking for the extension for.</param>
        /// <returns>Last 4 character string of string.</returns>
        public static string GetExtension(this string str) {
            var ext = str.Substring(Math.Max(0, str.Length - 4));
            var allowedExt = new List<string> { ".zip", ".csv", ".json", ".tsv" };
            if (!allowedExt.Contains(ext))
            {
                ext = ".custom";
            }
            return ext;
        }

        /// <summary>
        /// Extension method to convert strings to stream to be read.
        /// </summary>
        /// <param name="str">String to convert to stream</param>
        /// <returns>Stream instance</returns>
        public static Stream ToStream(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Extension method to round a timeSpan to nearest timespan period.
        /// </summary>
        /// <param name="time">TimeSpan To Round</param>
        /// <param name="roundingInterval">Rounding Unit</param>
        /// <param name="roundingType">Rounding method</param>
        /// <returns>Rounded timespan</returns>
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval, MidpointRounding roundingType)
        {
            if (roundingInterval == TimeSpan.Zero)
            {
                // divide by zero exception
                return time;
            }

            return new TimeSpan(
                Convert.ToInt64(Math.Round(
                    time.Ticks / (decimal)roundingInterval.Ticks,
                    roundingType
                )) * roundingInterval.Ticks
            );
        }


        /// <summary>
        /// Extension method to round timespan to nearest timespan period.
        /// </summary>
        /// <param name="time">Base timespan we're looking to round.</param>
        /// <param name="roundingInterval">Timespan period we're rounding.</param>
        /// <returns>Rounded timespan period</returns>
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval)
        {
            return Round(time, roundingInterval, MidpointRounding.ToEven);
        }

        /// <summary>
        /// Extension method to round a datetime down by a timespan interval.
        /// </summary>
        /// <param name="dateTime">Base DateTime object we're rounding down.</param>
        /// <param name="interval">Timespan interval to round to</param>
        /// <returns>Rounded datetime</returns>
        /// <remarks>Using this with timespans greater than 1 day may have unintended
        /// consequences. Be aware that rounding occurs against ALL time, so when using
        /// timespan such as 30 days we will see 30 day increments but it will be based
        /// on 30 day increments from the beginning of time.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime RoundDown(this DateTime dateTime, TimeSpan interval)
        {
            if (interval == TimeSpan.Zero)
            {
                // divide by zero exception
                return dateTime;
            }

            var amount = dateTime.Ticks % interval.Ticks;
            if (amount > 0)
            {
                return dateTime.AddTicks(-amount);
            }
            return dateTime;
        }

        /// <summary>
        /// Extension method to round a datetime to the nearest unit timespan.
        /// </summary>
        /// <param name="datetime">Datetime object we're rounding.</param>
        /// <param name="roundingInterval">Timespan rounding period.</param>
        /// <returns>Rounded datetime</returns>
        public static DateTime Round(this DateTime datetime, TimeSpan roundingInterval)
        {
            return new DateTime((datetime - DateTime.MinValue).Round(roundingInterval).Ticks);
        }

        /// <summary>
        /// Extension method to explicitly round up to the nearest timespan interval.
        /// </summary>
        /// <param name="time">Base datetime object to round up.</param>
        /// <param name="interval">Timespan interval to round to</param>
        /// <returns>Rounded datetime</returns>
        /// <remarks>Using this with timespans greater than 1 day may have unintended
        /// consequences. Be aware that rounding occurs against ALL time, so when using
        /// timespan such as 30 days we will see 30 day increments but it will be based
        /// on 30 day increments from the beginning of time.</remarks>
        public static DateTime RoundUp(this DateTime time, TimeSpan interval)
        {
            if (interval == TimeSpan.Zero)
            {
                // divide by zero exception
                return time;
            }

            return new DateTime(((time.Ticks + interval.Ticks - 1) / interval.Ticks) * interval.Ticks);
        }

        /// <summary>
        /// Business day here is defined as any day of the week that is not saturday or sunday
        /// </summary>
        /// <param name="date">The date to be examined</param>
        /// <returns>A bool indicating wether the datetime is a weekday or not</returns>
        public static bool IsCommonBusinessDay(this DateTime date)
        {
            return (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday);
        }

        /// <summary>
        /// Converts the Resolution instance into a TimeSpan instance
        /// </summary>
        /// <param name="resolution">The resolution to be converted</param>
        /// <returns>A TimeSpan instance that represents the resolution specified</returns>
        public static TimeSpan ToTimeSpan(this Resolution resolution)
        {
            switch (resolution)
            {
                case Resolution.Tick:
                    // ticks can be instantaneous
                    return TimeSpan.FromTicks(0);
                case Resolution.Second:
                    return TimeSpan.FromSeconds(1);
                case Resolution.Minute:
                    return TimeSpan.FromMinutes(1);
                case Resolution.Hour:
                    return TimeSpan.FromHours(1);
                case Resolution.Daily:
                    return TimeSpan.FromDays(1);
                default:
                    throw new ArgumentOutOfRangeException("resolution");
            }
        }

        /// <summary>
        /// Blocks the current thread until the current <see cref="T:System.Threading.WaitHandle"/> receives a signal, while observing a <see cref="T:System.Threading.CancellationToken"/>.
        /// </summary>
        /// <param name="waitHandle">The wait handle to wait on</param>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken"/> to observe.</param>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="T:System.OperationCanceledExcepton"><paramref name="cancellationToken"/> was canceled.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
        public static bool WaitOne(this WaitHandle waitHandle, CancellationToken cancellationToken)
        {
            return waitHandle.WaitOne(Timeout.Infinite, cancellationToken);
        }

        /// <summary>
        /// Blocks the current thread until the current <see cref="T:System.Threading.WaitHandle"/> is set, using a <see cref="T:System.TimeSpan"/> to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken"/>.
        /// </summary>
        ///
        /// <returns>
        /// true if the <see cref="T:System.Threading.WaitHandle"/> was set; otherwise, false.
        /// </returns>
        /// <param name="waitHandle">The wait handle to wait on</param>
        /// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken"/> to observe.</param>
        /// <exception cref="T:System.Threading.OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue"/>.</exception>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded. </exception><exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
        public static bool WaitOne(this WaitHandle waitHandle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return waitHandle.WaitOne((int) timeout.TotalMilliseconds, cancellationToken);
        }

        /// <summary>
        /// Blocks the current thread until the current <see cref="T:System.Threading.WaitHandle"/> is set, using a 32-bit signed integer to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken"/>.
        /// </summary>
        ///
        /// <returns>
        /// true if the <see cref="T:System.Threading.WaitHandle"/> was set; otherwise, false.
        /// </returns>
        /// <param name="waitHandle">The wait handle to wait on</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken"/> to observe.</param>
        /// <exception cref="T:System.Threading.OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1, which represents an infinite time-out.</exception>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
        public static bool WaitOne(this WaitHandle waitHandle, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return WaitHandle.WaitAny(new[] { waitHandle, cancellationToken.WaitHandle }, millisecondsTimeout) == 0;
        }

        /// <summary>
        /// Gets the MD5 hash from a stream
        /// </summary>
        /// <param name="stream">The stream to compute a hash for</param>
        /// <returns>The MD5 hash</returns>
        public static byte[] GetMD5Hash(this Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(stream);
            }
        }

        /// <summary>
        /// Convert a string into the same string with a URL! :)
        /// </summary>
        /// <param name="source">The source string to be converted</param>
        /// <returns>The same source string but with anchor tags around substrings matching a link regex</returns>
        public static string WithEmbeddedHtmlAnchors(this string source)
        {
            var regx = new Regex("http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*([a-zA-Z0-9\\?\\#\\=\\/]){1})?", RegexOptions.IgnoreCase);
            var matches = regx.Matches(source);
            foreach (Match match in matches)
            {
                source = source.Replace(match.Value, $"<a href=\'{match.Value}\' target=\'blank\'>{match.Value}</a>");
            }
            return source;
        }

        /// <summary>
        /// Get the first occurence of a string between two characters from another string
        /// </summary>
        /// <param name="value">The original string</param>
        /// <param name="left">Left bound of the substring</param>
        /// <param name="right">Right bound of the substring</param>
        /// <returns>Substring from original string bounded by the two characters</returns>
        public static string GetStringBetweenChars(this string value, char left, char right)
        {
            var startIndex = 1 + value.IndexOf(left);
            var length = value.IndexOf(right, startIndex) - startIndex;
            if (length > 0)
            {
                value = value.Substring(startIndex, length);
                startIndex = 1 + value.IndexOf(left);
                return value.Substring(startIndex).Trim();
            }
            return string.Empty;
        }

        /// <summary>
        /// Return the first in the series of names, or find the one that matches the configured algorithmTypeName
        /// </summary>
        /// <param name="names">The list of class names</param>
        /// <param name="algorithmTypeName">The configured algorithm type name from the config</param>
        /// <returns>The name of the class being run</returns>
        public static string SingleOrAlgorithmTypeName(this List<string> names, string algorithmTypeName)
        {
            // If there's only one name use that guy
            if (names.Count == 1) { return names.Single(); }

            // If we have multiple names we need to search the names based on the given algorithmTypeName
            // If the given name already contains dots (fully named) use it as it is
            // otherwise add a dot to the beginning to avoid matching any subsets of other names
            var searchName = algorithmTypeName.Contains(".") ? algorithmTypeName : "." + algorithmTypeName;
            return names.SingleOrDefault(x => x.EndsWith(searchName));
        }

        /// <summary>
        /// Converts the specified <paramref name="enum"/> value to its corresponding lower-case string representation
        /// </summary>
        /// <param name="enum">The enumeration value</param>
        /// <returns>A lower-case string representation of the specified enumeration value</returns>
        public static string ToLower(this Enum @enum)
        {
            return @enum.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Asserts the specified <paramref name="securityType"/> value is valid
        /// </summary>
        /// <remarks>This method provides faster performance than <see cref="Enum.IsDefined"/> which uses reflection</remarks>
        /// <param name="securityType">The SecurityType value</param>
        /// <returns>True if valid security type value</returns>
        public static bool IsValid(this SecurityType securityType)
        {
            switch (securityType)
            {
                case SecurityType.Base:
                case SecurityType.Equity:
                case SecurityType.Option:
                case SecurityType.FutureOption:
                case SecurityType.Commodity:
                case SecurityType.Forex:
                case SecurityType.Future:
                case SecurityType.Cfd:
                case SecurityType.Crypto:
                case SecurityType.Index:
                case SecurityType.IndexOption:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if the provided SecurityType is a type of Option.
        /// Valid option types are: Equity Options, Futures Options, and Index Options.
        /// </summary>
        /// <param name="securityType">The SecurityType to check if it's an option asset</param>
        /// <returns>
        /// true if the asset has the makings of an option (exercisable, expires, and is a derivative of some underlying),
        /// false otherwise.
        /// </returns>
        public static bool IsOption(this SecurityType securityType)
        {
            switch (securityType)
            {
                case SecurityType.Option:
                case SecurityType.FutureOption:
                case SecurityType.IndexOption:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if the provided SecurityType has a matching option SecurityType, used to represent
        /// the current SecurityType as a derivative.
        /// </summary>
        /// <param name="securityType">The SecurityType to check if it has options available</param>
        /// <returns>true if there are options for the SecurityType, false otherwise</returns>
        public static bool HasOptions(this SecurityType securityType)
        {
            switch (securityType)
            {
                case SecurityType.Equity:
                case SecurityType.Future:
                case SecurityType.Index:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Converts the specified string to its corresponding DataMappingMode
        /// </summary>
        /// <remarks>This method provides faster performance than enum parse</remarks>
        /// <param name="dataMappingMode">The dataMappingMode string value</param>
        /// <returns>The DataMappingMode value</returns>
        public static DataMappingMode? ParseDataMappingMode(this string dataMappingMode)
        {
            if (string.IsNullOrEmpty(dataMappingMode))
            {
                return null;
            }
            switch (dataMappingMode.LazyToLower())
            {
                case "0":
                case "lasttradingday":
                    return DataMappingMode.LastTradingDay;
                case "1":
                case "firstdaymonth":
                    return DataMappingMode.FirstDayMonth;
                case "2":
                case "openinterest":
                    return DataMappingMode.OpenInterest;
                default:
                    throw new ArgumentException($"Unexpected DataMappingMode: {dataMappingMode}");
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="securityType"/> value to its corresponding lower-case string representation
        /// </summary>
        /// <remarks>This method provides faster performance than <see cref="ToLower"/></remarks>
        /// <param name="securityType">The SecurityType value</param>
        /// <returns>A lower-case string representation of the specified SecurityType value</returns>
        public static string SecurityTypeToLower(this SecurityType securityType)
        {
            switch (securityType)
            {
                case SecurityType.Base:
                    return "base";
                case SecurityType.Equity:
                    return "equity";
                case SecurityType.Option:
                    return "option";
                case SecurityType.FutureOption:
                    return "futureoption";
                case SecurityType.IndexOption:
                    return "indexoption";
                case SecurityType.Commodity:
                    return "commodity";
                case SecurityType.Forex:
                    return "forex";
                case SecurityType.Future:
                    return "future";
                case SecurityType.Index:
                    return "index";
                case SecurityType.Cfd:
                    return "cfd";
                case SecurityType.Crypto:
                    return "crypto";
                default:
                    // just in case
                    return securityType.ToLower();
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="tickType"/> value to its corresponding lower-case string representation
        /// </summary>
        /// <remarks>This method provides faster performance than <see cref="ToLower"/></remarks>
        /// <param name="tickType">The tickType value</param>
        /// <returns>A lower-case string representation of the specified tickType value</returns>
        public static string TickTypeToLower(this TickType tickType)
        {
            switch (tickType)
            {
                case TickType.Trade:
                    return "trade";
                case TickType.Quote:
                    return "quote";
                case TickType.OpenInterest:
                    return "openinterest";
                default:
                    // just in case
                    return tickType.ToLower();
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="resolution"/> value to its corresponding lower-case string representation
        /// </summary>
        /// <remarks>This method provides faster performance than <see cref="ToLower"/></remarks>
        /// <param name="resolution">The resolution value</param>
        /// <returns>A lower-case string representation of the specified resolution value</returns>
        public static string ResolutionToLower(this Resolution resolution)
        {
            switch (resolution)
            {
                case Resolution.Tick:
                    return "tick";
                case Resolution.Second:
                    return "second";
                case Resolution.Minute:
                    return "minute";
                case Resolution.Hour:
                    return "hour";
                case Resolution.Daily:
                    return "daily";
                default:
                    // just in case
                    return resolution.ToLower();
            }
        }

        /// <summary>
        /// Process all items in collection through given handler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">Collection to process</param>
        /// <param name="handler">Handler to process those items with</param>
        public static void ProcessUntilEmpty<T>(this IProducerConsumerCollection<T> collection, Action<T> handler)
        {
            T item;
            while (collection.TryTake(out item))
            {
                handler(item);
            }
        }

        /// <summary>
        /// Performs on-line batching of the specified enumerator, emitting chunks of the requested batch size
        /// </summary>
        /// <typeparam name="T">The enumerable item type</typeparam>
        /// <param name="enumerable">The enumerable to be batched</param>
        /// <param name="batchSize">The number of items per batch</param>
        /// <returns>An enumerable of lists</returns>
        public static IEnumerable<List<T>> BatchBy<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                List<T> list = null;
                while (enumerator.MoveNext())
                {
                    if (list == null)
                    {
                        list = new List<T> {enumerator.Current};
                    }
                    else if (list.Count < batchSize)
                    {
                        list.Add(enumerator.Current);
                    }
                    else
                    {
                        yield return list;
                        list = new List<T> {enumerator.Current};
                    }
                }

                if (list?.Count > 0)
                {
                    yield return list;
                }
            }
        }

        /// <summary>
        /// Safely blocks until the specified task has completed executing
        /// </summary>
        /// <typeparam name="TResult">The task's result type</typeparam>
        /// <param name="task">The task to be awaited</param>
        /// <returns>The result of the task</returns>
        public static TResult SynchronouslyAwaitTaskResult<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Safely blocks until the specified task has completed executing
        /// </summary>
        /// <param name="task">The task to be awaited</param>
        /// <returns>The result of the task</returns>
        public static void SynchronouslyAwaitTask(this Task task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Safely blocks until the specified task has completed executing
        /// </summary>
        /// <param name="task">The task to be awaited</param>
        /// <returns>The result of the task</returns>
        public static T SynchronouslyAwaitTask<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Convert dictionary to query string
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        public static string ToQueryString(this IDictionary<string, object> pairs)
        {
            return string.Join("&", pairs.Select(pair => $"{pair.Key}={pair.Value}"));
        }

        /// <summary>
        /// Returns a new string in which specified ending in the current instance is removed.
        /// </summary>
        /// <param name="s">original string value</param>
        /// <param name="ending">the string to be removed</param>
        /// <returns></returns>
        public static string RemoveFromEnd(this string s, string ending)
        {
            if (s.EndsWith(ending))
            {
                return s.Substring(0, s.Length - ending.Length);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Thread safe concurrent dictionary order by implementation by using <see cref="SafeEnumeration{TSource,TKey}"/>
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/47630824/is-c-sharp-linq-orderby-threadsafe-when-used-with-concurrentdictionarytkey-tva</remarks>
        public static IOrderedEnumerable<KeyValuePair<TSource, TKey>> OrderBySafe<TSource, TKey>(
            this ConcurrentDictionary<TSource, TKey> source, Func<KeyValuePair<TSource, TKey>, TSource> keySelector
            )
        {
            return source.SafeEnumeration().OrderBy(keySelector);
        }

        /// <summary>
        /// Thread safe concurrent dictionary order by implementation by using <see cref="SafeEnumeration{TSource,TKey}"/>
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/47630824/is-c-sharp-linq-orderby-threadsafe-when-used-with-concurrentdictionarytkey-tva</remarks>
        public static IOrderedEnumerable<KeyValuePair<TSource, TKey>> OrderBySafe<TSource, TKey>(
            this ConcurrentDictionary<TSource, TKey> source, Func<KeyValuePair<TSource, TKey>, TKey> keySelector
            )
        {
            return source.SafeEnumeration().OrderBy(keySelector);
        }

        /// <summary>
        /// Force concurrent dictionary enumeration using a thread safe implementation
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/47630824/is-c-sharp-linq-orderby-threadsafe-when-used-with-concurrentdictionarytkey-tva</remarks>
        public static IEnumerable<KeyValuePair<TSource, TKey>> SafeEnumeration<TSource, TKey>(
            this ConcurrentDictionary<TSource, TKey> source)
        {
            foreach (var kvp in source)
            {
                yield return kvp;
            }
        }

        /// <summary>
        /// Returns a hex string of the byte array.
        /// </summary>
        /// <param name="source">the byte array to be represented as string</param>
        /// <returns>A new string containing the items in the enumerable</returns>
        public static string ToHexString(this byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                throw new ArgumentException($"Source cannot be null or empty.");
            }

            var hex = new StringBuilder(source.Length * 2);
            foreach (var b in source)
            {
                hex.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Determines if the two lists are equal, including all items at the same indices.
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="left">The left list</param>
        /// <param name="right">The right list</param>
        /// <returns>True if the two lists have the same counts and items at each index evaluate as equal</returns>
        public static bool ListEquals<T>(this IReadOnlyList<T> left, IReadOnlyList<T> right)
        {
            var count = left.Count;
            if (count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                if (!left[i].Equals(right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Computes a deterministic hash code based on the items in the list. This hash code is dependent on the
        /// ordering of items.
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="list">The list</param>
        /// <returns>A hash code dependent on the ordering of elements in the list</returns>
        public static int GetListHashCode<T>(this IReadOnlyList<T> list)
        {
            unchecked
            {
                var hashCode = 17;
                for (int i = 0; i < list.Count; i++)
                {
                    hashCode += (hashCode * 397) ^ list[i].GetHashCode();
                }

                return hashCode;
            }
        }

    }
}

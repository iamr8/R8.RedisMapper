using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using R8.RedisHelper.Utils.JsonConverters;

namespace R8.RedisHelper.Utils
{
    internal static class Helpers
    {
        private static Regex regex = new("(?:^|_| +)(.)", RegexOptions.Compiled);

        /// <summary>
        /// Returns properties of a type.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> to get properties from.</param>
        /// <returns>An array of public <see cref="PropertyInfo"/>s.</returns>
        /// <exception cref="ArgumentNullException">When the type is null.</exception>
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray();
            if (type.IsInterface)
            {
                var interfaceFields = type
                    .GetInterfaces()
                    .SelectMany(parentInterface => parentInterface.GetProperties(BindingFlags.Public | BindingFlags.Instance));
                props = props.Union(interfaceFields).ToArray();
            }

            props = props.DistinctBy(x => x.Name).ToArray();
            return props;
        }


        /// <summary>
        /// Returns a camelCase <see cref="string"/> from a given <see cref="string"/>.
        /// </summary>
        /// <returns>An updated <see cref="string"/> value.</returns>
        /// <exception cref="ArgumentNullException">When the string is null or empty.</exception>
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException(nameof(s));

            var key = regex.Replace(s, match => match.Groups[1].Value.ToUpper());
            if (key.Length == 0)
                return key;

            var sb = new StringBuilder(key[..1].ToLower());
            sb.Append(key[1..]);
            key = sb.ToString();

            return key;
        }
        
        /// <summary>
        /// Converts a DateTime to Unix Timestamp in seconds
        /// </summary>
        /// <returns>An integer representing the number of seconds that have elapsed since 00:00:00 UTC, Thursday, 1 January 1970.</returns>
        public static long ToUnixTimeSeconds(this DateTime datetime)
        {
            var timestamp = (long)datetime.Subtract(DateTime.UnixEpoch).TotalSeconds;
            return timestamp;
        }
    
        /// <summary>
        /// Converts a Unix Timestamp in seconds to DateTime
        /// </summary>
        /// <returns>A DateTime representing the number of seconds that have elapsed since 00:00:00 UTC, Thursday, 1 January 1970.</returns>
        public static DateTime FromUnixTimeSeconds(long seconds)
        {
            var timestamp = DateTime.UnixEpoch.AddSeconds(seconds);
            return timestamp;
        }
        
        /// <summary>
        /// Convert an object to Json
        /// </summary>
        public static string JsonSerialize<T>(this T obj, bool intended = false)
        {
            if (obj == null)
                return null;

            var defaultSettings = JsonSerializerDefaultOptions;
            defaultSettings.WriteIndented = intended;

            return JsonSerializer.Serialize(obj, defaultSettings);
        }
        
        /// <summary>
        /// A custom-defined <see cref="JsonSerializerOptions"/>
        /// </summary>
        public static JsonSerializerOptions JsonSerializerDefaultOptions
        {
            get
            {
                var settings = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
                    DefaultBufferSize = 1024,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };

                settings.Converters.Add(new JsonCultureToStringConverter());
                settings.Converters.Add(new JsonGuidToStringConverter());
                settings.Converters.Add(new JsonIPAddressToStringConverter());
                settings.Converters.Add(new JsonIPEndPointConverter());
                settings.Converters.Add(new JsonRegionInfoToStringConverter());
                settings.Converters.Insert(0, new JsonTimeSpanToLongConverter());
                settings.Converters.Insert(0, new JsonDateTimeToUnixConverter());

                return settings;
            }
        }
    }
}
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Runtime.Caching;
using Common.Extensions;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ReadThruCacheCommunication
    {
        public const string ReadThruNullKey = "ReadThruSourceNullKey";
        public const string ReadThruExceptionKey = "ReadThruExceptionKey";
        public const string ReadThruKeyForAbsoluteExpiration = "ReadThru|ABS_EXP=5";
        public const string ReadThruKeyForSlidingExpiration = "ReadThru|SLD_EXP=5";
        public const string ReadThruKeyForPriority = "ReadThru|PRIORITY=High";
        public const string ReadThruKeyForTag = "ReadThru|TAG=Sale";
        public const string ReadThruKeyForNamedTag = "ReadThru|NAMED_TAG=discount:3.5"; // DONOT CHANGE TYPE FROM DOUBLE AS IT IS BEING USED BY READTHRU PROVIDER
        public const string ReadThruKeyForQueryInfo = $"ReadThru|QUERY_INFO={nameof(Product.Id)}:3760";
        public const string ReadThruKeyForStateTransfer = $"ReadThru|STATE_TRANSFER";

        public const string ReadThruExceptionMessage = "ReadThru was informed to throw this exception. Key passed is {0}";

        static double _magicNumber = -99.99;
        public const int AcceptableExpirationDeviation = 1; //expiration obtanied from cache may not be exact as set. 



        internal static ReadThruCacheCommunicationCases GetCaseForKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key cannot be null for ReadThruCacheCommunicationCases retrieval.");

            return key switch
            {
                var k when k.Contains(ReadThruNullKey) => ReadThruCacheCommunicationCases.ReturnNull,
                var k when k.Contains(ReadThruExceptionKey) => ReadThruCacheCommunicationCases.ThrowException,
                var k when k.Contains(ReadThruKeyForAbsoluteExpiration) => ReadThruCacheCommunicationCases.SetAbsoluteExpiration,
                var k when k.Contains(ReadThruKeyForSlidingExpiration) => ReadThruCacheCommunicationCases.SetSlidingExpiration,
                var k when k.Contains(ReadThruKeyForPriority) => ReadThruCacheCommunicationCases.SetPriority,
                var k when k.Contains(ReadThruKeyForTag) => ReadThruCacheCommunicationCases.SetTag,
                var k when k.Contains(ReadThruKeyForNamedTag) => ReadThruCacheCommunicationCases.SetNamedTag,
                var k when k.Contains(ReadThruKeyForQueryInfo) => ReadThruCacheCommunicationCases.SetQueryInfo,
                var k when k.Contains(ReadThruKeyForStateTransfer) => ReadThruCacheCommunicationCases.StartNodeForStateTransfer,
                _ => ReadThruCacheCommunicationCases.CaseForValidKey
            };
        }

        public static bool IsExpirationIntervalSameAsSetByReadThru(TimeSpan expireAfter, string key )
        {
            var expirationIntervalFetched = GetExpirationValueFromKey(key);
            var expirationIntervalSet = expireAfter.TotalMinutes; // Math.Ceiling(expireAfter.TotalMinutes); //expireAfter.TotalMinutes;

            Console.WriteLine($" expirationInterval: {expirationIntervalFetched} ==  expirationIntervalOfItemFromCache: {expirationIntervalSet}");


            return Math.Abs(expirationIntervalFetched - expirationIntervalSet) <= AcceptableExpirationDeviation;

        }

        public static double GetExpirationValueFromKey(string key)
        {
            if(key == null)
                throw new ArgumentNullException(nameof(key), "Key cannot be null for expiration retrieval.");

            var split = key.Split('=');

            if (split == null || split.Length < 2)
                return _magicNumber;
            //    throw new ArgumentException($"Key does not contain expected format for expiration retrieval. Key :{key}", nameof(key));

            string expirationValue = split[1];

            if (double.TryParse(expirationValue, out double expirationIntervalFetched))
                return expirationIntervalFetched;
                //throw new ArgumentException($"Invalid expiration value in key {key}. Expiration value {expirationValue}", nameof(key));

            return _magicNumber;
        }

        public static bool IsPrioritySameAsSetByReadThru(CacheItemPriority priority, string key)
        {
            return GetCacheItemPriorityFromKey(key).Equals(priority);
        }

        public static CacheItemPriority GetCacheItemPriorityFromKey(string key)
        {
            if (key == null) 
                throw new ArgumentNullException(nameof(key), "Key cannot be null for priority retrieval.");

            var split = key?.Split('=');
            if (split == null || split.Length < 2)
                throw new ArgumentException($"Key does not contain expected format for priority retrieval. Key :{key}", nameof(key));

            string priorityValue = split[1];

            switch (priorityValue)
            {
                case "High":
                    return CacheItemPriority.High;

                case "Low":
                    return CacheItemPriority.Low;

                case "Normal":
                    return CacheItemPriority.Normal;

                case "NotRemovable":
                    return CacheItemPriority.NotRemovable;

                case "Default":
                    return CacheItemPriority.Default;

                case "AboveNormal":
                    return CacheItemPriority.AboveNormal;

                case "BelowNormal":
                    return CacheItemPriority.BelowNormal;

                default:
                    throw new ArgumentException($"Invalid priority value in key {key}. Priority value {priorityValue}", nameof(key));
            }
        }

        public static bool IsTagSameAsSetByReadThru(Tag[] tags, string key)
        {
            if (tags == null || tags.Length == 0)
                return false;

            var tag = GetCacheItemTagFromKey(key);

            return tag?.Equals(tags[0].TagName) == true;
        }

        public static string GetCacheItemTagFromKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key cannot be null for tag retrieval.");

            var split = key?.Split('=');
            if (split == null || split.Length < 2)
                throw new ArgumentException($"Key does not contain expected format for tag retrieval. Key :{key}", nameof(key));

           string tag = split[1];

            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException($"Invalid tag value in key {key}. Tag value {tag}", nameof(key));

            return tag;
        }

        public static bool IsNamedTagSameAsSetByReadThru(NamedTagsDictionary namedTags, string key)
        {
            IDictionary<string, object> namedTagAdded = GetEncodedKeyValuePair(key);

            if (namedTags == null || namedTags.Count != 1)
                return false;

            IDictionary<string, object>? namedTagReceived = namedTags.ToDictionary();

            if(namedTagReceived.TryGetValue(namedTagAdded.Keys.First(),out var val))
                return val.Equals(double.Parse(namedTagAdded.Values.First().ToString()));

            return false;

            //TODO remvoe below code after testing above code

            //var enumerator = namedTags.GetEnumerator();

            //while (enumerator.MoveNext())
            //{
            //    var current = enumerator.Current;
            //    if (current is DictionaryEntry entry)
            //    {
            //        return entry.Key.Equals(namedTagAdded.Keys.First()) &&
            //        entry.Value.Equals(double.Parse(namedTagAdded.Values.First().ToString()));
            //    }
            //    else if (current is KeyValuePair<string, object> kvp)
            //    {
            //        return kvp.Key.Equals(namedTagAdded.Keys.First()) &&
            //         kvp.Value.Equals(double.Parse(namedTagAdded.Values.First().ToString()));
            //    }
            //}

            //return false;
                        
        }

        public static FrozenDictionary<string, object> GetEncodedKeyValuePair(string key)
        {
            if(key == null)
                throw new ArgumentNullException(nameof(key), "Key cannot be null for key value pair retrieval.");

            var split = key?.Split('=');

            if (split == null || split.Length < 2)
                throw new ArgumentException($"Key does not contain expected format for key value pair retrieval. Key :{key}", nameof(key));

            string encodedPair = split[1];

            return new Dictionary<string, object>()
            {
                { encodedPair.Split(':')[0], encodedPair.Split(':')[1] }
            }.
            ToFrozenDictionary();//get icy icy feelings... if ever tried to modify it...
        }     
               
        public static void SetQueryInfo(string key, Product product)
        {
            // Get the encoded key-value pair (e.g. "Id" → 5)
            var pair = GetEncodedKeyValuePair(key);

            // We expect only one key-value pair
            string propertyName = pair.Keys.First();
            object propertyValue = pair.Values.First();

            // Get the property on the Product class
            var property = typeof(Product).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Property '{propertyName}' not found on Product class. Check if it matches the key from GetEncodedKeyValuePair.");
            }

            // Convert value to the correct property type
            object? convertedValue = Convert.ChangeType(propertyValue, property.PropertyType);

            // Set the value using reflection
            property.SetValue(product, convertedValue);
        }

    }

    public enum ReadThruCacheCommunicationCases
    {
        ReturnNull ,
        ThrowException ,
        SetAbsoluteExpiration   ,
        SetSlidingExpiration,
        SetPriority,
        SetTag,
        SetNamedTag,
        SetQueryInfo,
        CaseForValidKey,
        StartNodeForStateTransfer
    }
}

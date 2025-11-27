using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Common
{
    public class ProductNameParser
    {
        public const string CacheContext = "ProductForCache";
        public const string BackingSourceContext = "ProductForReadThru";

        public static bool IsItemObtainedFromCache(string name)
        {
            var split = name?.Split(':');

            if (split == null || split.Length < 3)
                throw new ArgumentException("Product name does not contain expected format for cache retrieval.", nameof(name));

            string description = split[0];
            string key = split[1];
            string context = split[2];

            if (string.IsNullOrEmpty(description) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(context))
                throw new ArgumentException("Product name does not contain expected format for cache retrieval.", nameof(name));

            return context == CacheContext;            
        }

        public static string PrepareProductNameForCache(string key)
        {
            if (string.IsNullOrEmpty(key))            
                throw new ArgumentException("key for preparing product name cannot be null or empty.", nameof(key));

            string description = $"Sample Product";
            return $"{description}:{key}:{CacheContext}";
        }

        internal static string PrepareProductNameForBackingSource(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key for preparing product name cannot be null or empty.", nameof(key));

            string description = $"Sample Product";            
            return $"{description}:{key}:{BackingSourceContext}";
        }

        internal static string PrepareProductNameToVerifUpdateInCacheWorks(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"key for {nameof(PrepareProductNameToVerifUpdateInCacheWorks)} method cannot be null or empty.", nameof(key));

            string description = $"Sample Product";
            return $"{description}:{key}:{nameof(PrepareProductNameToVerifUpdateInCacheWorks)}";
        }
    }
}

using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Common;

namespace BackingSourceTests.ReadThru
{
    internal class ReadThruBase : TestBase
    {
        public static ReadThruOptions GetReadThruOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.ReadThru,
                ProviderName = ReadThruProviderName
            };

            return readThruOptions;
        }

        public static ReadThruOptions GetDefaultReadThruOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.ReadThru,
            };

            return readThruOptions;
        }

        public static ReadThruOptions GetReadThruForcedOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.ReadThruForced,
                ProviderName = ReadThruProviderName
            };

            return readThruOptions;
        }

        public static ReadThruOptions GetInvalidReadOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.ReadThruForced,
                ProviderName = Guid.NewGuid().ToString() // Random invalid provider name
            };

            return readThruOptions;
        }
        public static ReadThruOptions GetNoneReadThruOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.None,
                ProviderName = ReadThruProviderName
            };

            return readThruOptions;
        }

        public static ResyncOptions GetResyncOptions()
        {
            ResyncOptions resyncOptions = new(true, ReadThruProviderName);           
            return resyncOptions;
        }   

        public static string[] GetRandomKeysForReadThruBulk(int totalKeys = 10000)
        {
            string[] keys = new string[totalKeys];

            for (int i = 0; i < totalKeys; i++)
            {
                keys[i] = GetRandomKey();
            }

            return keys;
        }

        public CacheItem GetCacheItemWithResyncOptions(Product stale)
        {
            return new CacheItem(stale)
            {
                Expiration = new Expiration(ExpirationType.Sliding, TimeSpan.FromSeconds(CleanInterval)),
                ResyncOptions = GetResyncOptions()
            };
        }

        public string[] GetHalfKeys(string[] keys)
        {
            var half = keys.Take(keys.Length / 2).ToArray();
            return half;
        }

        public string UpdateKeyWith(string key, string updatedValue)
        {
            return string.Join( Tilda ,[key, updatedValue]);
        }

        public string[] UpdateKeysWith(string[] keys, string updatedValue)
        {
            string[] updatedKeys = new string[keys.Length];

            for (int i = 0; i < keys.Length; i++)
            {
                updatedKeys[i] = UpdateKeyWith(keys[i],updatedValue);
            }

            return updatedKeys;
        }

        public string[] GetUpdatedHalfKeys(string[] keys, string updatedMessage)
        {
            var halfKeys = GetHalfKeys(keys);
            var updatedKeys = UpdateKeysWith(halfKeys, updatedMessage);
            return updatedKeys;
        }

        public void PerformAddBulk(string[] keys)
        {
            var items = new Dictionary<string, CacheItem>();
            
            for (int i = 0; i < keys.Length; i++)
            {
                var product = Util.GetProductForCache(keys[i]);
                var cacheItem = GetCacheItem(product);
                items.Add(keys[i], cacheItem);
            }

            Cache.AddBulk(items);
        }

        private CacheItem GetCacheItem(Product product)
        {
            var cacheItem = new CacheItem(product);
            return cacheItem;
        }

        public void PerformAddBulkWithResyncOptions(string[] keys)
        {
            var bulkItems = new Dictionary<string, CacheItem>();

            foreach (var key in keys)
            {
                var staleProduct = Util.GetProductForCache(key);
                CacheItem cacheItem = GetCacheItemWithResyncOptions(staleProduct);
                bulkItems.Add(key, cacheItem);
            }

            Cache.AddBulk(bulkItems);
        }
    }
}
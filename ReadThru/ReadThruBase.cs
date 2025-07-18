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
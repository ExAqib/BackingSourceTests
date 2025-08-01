using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Common;

namespace BackingSourceTests.WriteThru.Bulk
{
    class WriteThruBulkBase : WriteThruBase
    {
        public Dictionary<string, CacheItem> CreateSampleItems(int count)
        {
            var dict = new Dictionary<string, CacheItem>();
            for (int i = 0; i < count; i++)
            {
                var key = GetRandomKey();
                var product = Util.GetProductForCache(key);
                dict[key] = GetCacheItem(product);
            }
            return dict;
        }

        public Dictionary<string, CacheItem> TransformKeys(
            Dictionary<string, CacheItem> original,
            string transformationKey)
        {
            return original.Keys.ToDictionary(
                key => UpdateKeyWith(key, transformationKey),
                key => original[key]
            );
        }

        protected virtual object BulkAct(string mode, bool preAdd, Dictionary<string, CacheItem> items)
        {
            if (preAdd)
            {
                Cache.AddBulk(items);
                return Cache.InsertBulk(items, GetWriteThruOptions(mode));
            }
            else
            {
                return Cache.AddBulk(items, GetWriteThruOptions(mode));
            }
        }

       
    }
}

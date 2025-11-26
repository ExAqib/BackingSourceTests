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

        public void VerifyCacheUnchanged(Dictionary<string, CacheItem> expectedItems)
        {
            foreach (var kvp in expectedItems)
            {
                var resultItem = Cache.Get<Product>(kvp.Key);
                Assert.That(resultItem, Is.Not.Null, $"Item [{kvp.Key}] should remain in cache.");
                Assert.That(resultItem, Is.EqualTo(kvp.Value.GetValue<Product>()), $"Cache content mismatch for [{kvp.Key}].");
            }
        }

        public void VerifyCacheUpdatedByBackingSource(Dictionary<string, CacheItem> updatedItems)
        {
            foreach (var kvp in updatedItems)
            {
                var item = Cache.Get<Product>(kvp.Key);
                Assert.That(item, Is.Not.Null, $"Item [{kvp.Key}] missing after update.");
                VerifyItemObtainedByUpdateInCache(kvp.Key, item);
                //VerifyItemObtainedFromBackingSource(kvp.Key, item);

                var previousValue = kvp.Value.GetValue<Product>();
                Assert.That(previousValue, Is.Not.EqualTo(item), $"Expected updated value for [{kvp.Key}].");
            }
        }

    }
}

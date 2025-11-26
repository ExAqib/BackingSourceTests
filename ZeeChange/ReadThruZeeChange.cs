using Alachisoft.NCache.Client;
using BackingSourceTests.ReadThru;
using Common;


namespace BackingSourceTests.ZeeChange
{
    [TestFixture]
    internal class ReadThruZeeChange : ReadThruBase
    {
        string[] _keys;

        [SetUp]
        public void SetUp()
        {
            Cache.Clear();
            StopSecondNode();
            _keys = GetRandomKeysForReadThruBulk(10000);
            PerformAddBulk(_keys);
        }



        [Test]
        public void GetBulkItems_UpdateItemsWithReadThruForced_NodeIsStartedOnReadThru()
        {
            string[] halfKeys = GetUpdatedHalfKeys(_keys, ReadThruCacheCommunication.ReadThruKeyForStateTransfer);

            PerformAddBulk(halfKeys);

            var debugItem = Cache.Get<Product>(halfKeys[0]);

            AsyncStartNode();

            // GetBulk call will wait for 30 seconds for state transfer to complete.
            IDictionary<string, Product>? items = Cache.GetBulk<Product>(halfKeys, GetReadThruForcedOptions());

            // Debugger will be launched. Start another node manually.
            VerifyItemsObtainedFromBackingSource(items);

            Assert.That(items, Has.Count.EqualTo(halfKeys.Length));

            Assert.Multiple(() =>
            {
                // Updated keys must now have absolute expiration
                foreach (var key in halfKeys)
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    VerifyItemObtainedFromBackingSource(key, cacheItem.GetValue<Product>());
                }

                // Non-updated keys must not be affected by backing source
                foreach (var key in _keys.Except(halfKeys))
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    VerifyItemObtainedFromCache(cacheItem.GetValue<Product>());
                }
            });
        }
      
        [Test]
        public void GetBulkItems_WithReadThruForced_NodeIsStartedOnReadThru()
        {
            string[] halfKeys = GetUpdatedHalfKeys(_keys, ReadThruCacheCommunication.ReadThruKeyForStateTransfer);

            PerformAddBulk(halfKeys);

            var debugItem = Cache.Get<Product>(_keys[0]);

            AsyncStartNode();

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(halfKeys, GetReadThruForcedOptions());

            // Debugger will be launched. Start another node manually.
            VerifyItemsObtainedFromBackingSource(items);

            Assert.That(items, Has.Count.EqualTo(halfKeys.Length));

            Assert.Multiple(() =>
            {
                // Updated keys must now have absolute expiration
                foreach (var key in halfKeys)
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    VerifyItemObtainedFromBackingSource(key, cacheItem.GetValue<Product>());                   
                }

                // Non-updated keys must not be affected by backing source
                foreach (var key in _keys.Except(halfKeys))
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    VerifyItemObtainedFromCache(cacheItem.GetValue<Product>());
                }
            });
        }
               

    }

}

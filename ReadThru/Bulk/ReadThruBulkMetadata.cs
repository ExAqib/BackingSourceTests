using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Runtime.Caching;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ReadThru.Bulk
{
    [TestFixture]
    class ReadThruBulkMetadata : ReadThruBase
    {
        string[] _keys;

        [SetUp]
        public void SetUp()
        {
            Cache.Clear();
            _keys = GetRandomKeys(100);
            PerformAddBulk(_keys);
        }

        [Test]
        public void GetBulkItems_WithReadThruForced_AbsoluteExpirationIsApplied()
        {
            string[] halfKeys = GetUpdatedHalfKeys(_keys, ReadThruCacheCommunication.ReadThruKeyForAbsoluteExpiration);

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(halfKeys, GetReadThruForcedOptions());

            VerifyItemsObtainedFromBackingSource(items);

            Assert.Multiple(() =>
            {
                // Updated keys must now have absolute expiration
                foreach (var key in halfKeys)
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);

                    Assert.That(cacheItem?.Expiration?.Type, Is.Not.Null, $"Updated CacheItem or Expiration is null.");
                    Assert.That(cacheItem.Expiration.Type, Is.EqualTo(ExpirationType.Absolute),
                        $"Absolute Expiration not applied for key {key}");
                    Assert.That(
                        ReadThruCacheCommunication.IsExpirationIntervalSameAsSetByReadThru(cacheItem.Expiration.ExpireAfter, key),
                        Is.True,
                        $"Expiration interval mismatch for key {key}");
                }

                // Other untouched keys must remain unaffected
                foreach (var key in _keys.Except(halfKeys))
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    Assert.That(cacheItem?.Expiration?.Type,Is.EqualTo(ExpirationType.None),
                        $"Non-updated key {key} should NOT have expiration applied");
                }
            });
        }

        [Test]
        public void GetBulkItems_WithReadThruForced_SlidingExpirationIsApplied()
        {
            string[] halfKeys = GetUpdatedHalfKeys(_keys, ReadThruCacheCommunication.ReadThruKeyForSlidingExpiration);

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(halfKeys, GetReadThruForcedOptions());

            VerifyItemsObtainedFromBackingSource(items);

            Assert.Multiple(() =>
            {
                foreach (var key in halfKeys)
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);

                    Assert.That(cacheItem?.Expiration?.Type, Is.Not.Null, $"Updated CacheItem or Expiration is null.");
                    Assert.That(cacheItem.Expiration.Type, Is.EqualTo(ExpirationType.Sliding),
                        $"Sliding Expiration not applied for key {key}");
                    Assert.That(
                        ReadThruCacheCommunication.IsExpirationIntervalSameAsSetByReadThru(cacheItem.Expiration.ExpireAfter, key),
                        Is.True,
                        $"Expiration interval mismatch for key {key}");
                }

                foreach (var key in _keys.Except(halfKeys))
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    Assert.That(cacheItem?.Expiration?.Type, Is.EqualTo(ExpirationType.None),
                        $"Non-updated key {key} should NOT have expiration applied");
                }
            });
        }

        [Test]
        public void GetBulkItems_WithReadThruForced_ItemPriorityIsApplied()
        {
            string[] halfKeys = GetUpdatedHalfKeys(_keys, ReadThruCacheCommunication.ReadThruKeyForPriority);

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(halfKeys, GetReadThruForcedOptions());

            VerifyItemsObtainedFromBackingSource(items);

            Assert.Multiple(() =>
            {
                foreach (var key in halfKeys)
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);

                    Assert.That(cacheItem?.Priority, Is.Not.Null, $"Priority not set for key {key}");
                    Assert.That(
                        ReadThruCacheCommunication.IsPrioritySameAsSetByReadThru(cacheItem.Priority, key),
                        Is.True,
                        $"Priority mismatch for key {key}");
                }

                foreach (var key in _keys.Except(halfKeys))
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    Assert.That(cacheItem?.Priority, Is.EqualTo(CacheItemPriority.Normal),
                        $"Non-updated key {key} should have default priority");
                }
            });
        }

        [Test]
        public void GetBulkItems_WithReadThruForced_TagsAreAppliedAndQueryable()
        {
            string[] halfKeys = GetUpdatedHalfKeys(_keys, ReadThruCacheCommunication.ReadThruKeyForTag);

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(halfKeys, GetReadThruForcedOptions());

            VerifyItemsObtainedFromBackingSource(items);

            Assert.Multiple(() =>
            {
                foreach (var key in halfKeys)
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);

                    Assert.That(cacheItem?.Tags, Is.Not.Null, $"Tags not applied for key {key}");
                    Assert.That(
                        ReadThruCacheCommunication.IsTagSameAsSetByReadThru(cacheItem.Tags, key),
                        Is.True,
                        $"Tag mismatch for key {key}");

                    // Validate queryability via SearchService.GetByTag
                    var expectedTag = new Tag(ReadThruCacheCommunication.GetCacheItemTagFromKey(key));
                    ICollection<string> taggedKeys = Cache.SearchService.GetKeysByTag(expectedTag);
                    Assert.That(taggedKeys.Contains(key), Is.True,
                        $"Key {key} should be retrievable via tag query.");
                }

                foreach (var key in _keys.Except(halfKeys))
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    Assert.That(cacheItem?.Tags, Is.Null,
                        $"Non-updated key {key} should NOT have tags");
                }
            });
        }

        [Test]
        public void GetBulkItems_WithReadThruForced_NamedTagsAreAppliedAndQueryable()
        {
            string[] halfKeys = GetUpdatedHalfKeys(_keys, ReadThruCacheCommunication.ReadThruKeyForNamedTag);

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(halfKeys, GetReadThruForcedOptions());

            VerifyItemsObtainedFromBackingSource(items);

            Assert.Multiple(() =>
            {
                foreach (var key in halfKeys)
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);

                    Assert.That(cacheItem?.NamedTags, Is.Not.Null, $"NamedTags not applied for key {key}");
                    Assert.That(
                        ReadThruCacheCommunication.IsNamedTagSameAsSetByReadThru(cacheItem.NamedTags, key),
                        Is.True,
                        $"NamedTags mismatch for key {key}");
                }

                foreach (var key in _keys.Except(halfKeys))
                {
                    CacheItem cacheItem = Cache.GetCacheItem(key);
                    Assert.That(cacheItem?.NamedTags, Is.Null,
                        $"Non-updated key {key} should NOT have named tags");
                }
            });
        }

        [Test]
        public void GetBulkItems_WithReadThruForced_QueryInfoIsAppliedAndQueryable()
        {
            string[] halfKeys = GetUpdatedHalfKeys(_keys, ReadThruCacheCommunication.ReadThruKeyForQueryInfo);

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(halfKeys, GetReadThruForcedOptions());

            foreach (var key in halfKeys)
            {
                var queryKeyValue = ReadThruCacheCommunication.GetEncodedKeyValuePair(key);
                string fieldName = queryKeyValue.Keys.First();
                object expectedValue = queryKeyValue.Values.First();

                string query = $"SELECT * FROM {typeof(Product).FullName} WHERE {fieldName} = {expectedValue}";
                TestContext.WriteLine($"Executing query: {query}");

                var queryCommand = new QueryCommand(query);
                var reader = Cache.SearchService.ExecuteReader(queryCommand);

                bool foundMatchingRecord = false;
                while (reader.Read())
                {
                    int index = reader.GetOrdinal(fieldName);
                    Assert.That(index, Is.GreaterThanOrEqualTo(0),
                        $"Expected field '{fieldName}' not found in query result metadata.");

                    var actualValue = reader.GetValue<object>(index);
                    Assert.That(actualValue.ToString(), Is.EqualTo(expectedValue),
                        $"Query returned unexpected value for field '{fieldName}'. Expected: {expectedValue}, got: {actualValue}");

                    foundMatchingRecord = true;
                }

                Assert.That(foundMatchingRecord, Is.True,
                    $"Query did not return any matching records for key '{key}' with {fieldName} = {expectedValue}.");
            }
        }

        [Test]
        public void GetBulkItems_WhenResyncTriggered_FetchesFreshValuesFromDataSource()
        {
            Cache.Clear();
            PerformAddBulkWithResyncOptions(_keys);

            SleepForCleanInterval(nameof(GetBulkItems_WhenResyncTriggered_FetchesFreshValuesFromDataSource));

            IDictionary<string, Product>? refreshedItems = Cache.GetBulk<Product>(_keys);

            Assert.That(refreshedItems, Is.Not.Null, "GetBulk should not return null after resync.");

            Assert.Multiple(() =>
            {
                foreach (var key in _keys)
                {
                    Product refreshed = refreshedItems[key];
                    VerifyItemObtainedFromBackingSource(key, refreshed);
                    Assert.That(refreshed, Is.Not.EqualTo(Util.GetProductForCache(key)),
                        $"Resync should have fetched a fresh value for key {key}.");
                }
            });
        }

    }

}
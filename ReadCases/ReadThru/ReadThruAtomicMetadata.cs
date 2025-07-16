using Alachisoft.NCache.Caching;
using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Common;


namespace BackingSourceTests.ReadCases.ReadThru
{
    [TestFixture]
    class ReadThruAtomicMetadata : ReadThruBase
    {
        //PENDING CASES
        // verify itemas are properly inserted to cachae with queryinfo, tags,namedtags etc
        // verify resync works

        [SetUp]
        public void ClearCache()
        {
            Cache.Clear();
        }

        [Test]
        public void VerifyAbsoluteExpirationIsAppliedFromDataSource()
        {
            // Key that encodes expiration 
            // make sure cache is cleared before running this test. (currently [SetUp] is callled that clears the cache)
            string key = ReadThruCacheCommunication.ReadThruKeyForAbsoluteExpiration;

            var product = Cache.Get<Product>(key, GetReadThruOptions());

            // Fetch cache item metadata
            CacheItem cacheItem = Cache.GetCacheItem(key);

            Assert.Multiple(() =>
            {
                Assert.That(cacheItem?.Expiration?.Type, Is.Not.Null, "Absolute Expiration set by ReadThru Provider is not working.");
                Assert.That(cacheItem.Expiration.Type, Is.EqualTo(ExpirationType.Absolute));
                Assert.That(ReadThruCacheCommunication.IsExpirationIntervalSameAsSetByReadThru(cacheItem.Expiration.ExpireAfter, key), Is.True, "Expiration interval mismatch between cache and ReadThruProvider");
            });
        }

        [Test]
        public void VerifySlidingExpirationIsAppliedFromDataSource()
        {
            // Key that encodes expiration 
            // make sure cache is cleared before running this test. (currently [SetUp] is callled that clears the cache)
            string key = ReadThruCacheCommunication.ReadThruKeyForSlidingExpiration;

            var product = Cache.Get<Product>(key, GetReadThruOptions());

            // Fetch cache item metadata
            CacheItem cacheItem = Cache.GetCacheItem(key);

            Assert.Multiple(() =>
            {
                Assert.That(cacheItem?.Expiration?.Type, Is.Not.Null, "Sliding Expiration set by ReadThru Provider is not working.");
                Assert.That(cacheItem.Expiration.Type, Is.EqualTo(ExpirationType.Sliding));
                Assert.That(ReadThruCacheCommunication.IsExpirationIntervalSameAsSetByReadThru(cacheItem.Expiration.ExpireAfter, key), Is.True, "Expiration interval mismatch between cache and ReadThruProvider");
            });
        }

        [Test]
        public void VerifyItemPriorityIsAppliedFromDataSource()
        {
            string key = ReadThruCacheCommunication.ReadThruKeyForPriority;

            var product = Cache.Get<Product>(key, GetReadThruOptions());

            CacheItem cacheItem = Cache.GetCacheItem(key);

            Assert.Multiple(() =>
            {
                Assert.That(cacheItem?.Priority, Is.Not.Null, "CacheItem Priority set by ReadThru Provider is not working.");
                Assert.That(ReadThruCacheCommunication.IsPrioritySameAsSetByReadThru(cacheItem.Priority, key), Is.True, "CacheItem priority mismatch between cache and ReadThruProvider");
            });
        }


        [Test]
        public void VerifyTagsAreAppliedFromDataSource()
        {
            string key = ReadThruCacheCommunication.ReadThruKeyForNamedTag;

            var product = Cache.Get<Product>(key, GetReadThruOptions());

            CacheItem cacheItem = Cache.GetCacheItem(key);
            Assert.Multiple(() =>
            {
                Assert.That(cacheItem?.Tags, Is.Not.Null, "CacheItem Tag set by ReadThru Provider is not working.");
                Assert.That(ReadThruCacheCommunication.IsTagSameAsSetByReadThru(cacheItem.Tags, key), Is.True, "CacheItem priority mismatch between cache and ReadThruProvider");
            });
        }

        [Test]
        public void VerifyNamedTagsAreAppliedFromDataSource()
        {
            string key = ReadThruCacheCommunication.ReadThruKeyForNamedTag;

            var product = Cache.Get<Product>(key, GetReadThruOptions());

            CacheItem cacheItem = Cache.GetCacheItem(key);

            Assert.Multiple(() =>
            {
                Assert.That(cacheItem?.NamedTags, Is.Not.Null, "CacheItem NamedTags set by ReadThru Provider is not working.");
                Assert.That(ReadThruCacheCommunication.IsNamedTagSameAsSetByReadThru(cacheItem.NamedTags, key), Is.True, "CacheItem NamedTags mismatch between cache and ReadThruProvider");
            });
        }

        [Test]
        public void VerifyQueryInfoAppliedFromDataSource()
        {
            // PRE-REQUISITE: MAKE SURE PRODUCT CLASS IS QUERY INDEXED
            string key = ReadThruCacheCommunication.ReadThruKeyForQueryInfo;

            var product = Cache.Get<Product>(key, GetReadThruOptions());
            Assert.That(product, Is.Not.Null, "Product should not be null when ReadThru is configured.");

            var queryKeyValue = ReadThruCacheCommunication.GetEncodedKeyValuePair(key);
            string fieldName = queryKeyValue.Keys.First();
            object expectedValue = queryKeyValue.Values.First();

            string query = $"SELECT * FROM Product WHERE {fieldName} = {expectedValue}";
            TestContext.WriteLine($"Executing query: {query}");
            var queryCommand =  new QueryCommand(query);

            var reader = Cache.SearchService.ExecuteReader(queryCommand);

            bool foundMatchingRecord = false;
            while (reader.Read())
            {
               int index = reader.GetOrdinal(fieldName);

                Assert.That(index, Is.GreaterThanOrEqualTo(0),
                    $"Expected field '{fieldName}' not found in query result metadata.");

                var actualValue = reader.GetValue<object>(index);

                Assert.That(actualValue, Is.EqualTo(expectedValue),
                    $"Query returned unexpected value for field '{fieldName}'. Expected: {expectedValue}, but got: {actualValue}");

                foundMatchingRecord = true;
            }

            Assert.That(foundMatchingRecord, Is.True,
      $"Query did not return any matching records for key '{key}' with condition {fieldName} = {expectedValue}.");

        }
    }
}

using Alachisoft.NCache.Common;
using Alachisoft.NCache.Config.Dom;
using Alachisoft.NCache.Runtime.Exceptions;
using Common;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Quartz.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ReadThru.Bulk
{
    [TestFixture]
    class ReadThruBulk : ReadThruBase
    {
        string[] _keys;

        //TODO bulk get cache item test case

        [SetUp]
        public void SetUp()
        {
            Cache.Clear();
            _keys = GetRandomKeysForReadThruBulk();
        }

        
        [Test]
        public void GetBulkItems_WhenNoneInCache_FetchesAllFromDataSource()
        {
            IDictionary<string, Product>? items = Cache.GetBulk<Product>(_keys, GetReadThruOptions());

            Assert.Multiple(() =>
            {
                Assert.That(items, Is.Not.Null, "BulkItems should not be null when fetched from ReadThru.");
                Assert.That(items.Count, Is.EqualTo(_keys.Length), "All keys should have been fetched from ReadThru.");

                foreach (var key in _keys)
                {
                    Assert.That(items.ContainsKey(key), Is.True, $"Key {key} should be present in the fetched items.");
                    VerifyItemObtainedFromBackingSource(key, items[key]);
                }
            });
        }

        [Test]
        public void GetBulkItems_WhenSomeAlreadyCached_FetchesOnlyMissingFromDataSource()
        {
            var halfKeys = _keys.Take(_keys.Length / 2).ToArray();
            foreach (var key in halfKeys)
            {
                Cache.Insert(key, Util.GetProductForCache(key));
            }                

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(_keys, GetReadThruOptions());

            Assert.Multiple(() =>
            {
                Assert.That(items.Count, Is.EqualTo(_keys.Length), "All requested keys should be returned.");

                foreach (var key in halfKeys)
                {
                    VerifyItemObtainedFromCache(items[key]);
                }

                foreach (var key in _keys.Except(halfKeys))
                {
                    VerifyItemObtainedFromBackingSource(key, items[key]);
                }
            });
        }

        [Test]
        public void GetBulkItems_WithReadThruForced_RefreshesAllFromDataSource()
        {
            foreach (var key in _keys)
            {
                Cache.Insert(key, new Product { Id = -1, Name = "Stale", Price = 0 });
            }

            //PerformAddBulk(keys); can also call this. just verify above product is needed or nnot 

            IDictionary<string, Product>? freshItems = Cache.GetBulk<Product>(_keys, GetReadThruForcedOptions());

            Assert.Multiple(() =>
            {
                foreach (var key in _keys)
                {
                    VerifyItemObtainedFromBackingSource(key, freshItems[key]);

                    var refreshed = Cache.Get<Product>(key);
                    VerifyItemObtainedFromBackingSource(key, refreshed);
                }
            });
        }

        public void GetBulkItems_WithReadThruForcedAndOneKeyInCache_RefreshesAllFromDataSource()
        {

            var halfKeys = _keys.Take(_keys.Length / 2).ToArray();

            foreach (var key in _keys)
            {
                Cache.Insert(key, new Product { Id = -1, Name = "Stale", Price = 0 });
            }

            Cache.Remove(_keys[_keys.Length / 2]);

            IDictionary<string, Product>? freshItems = Cache.GetBulk<Product>(_keys, GetReadThruForcedOptions());

            Assert.Multiple(() =>
            {
                foreach (var key in _keys)
                {
                    VerifyItemObtainedFromBackingSource(key, freshItems[key]);

                    var refreshed = Cache.Get<Product>(key);
                    VerifyItemObtainedFromBackingSource(key, refreshed);
                }
            });
        }

        [Test]
        public void GetBulkItems_WhenAllCached_DoesNotCallDataSource()
        {
            foreach (var key in _keys)
            {
                Cache.Insert(key, Util.GetProductForCache(key));
            }

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(_keys, GetReadThruOptions());

            Assert.Multiple(() =>
            {
                Assert.That(items.Count, Is.EqualTo(_keys.Length));
                foreach (var key in _keys)
                {
                    VerifyItemObtainedFromCache(items[key]);
                }
            });
        }

        [Test]
        public void GetBulkItems_WithInvalidReadThruOptions_ThrowsOperationFailedException()
        {
            var ex = Assert.Throws<OperationFailedException>(() =>
            {
               var result = Cache.GetBulk<Product>(_keys, GetInvalidReadOptions());
            });

            Assert.That(ex.Message, Is.EqualTo(BackingSourceNotAvailable));
            Assert.That(Cache.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetBulkItems_WhenOneKeyThrowsException_NoItemAddedToCache()
        {
            _keys[_keys.Length / 2] = ReadThruCacheCommunication.ReadThruExceptionKey;

            IDictionary<string, Product>? getBulkResult = Cache.GetBulk<Product>(_keys, GetReadThruOptions());

            Assert.Multiple(() =>
            {
                Assert.That( getBulkResult, Is.Empty, $"If one key has thrown exception, all count should be 0. Expected Count: {0} Obtained count:{getBulkResult.Count}");
                Assert.That(Cache, Is.Empty, "No item should be added in cache if ReadThru has thrown exception.");
            });
        }

        [Test]
        public void GetBulk_PartialCacheHit_WithReadThruFailure_ReturnsCachedOnly()
        {
            _keys[_keys.Length / 2] = ReadThruCacheCommunication.ReadThruExceptionKey;
            int addedKeysInCache = 0;
            Random random = new();

            foreach (var key in _keys)
            {
                if(key == ReadThruCacheCommunication.ReadThruExceptionKey)
                    continue;

                if (addedKeysInCache == 0)
                {
                    Cache.Insert(key, Util.GetProductForCache(key));
                    addedKeysInCache++;
                    continue;
                }

                bool random_50_50_bool = random.NextDouble() < 0.5;

                if (random_50_50_bool)
                {
                    Cache.Insert(key, Util.GetProductForCache(key));
                    addedKeysInCache++;
                }
            }


             IDictionary<string, Product>? getBulkResult = Cache.GetBulk<Product>(_keys, GetReadThruOptions());

            Assert.Multiple(() =>
            {
                Assert.That(getBulkResult,Has.Count.EqualTo(addedKeysInCache),$"All items that are already present in cache should be returned. ItemsExpected: {addedKeysInCache} ItemsObtained: {getBulkResult.Count} ");
                Assert.That(getBulkResult.ContainsKey(ReadThruCacheCommunication.ReadThruExceptionKey),Is.False,"The key against which exception has been thrown, should have any value.");                
                foreach (var item in getBulkResult)
                {
                    VerifyItemObtainedFromCache(item.Value);
                }
            });
        }

        [Test]
        public void GetBulkItems_WhenSomeKeysReturnNull_SkipsNullKeysAndReturnsOthers()
        {
            _keys[_keys.Length / 2] = ReadThruCacheCommunication.ReadThruNullKey;

            IDictionary<string, Product>? getBulkResult = Cache.GetBulk<Product>(_keys, GetReadThruOptions());

            Assert.That(getBulkResult, Is.Not.Null);
            Assert.That(getBulkResult.Count, Is.EqualTo(_keys.Length - 1), "Null-returning key should be excluded.");
            Assert.That(getBulkResult.ContainsKey(ReadThruCacheCommunication.ReadThruExceptionKey), Is.False,"Null Key for backing source, should not be returned as there is no value against it.");

            VerifyItemsObtainedFromBackingSource(getBulkResult);
        }

        [Test]
        public void GetBulkItems_WhenCachedAndNoReadThruOptions_ReturnsFromCacheWithoutException()
        {
            foreach (var key in _keys)
            {
                Cache.Insert(key, Util.GetProductForCache(key));
            }

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(_keys);

            Assert.Multiple(() =>
            {
                Assert.That(items.Count, Is.EqualTo(_keys.Length));
                foreach (var key in _keys)
                {
                    Assert.That(items[key], Is.EqualTo(Util.GetProductForCache(key)));
                }
            });
        }
    }
}
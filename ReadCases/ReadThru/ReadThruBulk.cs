using Alachisoft.NCache.Common;
using Alachisoft.NCache.Config.Dom;
using Alachisoft.NCache.Runtime.Exceptions;
using Common;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ReadCases.ReadThru
{
    [TestFixture]
    class ReadThruBulk : ReadThruBase
    {
        [SetUp]
        public void ClearCache()
        {
            Cache.Clear();
        }

        [Test]
        public void GetBulkItems_WhenNoneInCache_FetchesAllFromDataSource()
        {
            string[] keys = GetRandomKeysForReadThruBulk();
            IDictionary<string, Product>? items = Cache.GetBulk<Product>(keys, GetReadThruOptions());

            Assert.Multiple(() =>
            {
                Assert.That(items, Is.Not.Null, "BulkItems should not be null when fetched from ReadThru.");
                Assert.That(items.Count, Is.EqualTo(keys.Length), "All keys should have been fetched from ReadThru.");

                foreach (var key in keys)
                {
                    Assert.That(items.ContainsKey(key), Is.True, $"Key {key} should be present in the fetched items.");
                    VerifyItemObtainedFromReadThru(key, items[key]);
                }
            });
        }

        [Test]
        public void GetBulkItems_WhenSomeAlreadyCached_FetchesOnlyMissingFromDataSource()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            var halfKeys = keys.Take(keys.Length / 2).ToArray();
            foreach (var key in halfKeys)
            {
                Cache.Insert(key, Util.GetProductForCache(key));
            }

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(keys, GetReadThruOptions());

            Assert.Multiple(() =>
            {
                Assert.That(items.Count, Is.EqualTo(keys.Length), "All requested keys should be returned.");

                foreach (var key in halfKeys)
                {
                    VerifyItemObtainedFromCache(items[key]);
                }

                foreach (var key in keys.Except(halfKeys))
                {
                    VerifyItemObtainedFromReadThru(key, items[key]);
                }
            });
        }

        [Test]
        public void GetBulkItems_WithReadThruForced_RefreshesAllFromDataSource()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            foreach (var key in keys)
            {
                Cache.Insert(key, new Product { Id = -1, Name = "Stale", Price = 0 });
            }

            IDictionary<string, Product>? freshItems = Cache.GetBulk<Product>(keys, GetReadThruForcedOptions());

            Assert.Multiple(() =>
            {
                foreach (var key in keys)
                {
                    VerifyItemObtainedFromReadThru(key, freshItems[key]);

                    var refreshed = Cache.Get<Product>(key);
                    VerifyItemObtainedFromReadThru(key, refreshed);
                }
            });
        }

        [Test]
        public void GetBulkItems_WhenAllCached_DoesNotCallDataSource()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            foreach (var key in keys)
            {
                Cache.Insert(key, Util.GetProductForCache(key));
            }

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(keys, GetReadThruOptions());

            Assert.Multiple(() =>
            {
                Assert.That(items.Count, Is.EqualTo(keys.Length));
                foreach (var key in keys)
                {
                    VerifyItemObtainedFromCache(items[key]);
                }
            });
        }

        [Test]
        public void GetBulkItems_WithInvalidReadThruOptions_ThrowsOperationFailedException()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.GetBulk<Product>(keys, GetInvalidReadOptions());
            });

            Assert.That(ex.Message, Is.EqualTo(BackingSourceNotAvailable));
            Assert.That(Cache.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetBulkItems_WhenOneKeyThrowsException_ReturnsNullForThatKey()
        {
            string[] keys = GetRandomKeysForReadThruBulk();
            keys[keys.Length / 2] = ReadThruCacheCommunication.ReadThruExceptionKey;

            IDictionary<string, Product>? getBulkResult = Cache.GetBulk<Product>(keys, GetReadThruOptions());

            Assert.That(getBulkResult, Is.Not.Null);
            Assert.That(getBulkResult.Count, Is.EqualTo(keys.Length), "One key failed, but total count should remain same.");
            Assert.That(getBulkResult[ReadThruCacheCommunication.ReadThruExceptionKey], Is.Null);

            getBulkResult.Remove(ReadThruCacheCommunication.ReadThruExceptionKey);
            VerifyItemsObtainedFromReadThru(getBulkResult);
        }

        [Test]
        public void GetBulkItems_WhenSomeKeysReturnNull_SkipsNullKeysAndReturnsOthers()
        {
            string[] keys = GetRandomKeysForReadThruBulk();
            keys[keys.Length / 2] = ReadThruCacheCommunication.ReadThruNullKey;

            IDictionary<string, Product>? getBulkResult = Cache.GetBulk<Product>(keys, GetReadThruOptions());

            Assert.That(getBulkResult, Is.Not.Null);
            Assert.That(getBulkResult.Count, Is.EqualTo(keys.Length - 1), "Null-returning key should be excluded.");
            Assert.That(getBulkResult[ReadThruCacheCommunication.ReadThruExceptionKey], Is.Null);

            VerifyItemsObtainedFromReadThru(getBulkResult);
        }

        [Test]
        public void GetBulkItems_WhenCachedAndNoReadThruOptions_ReturnsFromCacheWithoutException()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            foreach (var key in keys)
            {
                Cache.Insert(key, Util.GetProductForCache(key));
            }

            IDictionary<string, Product>? items = Cache.GetBulk<Product>(keys);

            Assert.Multiple(() =>
            {
                Assert.That(items.Count, Is.EqualTo(keys.Length));
                foreach (var key in keys)
                {
                    Assert.That(items[key], Is.EqualTo(Util.GetProductForCache(key)));
                }
            });
        }
    }
}
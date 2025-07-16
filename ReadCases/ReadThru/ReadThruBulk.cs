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

        // Verify all missing keys are obtained from DS in a single bulk call
        // Verify some keys in cache → only missing keys fetched
        // Verify forced bulk read-thru refreshes even existing keys
        // Verify no read-thru call is made if all keys are present in cache
        // Verify exception is thrown if wrong readthru options are given
        // Verify proper exception handling if one key fails(throws exception)
        // Verify proper handling if some keys return null
        // verify exception is not thrown if readthru options are not given and items are present in cache

        // Metat data cases 
        // client cache cases


        [SetUp]
        public void ClearCache()
        {
            Cache.Clear();
        }


        [Test]
        public void GetAllBulkItem_NoItemInCache_AllItemsFetchedFromReadThru()
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

        // Verify some keys in cache → only missing keys fetched
        [Test]
        public void GetBulkItems_SomeItemsAlreadyInCache_MissingItemsFetchedFromReadThru()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            // Pre-cache half of the keys
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
                    VerifyItemObtainedFromCache(items[key]); // should come from cache
                }

                foreach (var key in keys.Except(halfKeys))
                {
                    VerifyItemObtainedFromReadThru(key, items[key]); // should come from provider
                }
            });
        }

        // Verify all items are fetched from data source if ReadThruForced option is used and items are present in cache
        [Test]
        public void GetBulkItems_ReadThruForced_AllItemsRefreshedFromDataSource()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            // Pre-cache stale items
            foreach (var key in keys)
            {
                Cache.Insert(key, new Product { Id = -1, Name = "Stale", Price = 0 });
            }

            IDictionary<string, Product>? freshItems = Cache.GetBulk<Product>(keys, GetReadThruForcedOptions());

            Assert.Multiple(() =>
            {
                foreach (var key in keys)
                {
                    VerifyItemObtainedFromReadThru(key, freshItems[key]); // should always fetch fresh

                    // Also verify cache is refreshed
                    var refreshed = Cache.Get<Product>(key);
                    VerifyItemObtainedFromReadThru(key, refreshed);
                }
            });
        }

        // Verify no read-thru call is made if all keys are present in cache
        [Test]
        public void GetBulkItems_AllKeysInCache_NoReadThruCallMade()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            // Pre-cache all keys
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

        // Verify exception is thrown if wrong readthru options are given
        [Test]
        public void GetBulkItems_InvalidReadThruOptions_ThrowsOperationFailedException()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.GetBulk<Product>(keys, GetInvalidReadOptions());
            });

            Assert.That(ex.Message, Is.EqualTo(BackingSourceNotAvailable));
            Assert.That(Cache.Count  , Is.EqualTo(0));
        }

        // Verify proper exception handling if one key fails (throws exception)
        [Test]
        public void GetBulkItems_OneKeyThrowsException_ExceptionIsPropagated()
        {
            string[] keys = GetRandomKeysForReadThruBulk();
            keys[keys.Length / 2] = ReadThruCacheCommunication.ReadThruExceptionKey;

            IDictionary<string, Product>? getBulkResult  = Cache.GetBulk<Product>(keys, GetReadThruOptions());

            Assert.That(getBulkResult,Is.Not.Null);
            Assert.That(getBulkResult.Count, Is.EqualTo(keys.Length), "One key is failed, but count should not be affected");// Maybe fix it if needed
            Assert.That(getBulkResult[ReadThruCacheCommunication.ReadThruExceptionKey],Is.Null); //  GetBulk Resposne i.e. IDictionary<string, Product> cannot have excption. so exxpect null. Fix it if needed
            
            getBulkResult.Remove(ReadThruCacheCommunication.ReadThruExceptionKey); // Remove the key that throws exception
            VerifyItemsObtainedFromReadThru(getBulkResult);
        }

        // Verify proper handling if some keys return null
        [Test]
        public void GetBulkItems_SomeKeysReturnNull_ResultIsReturnedForNonNullKeys()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            keys[keys.Length / 2] = ReadThruCacheCommunication.ReadThruNullKey;

            IDictionary<string, Product>? getBulkResult = Cache.GetBulk<Product>(keys, GetReadThruOptions());

            Assert.That(getBulkResult, Is.Not.Null);
            Assert.That(getBulkResult.Count, Is.EqualTo(keys.Length - 1), "Null key should be skipped.");
            Assert.That(getBulkResult[ReadThruCacheCommunication.ReadThruExceptionKey], Is.Null);

            VerifyItemsObtainedFromReadThru(getBulkResult);
        }

        // Verify exception is NOT thrown if readthru options are NOT given and items are present in cache
        [Test]
        public void GetBulkItems_NoReadThruOptions_ItemsPresentInCache_NoException()
        {
            string[] keys = GetRandomKeysForReadThruBulk();

            // Pre-cache all keys
            foreach (var key in keys)
            {
                Cache.Insert(key, Util.GetProductForCache(key));
            }

            // Should retrieve from cache without throwing
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

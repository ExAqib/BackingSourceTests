using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Exceptions;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ReadThru.Atomic
{
    // verify  item is obtianed from data source (DS) if not present in cache
    // verify item is not obtained from ds if present in cache
    // verify item is obtained with default readThru options 
    // verify exception is thrown if wrong readthru options are given and item is not present in cache
    // verify exception is not thrown if readthru options are not given and item is present in cache
    //  ADD TEST CASES FOR CLIENT CACHE
    // verify exception is thrown if provider name that is provided in ReadThruOptions is not configured in cache
    // verify exception is not thrown if readOpprion .None is procvided
    // verify item is obtained from DS if ReadOption.ReadThruForced is provided. Make sure item is also present in cache
    // verify item is obtained from DS if ReadOption.ReadThruForced is provided. Make sure item is not present in cache
    // verify proper exception is thrown if null is returned by data soruce
    // verify proper exception is thrown if data source throws exception


    [TestFixture]
    internal class ReadThruAtomic : ReadThruBase
    {
        [Test]
        public void GetItem_WhenNotInCache_FetchesFromDataSource()
        {
            string key = GetRandomKey();
            Product product = Cache.Get<Product>(key, GetReadThruOptions());
            VerifyItemObtainedFromReadThru(key, product);
        }

        [Test]
        public void GetItem_WhenPresentInCache_DoesNotFetchFromDataSource()
        {
            string key = GetRandomKey();
            Product expected = Util.GetProductForCache(key);

            Cache.Insert(key, expected);

            Product product = Cache.Get<Product>(key, GetReadThruOptions());
            VerifyItemObtainedFromCache(product);
        }

        [Test]
        public void GetItem_WithDefaultReadThruOptions_FetchesFromDataSource()
        {
            string key = GetRandomKey();

            Product product = Cache.Get<Product>(key, GetDefaultReadThruOptions());
            VerifyItemObtainedFromReadThru(key, product);
        }

        [Test]
        public void GetItem_WithInvalidReadThruOptions_ThrowsOperationFailedException()
        {
            string key = GetRandomKey();

            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<object>(key, GetInvalidReadOptions());
            });

            Assert.That(ex.Message, Is.EqualTo(BackingSourceNotAvailable));
        }

        [Test]
        public void GetItem_WhenPresentInCacheWithoutReadThruOptions_DoesNotThrow()
        {
            string key = GetRandomKey();
            Product expected = Util.GetProductForCache(key);
            Cache.Insert(key, expected);

            Product product = Cache.Get<Product>(key);

            Assert.That(product, Is.Not.Null);
            Assert.That(product, Is.EqualTo(expected));
        }

        [Test]
        public void GetItem_WithReadModeNone_ReturnsCachedValue()
        {
            string key = GetRandomKey();
            Product expected = Util.GetProductForCache(key);
            Cache.Insert(key, expected);

            Product product = Cache.Get<Product>(key, GetNoneReadThruOptions());
            Assert.That(product, Is.EqualTo(expected));
        }

        [Test]
        public void GetItem_WithReadThruForced_WhenInCache_RefreshesFromDataSource()
        {
            string key = GetRandomKey();

            Product stale = new Product { Id = -1, Name = "Stale", Price = 0 };
            Cache.Insert(key, stale);

            Product freshProduct = Cache.Get<Product>(key, GetReadThruForcedOptions());

            VerifyItemObtainedFromReadThru(key, freshProduct);

            Product fromCache = Cache.Get<Product>(key);
            VerifyItemObtainedFromReadThru(key, fromCache);

            Assert.That(stale, Is.Not.EqualTo(fromCache));
        }

        [Test]
        public void GetItem_WithReadThruForced_WhenMissing_FetchesAndCachesFromDataSource()
        {
            string key = GetRandomKey();

            Product product = Cache.Get<Product>(key, GetReadThruForcedOptions());
            VerifyItemObtainedFromReadThru(key, product);

            Product fromCache = Cache.Get<Product>(key);
            VerifyItemObtainedFromReadThru(key, fromCache);
        }

        [Test]
        public void GetItem_WhenDataSourceReturnsNull_ThrowsOperationFailedException()
        {
            string key = ReadThruCacheCommunication.ReadThruNullKey;

            Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<Product>(key, GetReadThruOptions());
            }, "Should throw exception when DS returns null product.");
        }

        [Test]
        public void GetItem_WhenDataSourceThrowsException_ThrowsOperationFailedException()
        {
            string key = ReadThruCacheCommunication.ReadThruExceptionKey;

            Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<Product>(key, GetReadThruOptions());
            }, "Should propagate exception thrown by data source.");
        }
    }
}

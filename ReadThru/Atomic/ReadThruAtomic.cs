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
        private string _key;

        [SetUp]
        public void SetUp()
        {
            // This runs before each test
            Cache.Clear();
            _key = GetRandomKey();
        }

        [Test]
        public void GetItem_WhenNotInCache_FetchesFromDataSource()
        {
            Product product = Cache.Get<Product>(_key, GetReadThruOptions());
            VerifyItemObtainedFromBackingSource(_key, product);
        }

        [Test]
        public void GetItem_WhenPresentInCache_DoesNotFetchFromDataSource()
        {
            Product expected = Util.GetProductForCache(_key);
            Cache.Insert(_key, expected);

            Product product = Cache.Get<Product>(_key, GetReadThruOptions());
            VerifyItemObtainedFromCache(product);
        }

        [Test]
        public void GetItem_WithDefaultReadThruOptions_FetchesFromDataSource()
        {
            Product product = Cache.Get<Product>(_key, GetDefaultReadThruOptions());
            VerifyItemObtainedFromBackingSource(_key, product);
        }

        [Test]
        public void GetItem_WithInvalidReadThruOptions_ThrowsOperationFailedException()
        {
            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<object>(_key, GetInvalidReadOptions());
            });

            Assert.That(ex.Message, Is.EqualTo(BackingSourceNotAvailable));
        }

        [Test]
        public void GetItem_WhenPresentInCacheWithoutReadThruOptions_DoesNotThrow()
        {
            Product expected = Util.GetProductForCache(_key);
            Cache.Insert(_key, expected);

            Product product = Cache.Get<Product>(_key);

            Assert.That(product, Is.Not.Null);
            Assert.That(product, Is.EqualTo(expected));
        }

        [Test]
        public void GetItem_WithReadModeNone_ReturnsCachedValue()
        {
            Product expected = Util.GetProductForCache(_key);
            Cache.Insert(_key, expected);

            Product product = Cache.Get<Product>(_key, GetNoneReadThruOptions());
            Assert.That(product, Is.EqualTo(expected));
        }

        [Test]
        public void GetItem_WithReadThruForced_WhenInCache_RefreshesFromDataSource()
        {
            Product stale = new Product { Id = -1, Name = "Stale", Price = 0 };
            Cache.Insert(_key, stale);

            Product freshProduct = Cache.Get<Product>(_key, GetReadThruForcedOptions());
            VerifyItemObtainedFromBackingSource(_key, freshProduct);

            Product fromCache = Cache.Get<Product>(_key);
            VerifyItemObtainedFromBackingSource(_key, fromCache);

            Assert.That(stale, Is.Not.EqualTo(fromCache));
        }

        [Test]
        public void GetItem_WithReadThruForced_WhenMissing_FetchesAndCachesFromDataSource()
        {
            Product product = Cache.Get<Product>(_key, GetReadThruForcedOptions());
            VerifyItemObtainedFromBackingSource(_key, product);

            Product fromCache = Cache.Get<Product>(_key);
            VerifyItemObtainedFromBackingSource(_key, fromCache);
        }

        [Test]
        public void GetItem_WhenDataSourceReturnsNull_ThrowsOperationFailedException()
        {
            _key = ReadThruCacheCommunication.ReadThruNullKey; // override for null case

            Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<Product>(_key, GetReadThruOptions());
            }, "Should throw exception when DS returns null product.");
        }

        [Test]
        public void GetItem_WhenDataSourceThrowsException_ThrowsOperationFailedException()
        {
            _key = ReadThruCacheCommunication.ReadThruExceptionKey; // override for exception case

            Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<Product>(_key, GetReadThruOptions());
            }, "Should propagate exception thrown by data source.");
        }
    }

}

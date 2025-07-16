using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Exceptions;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ReadCases.ReadThru
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
        public void VerifyItemIsObtainedFromDataSource()
        {
            string key = GetRandomKey();
            Product product = Cache.Get<Product>(key, GetReadThruOptions());
            VerifyItemObtainedFromReadThru(key, product);
        }

        // ✅ verify item is NOT obtained from DS if present in cache
        [Test]
        public void VerifyItemIsNotObtainedFromDataSourceIfPresentInCache()
        {
            string key = GetRandomKey();
            Product expected = Util.GetProductForCache(key);
          
            Cache.Insert(key, expected);

            // Try to get from cache again with ReadThru options
            Product product = Cache.Get<Product>(key, GetReadThruOptions());
            VerifyItemObtainedFromCache(product);   
        }

        // ✅ verify item is obtained with default readThru options (no provider name)
        [Test]
        public void VerifyItemIsObtainedWithDefaultReadThruOptions()
        {
            string key = GetRandomKey();

            // Using default options (only Mode = ReadThru, no explicit provider)
            Product product = Cache.Get<Product>(key, GetDefaultReadThruOptions());
            VerifyItemObtainedFromReadThru(key, product);
        }

        // ✅ verify exception is thrown if wrong readthru options are given and item is not present in cache
        [Test]
        public void VerifyExceptionThrownWhenInvalidReadThruOptionsProvided()
        {
            string key = GetRandomKey(); 

            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<object>(key, GetInvalidReadOptions());
            });

            // Verify the message matches exactly
            Assert.That(ex.Message, Is.EqualTo(BackingSourceNotAvailable));
        }

        // ✅ verify NO exception is thrown if readThru options are NOT given & item present
        [Test]
        public void VerifyNoExceptionWhenItemPresentAndNoReadThruOptions()
        {
            string key = GetRandomKey();
            Product expected = Util.GetProductForCache(key);
            Cache.Insert(key, expected);

            // Should retrieve from cache without throwing
            Product product = Cache.Get<Product>(key);

            Assert.That(product, Is.Not.Null);
            Assert.That(product, Is.EqualTo(expected));
        }
                
        // ✅ verify NO exception if ReadMode.None is provided
        [Test]
        public void VerifyNoExceptionWhenReadModeNone()
        {
            string key = GetRandomKey();
            Product expected = Util.GetProductForCache(key);
            Cache.Insert(key, expected);

            // Should just return cached value
            Product product = Cache.Get<Product>(key, GetNoneReadThruOptions());
            Assert.That(product, Is.EqualTo(expected));
        }

        // ✅ verify item is obtained from DS if ReadThruForced & item IS present (forced refresh)
        [Test]
        public void VerifyItemObtainedFromDSWithReadThruForcedEvenIfInCache()
        {
            string key = GetRandomKey();

            // Preload cache with stale product
            Product stale = new Product { Id = -1, Name = "Stale", Price = 0 };
            Cache.Insert(key, stale);

            // Force readThru
            Product freshProduct = Cache.Get<Product>(key, GetReadThruForcedOptions());

            // Should return updated DS value
            VerifyItemObtainedFromReadThru(key, freshProduct);

            // Ensure cache was refreshed with fresh value
            Product fromCache = Cache.Get<Product>(key);
            VerifyItemObtainedFromReadThru(key, fromCache);

            Assert.That(stale, Is.Not.EqualTo(fromCache));
        }

        // ✅ verify item is obtained from DS if ReadThruForced & NOT present in cache
        [Test]
        public void VerifyItemObtainedFromDSWithReadThruForcedWhenMissing()
        {
            string key = GetRandomKey();

            Product product = Cache.Get<Product>(key, GetReadThruForcedOptions());
            VerifyItemObtainedFromReadThru(key, product);

            // Also ensure it was inserted into cache after fetching
            Product fromCache = Cache.Get<Product>(key);
            VerifyItemObtainedFromReadThru(key, fromCache);
        }

        // ✅ verify proper exception if DS returns NULL
        [Test]
        public void VerifyExceptionWhenDataSourceReturnsNull()
        {
            string key = ReadThruCacheCommunication.ReadThruNullKey;

            Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<Product>(key, GetReadThruOptions());
            }, "Should throw exception when DS returns null product.");
        }

        // ✅ verify proper exception if DS itself throws exception
        [Test]
        public void VerifyExceptionWhenDataSourceThrowsException()
        {
            string key = ReadThruCacheCommunication.ReadThruExceptionKey; 

            Assert.Throws<OperationFailedException>(() =>
            {
                _ = Cache.Get<Product>(key, GetReadThruOptions());
            }, "Should propagate exception thrown by data source.");
        }
    }
    
}

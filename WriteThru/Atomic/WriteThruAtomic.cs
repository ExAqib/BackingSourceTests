using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Exceptions;
using Common;
using Common.WriteThru;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.WriteThru.Atomic
{
    //TODO: Can write more test cases for Remove api. I am assuming that if all cases for Insert and Add have passed , and one case of Remove hasn't thrown any invalid exception , it means all cases of remove are passed.
    [TestFixture]
    class WriteThruAtomic : WriteThruBase
    {
        string _key;
        Product _product;

        public WriteThruAtomic() : base() { }

        [SetUp]
        public void Setup()
        {
            Cache.Clear();
            _key = GetRandomKey();
            _product = Util.GetProductForCache(_key);
            CacheItem = GetCacheItem(_product);
        }

        [Test]
        public void WriteThru_WhenItemNotInCache_WritesToDataSource()
        {
            Cache.Add(_key, CacheItem, GetWriteThruOptions());
            // No Exception thrown → WriteThru was successful
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void WriteThru_RemoveFromCache_ItemRemovedFromDataSource(string mode)
        {
            string updatedKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForRemovedFromCache);

            Cache.Add(updatedKey, CacheItem);
            Cache.Remove(updatedKey, null, null, GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);

            var item = Cache.Get<Product>(_key);

            Assert.That(item, Is.Null);
        }


        [TestCase(WriteThru, true)]
        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_ProvideInvalidWriteThruOption_ExceptionThrown(string mode,bool preAdd = false)
        {
            var invalidWriteThruOptions = GetWriteThruOptionsWithWrongProviderName(mode);

            var ex = Assert.Throws<ConfigurationException>(() =>
            {
                if (preAdd)
                {
                    Cache.Insert(_key, CacheItem, invalidWriteThruOptions);
                }
                else
                    Cache.Add(_key, CacheItem, invalidWriteThruOptions);
            });

            var debugItem = Cache.Get<object>(_key);

            Assert.Multiple(() =>
            {
                Assert.That(ex.Message, Does.Contain(InvalidWriteThruProviderExceptionMessage));
            });
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_UpdateInCacheFromDataSourceWithDefalutProvider_ItemUpdatedInCache(string mode,bool preAdd = false)
        {
            string updatedKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForUpdateInCache);

            var defaultWriteThruOptions = GetWriteThruOptionsWithDefaultProviderName(mode);

            if (preAdd)
            {
                Cache.Add(updatedKey , CacheItem);
                Cache.Insert(updatedKey, CacheItem, defaultWriteThruOptions);
            }
            else
                Cache.Add(updatedKey, CacheItem, defaultWriteThruOptions);

            WaitForWriteBehindCompletionIfNeeded(mode);

            var item = Cache.Get<Product>(_key);

            Assert.That(item, Is.Not.Null);
            VerifyItemObtainedFromBackingSource(_key, item);
            var previousValue = CacheItem.GetValue<Product>();
            Assert.That(previousValue, Is.Not.EqualTo(item)); // PreviousValue was from cache, item is from data source
        }


        [TestCase(WriteThru)]
        [TestCase(WriteThru,true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind,true)]
        public void WriteThruWithInsert_UpdateInCacheFromDataSource_ItemUpdatedInCache(string mode,bool preAdd = false)
        {
            string updatedKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForUpdateInCache);

            Act(mode, preAdd, updatedKey);

            WaitForWriteBehindCompletionIfNeeded(mode);

            var item = Cache.Get<Product>(_key);

            Assert.That(item, Is.Not.Null);
            VerifyItemObtainedFromBackingSource(_key, item);
            var previousValue = CacheItem.GetValue<Product>();
            Assert.That(previousValue, Is.Not.EqualTo(item));
        }


        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_UpdateInCacheFromDataSource_ItemUpdatedInCache(string mode,bool preAdd = false)
        {
            string updatedKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForUpdateInCache);
            
            Act(mode, preAdd, updatedKey);

            WaitForWriteBehindCompletionIfNeeded(mode);

            var item = Cache.Get<Product>(_key);

            Assert.That(item, Is.Not.Null);
            VerifyItemObtainedFromBackingSource(_key, item);
            var previousValue = CacheItem.GetValue<Product>();
            Assert.That(previousValue, Is.Not.EqualTo(item));
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_WhenStatusIsSuccess_CacheRemainsUnchanged(string mode, bool preAdd = false)
        {
            var successKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForSuccess);

            Act(mode, preAdd, successKey);
            WaitForWriteBehindCompletionIfNeeded(mode);

            var resultItem = Cache.Get<Product>(successKey);

            Assert.That(resultItem, Is.Not.Null, "Item should remain in cache for Success.");
            Assert.That(resultItem, Is.EqualTo(CacheItem.GetValue<Product>()), "Cache should remain unchanged for Success.");
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_WhenStatusIsFailure_ItemIsRemovedFromCache(string mode, bool preAdd = false)
        {
            var failureKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForFailure);
            Act(mode,preAdd,failureKey);

            WaitForWriteBehindCompletionIfNeeded(mode);

            var resultItem = Cache.Get<Product>(failureKey);

            Assert.That(resultItem, Is.Null, "Item should be removed from cache on Failure.");
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_WhenStatusIsFailureRetry_ItemRemainsInCache(string mode, bool preAdd = false)
        {
            var retryKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForFailureRetry);

            Act(mode, preAdd, retryKey);
            WaitForWriteBehindCompletionIfNeeded(mode);

            var resultItem = Cache.Get<Product>(retryKey);

            Assert.That(resultItem, Is.Not.Null, "Item should remain in cache for FailureRetry.");
            Assert.That(resultItem, Is.EqualTo(CacheItem.GetValue<Product>()), "Cache content should remain unchanged for retry.");
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_WhenStatusIsFailureDontRemove_ItemIsNotRemoved(string mode, bool preAdd = false)
        {
            var dontRemoveKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForFailureDontRemove);

            Act(mode, preAdd, dontRemoveKey);
            WaitForWriteBehindCompletionIfNeeded(mode);

            var resultItem = Cache.Get<Product>(dontRemoveKey);

            Assert.That(resultItem, Is.Not.Null, "Item should not be removed for FailureDontRemove.");
            Assert.That(resultItem, Is.EqualTo(CacheItem.GetValue<Product>()), "Cache should remain unchanged for FailureDontRemove.");
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_WhenProviderThrowsException_ExceptionPropagated(string mode, bool preAdd = false)
        {
            var exKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForThrowException);

            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                if (preAdd)
                {
                    Cache.Add(exKey, CacheItem);
                    Cache.Insert(exKey, CacheItem, GetWriteThruOptions(mode));
                }
                else
                    Cache.Add(exKey, CacheItem, GetWriteThruOptions(mode));
            });

            Assert.Multiple(() =>
            {
                Assert.That(ex.Message, Does.Contain(WriteThruCommunication.ExceptionMessage));
                Assert.That(Cache.Get<Product>(exKey), Is.Null);
            });
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThru_WhenProviderReturnsErrorMessage_ErrorIsLogged(string mode, bool preAdd = false)
        {
            var errorKey = UpdateKeyWith(_key, WriteThruCommunication.KeyForErrorMessage);

            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                Cache.Add(errorKey, CacheItem, GetWriteThruOptions(mode));
                if (preAdd)
                {
                    Cache.Add(errorKey, CacheItem);
                    Cache.Insert(errorKey, CacheItem, GetWriteThruOptions(mode));
                }
                else
                    Cache.Add(errorKey, CacheItem, GetWriteThruOptions(mode));
            });

            Assert.That(ex.Message, Does.Contain(WriteThruCommunication.ErrorMessage));
        }

       

    }
}

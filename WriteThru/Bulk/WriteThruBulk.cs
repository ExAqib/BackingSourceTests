using Alachisoft.NCache.Runtime.Exceptions;
using Common.WriteThru;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NCache.Client;

namespace BackingSourceTests.WriteThru.Bulk
{

    // ALL TEST CASES ARE TESTED EXCEPT FOT ALL BULK WRITE THRU CASES. THIS IS BECAUSE OTHER TEAM MEMBER WAS COMPLETING THE BULK WRITE THRU FEATURE.

    [TestFixture]
    class WriteThruBulk : WriteThruBulkBase
    {
        private Dictionary<string, CacheItem> _items;
        private Dictionary<string, Product> _products;
        private IDictionary<string, Exception>? Result;

        public WriteThruBulk() : base() { }

        [SetUp]
        public void Setup()
        {
            Cache.Clear();
            _items = CreateSampleItems(100);  
            _products = _items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetValue<Product>());
            Result = null;
        }


        protected override object BulkAct(string mode, bool preAdd, Dictionary<string, CacheItem> items)
        {
            Result = base.BulkAct(mode, preAdd, items) as IDictionary<string,Exception>; 
            return Result;
        }

        private void VerifyCacheUnchanged(Dictionary<string, CacheItem> expectedItems)
        {
            foreach (var kvp in expectedItems)
            {
                var resultItem = Cache.Get<Product>(kvp.Key);
                Assert.That(resultItem, Is.Not.Null, $"Item [{kvp.Key}] should remain in cache.");
                Assert.That(resultItem, Is.EqualTo(kvp.Value.GetValue<Product>()), $"Cache content mismatch for [{kvp.Key}].");
            }
        }

        private void VerifyCacheUpdated(Dictionary<string, CacheItem> updatedItems)
        {
            foreach (var kvp in updatedItems)
            {
                var item = Cache.Get<Product>(kvp.Key);
                Assert.That(item, Is.Not.Null, $"Item [{kvp.Key}] missing after update.");
                VerifyItemObtainedFromBackingSource(kvp.Key, item);

                var previousValue = kvp.Value.GetValue<Product>();
                Assert.That(previousValue, Is.Not.EqualTo(item), $"Expected updated value for [{kvp.Key}].");
            }
        }

        private void VerifyCacheRemoved(Dictionary<string, CacheItem> removedItems)
        {
            foreach (var kvp in removedItems)
                Assert.That(Cache.Get<Product>(kvp.Key), Is.Null, $"Item [{kvp.Key}] should be removed.");
        }

        private void VerifyAllResultIsException()
        {
            if (Result == null)
                throw new ArgumentNullException(nameof(Result));

            foreach (var kvp in Result)
                Assert.That(kvp.Value, Is.TypeOf<Exception>(), $"Item [{kvp.Key}] should be exception. But is is :{kvp.Value}");
        }

        // ✅ TEST CASES

        [Test]
        public void WriteThruBulk_WhenItemsNotInCache_WritesToDataSource()
        {
            Cache.AddBulk(_items, GetWriteThruOptions());
            // No exception → Bulk WriteThru successful
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void WriteThruBulk_RemoveFromCache_ItemsRemovedFromDataSource(string mode)
        {
            var removeItems = TransformKeys(_items, WriteThruCommunication.KeyForRemovedFromCache);

            Cache.AddBulk(removeItems);
            Cache.RemoveBulk(removeItems.Keys.ToArray(), GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);
            VerifyCacheRemoved(removeItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_ProvideInvalidWriteThruOption_ExceptionThrown(string mode, bool preAdd = false)
        {
            var invalidOptions = GetWriteThruOptionsWithWrongProviderName(mode);

            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                if (preAdd)
                {
                    Cache.AddBulk(_items);
                    Cache.InsertBulk(_items, invalidOptions);
                }
                else
                    Cache.AddBulk(_items, invalidOptions);
            });

            Assert.Multiple(() =>
            {
                Assert.That(ex.Message, Does.Contain(InvalidWriteThruProviderExceptionMessage));
                VerifyCacheRemoved(_items); // should not remain in cache
            });
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_UpdateFromDataSourceWithDefaultProvider_ItemsUpdated(string mode, bool preAdd = false)
        {
            var updatedItems = TransformKeys(_items, WriteThruCommunication.KeyForUpdateInCache);
            var defaultOptions = GetWriteThruOptionsWithDefaultProviderName(mode);

            if (preAdd)
            {
                Cache.AddBulk(updatedItems);
                Cache.InsertBulk(updatedItems, defaultOptions);
            }
            else
                Cache.AddBulk(updatedItems, defaultOptions);

            WaitForWriteBehindCompletionIfNeeded(mode);
            VerifyCacheUpdated(updatedItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_WhenStatusIsSuccess_CacheRemainsUnchanged(string mode, bool preAdd = false)
        {
            var successItems = TransformKeys(_items, WriteThruCommunication.KeyForSuccess);

            BulkAct(mode, preAdd, successItems);
            WaitForWriteBehindCompletionIfNeeded(mode);

            VerifyCacheUnchanged(successItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_WhenStatusIsFailure_ItemsRemoved(string mode, bool preAdd = false)
        {
            var failureItems = TransformKeys(_items, WriteThruCommunication.KeyForFailure);

            BulkAct(mode, preAdd, failureItems);
            WaitForWriteBehindCompletionIfNeeded(mode);

            VerifyCacheRemoved(failureItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_WhenStatusIsFailureRetry_ItemsRemainInCache(string mode, bool preAdd = false)
        {
            var retryItems = TransformKeys(_items, WriteThruCommunication.KeyForFailureRetry);

            BulkAct(mode, preAdd, retryItems);
            WaitForWriteBehindCompletionIfNeeded(mode);

            VerifyCacheUnchanged(retryItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_WhenStatusIsFailureDontRemove_ItemsNotRemoved(string mode, bool preAdd = false)
        {
            var dontRemoveItems = TransformKeys(_items, WriteThruCommunication.KeyForFailureDontRemove);

            BulkAct(mode, preAdd, dontRemoveItems);
            WaitForWriteBehindCompletionIfNeeded(mode);

            VerifyCacheUnchanged(dontRemoveItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_WhenProviderThrowsException_ExceptionPropagated(string mode, bool preAdd = false)
        {
            var exceptionItems = TransformKeys(_items, WriteThruCommunication.KeyForThrowException);

            
            BulkAct(mode, preAdd, exceptionItems);

            VerifyAllResultIsException();
            VerifyCacheRemoved(exceptionItems);

        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_WhenProviderReturnsErrorMessage_ErrorIsLogged(string mode, bool preAdd = false)
        {
            // May be this test case needs to be fixed. 
            var errorItems = TransformKeys(_items, WriteThruCommunication.KeyForErrorMessage);

            var ex = Assert.Throws<OperationFailedException>(() =>
            {
                BulkAct(mode, preAdd, errorItems);
            });

            Assert.That(ex.Message, Does.Contain(WriteThruCommunication.ErrorMessage));
        }
    }

}

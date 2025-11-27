using Alachisoft.NCache.Client;
using BackingSourceTests.ReadThru;
using BackingSourceTests.WriteThru.Bulk;
using Common;
using Common.WriteThru;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ZeeChange
{
    [TestFixture]
    internal class WriteThruZeeChangeCases: WriteThruBulkBase 
    {
        private Dictionary<string, CacheItem> _items;
        private Dictionary<string, Product> _products;
        private IDictionary<string, Exception>? Result;
        private int unAffectedItemCount = 10000;
        public WriteThruZeeChangeCases() : base() { }

        [SetUp]
        public void Setup()
        {
            Cache.Clear();
            StopSecondNode();

            Cache.InsertBulk(CreateSampleItems(10000));
            _items = CreateSampleItems(unAffectedItemCount);
            _products = _items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetValue<Product>());
            Result = null;
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void WriteThruBulk_ZeeChangeWriteThruFailure_ItemsRemovedFromCache(string mode)
        {
            var zeeChangeItems = TransformKeys(_items, WriteThruCommunication.KeyForZeeChangeCacheWriteThruFailure);

            var addBulkRes = Cache.AddBulk(zeeChangeItems);
            AsyncStartSecondNode();
            var inserBulkRes = Cache.InsertBulk(zeeChangeItems, GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);
            VerifyItemsRemovedFromCache(zeeChangeItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void WriteThruBulk_ZeeChangeWhenStatusIsSuccess_CacheRemainsUnchanged(string mode)
        {
            var successItems = TransformKeys(_items, WriteThruCommunication.KeyForZeeChangeSuccess);

            var addBulkRes = Cache.AddBulk(successItems, GetWriteThruOptions(mode));
            AsyncStartSecondNode();

            WaitForWriteBehindCompletionIfNeeded(mode);           

            VerifyCacheUnchanged(successItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void WriteThruBulk_ZeeChangeRemoveFromCache_ItemsRemovedFromDataSource(string mode)
        {
            var removeItems = TransformKeys(_items, WriteThruCommunication.KeyForZeeChangeRemoveFromCache);

            Cache.AddBulk(removeItems);
            AsyncStartSecondNode();
            Cache.RemoveBulk(removeItems.Keys.ToArray(), GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);
            VerifyItemsRemovedFromCache(removeItems);
        }


        [TestCase(WriteThru, WriteThruCommunication.KeyForZeeChangeFailureRetry)]
        [TestCase(WriteThru, WriteThruCommunication.KeyForZeeChangeFailureDontRemove)]
        [TestCase(WriteBehind, WriteThruCommunication.KeyForZeeChangeFailureRetry)]
        [TestCase(WriteBehind, WriteThruCommunication.KeyForZeeChangeFailureDontRemove)]
        public void WriteThruBulk_ZeeChangeWhenStatusIsFailureRetryORFailureDontRemove_ItemsRemainInCache(string mode,string key)
        {
            var retryItems = TransformKeys(_items, key);

            AsyncStartSecondNode();
            Cache.AddBulk(retryItems,GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);
          
            VerifyCacheUnchanged(retryItems);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulk_ZeeChangeUpdateFromDataSource_ItemsUpdated(string mode, bool preAdd = false)
        {
            var updatedItems = TransformKeys(_items, WriteThruCommunication.KeyForZeeChangeUpdateInCache);
            var defaultOptions = GetWriteThruOptionsWithDefaultProviderName(mode);

            Cache.AddBulk(updatedItems);

            AsyncStartSecondNode();
            Cache.InsertBulk(updatedItems, defaultOptions);

            WaitForWriteBehindCompletionIfNeeded(mode);
            VerifyCacheUpdatedByBackingSource(updatedItems);
        }

        internal override void WaitForWriteBehindCompletionIfNeeded(string mode)
        {
         
            if (mode.Equals(WriteBehind))
            {
                TestContext.WriteLine($"ZeeChange Waiting for {WriteThruCommunication.ZEE_CHANGE_WAIT_TIME_IN_SECONDS} seconds for WriteBehind completion.");
                Thread.Sleep(TimeSpan.FromSeconds(WriteThruCommunication.ZEE_CHANGE_WAIT_TIME_IN_SECONDS));
            }

            base.WaitForWriteBehindCompletionIfNeeded(mode);
        }

        private void VerifyItemsRemovedFromCache(Dictionary<string, CacheItem> removedItems)
        {
            var getBulkResult = Cache.
                GetBulk<Product>(removedItems.Keys);

            Assert.That(getBulkResult, Is.Empty, "GetBulk result should be empty."); 
        }
       
    }
}

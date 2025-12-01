using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Common.WriteThru;

namespace BackingSourceTests.WriteThru.Bulk
{
    [TestFixture]
    class WriteThruBulkMeta : WriteThruBulkBase
    {
        private Dictionary<string, CacheItem> _items;

        [SetUp]
        public void Setup()
        {
            Cache.Clear();
            _items = CreateSampleItems(100);
            File.WriteAllText(WriteThruCommunication.WRITE_THRU_SHARED_FILE, string.Empty);
        }


        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulkMeta_WithAbsoluteExpiration_SuccesfullyReceivedByProvider(string mode, bool preAdd = false)
        {
            // ✅ Arrange 
            SetAbsoluteExpirationInItems();
            var caheItemsWithExpiationInfo = TransformKeys(_items, WriteThruCommunication.KeyForAbsoluteExpiration);

            // ✅ Act
            BulkAct(mode, preAdd, caheItemsWithExpiationInfo);
            WaitForWriteBehindCompletionIfNeeded(mode);
            Thread.Sleep(TimeSpan.FromSeconds(WriteThruFileWriteWaitTime));

            // ✅ Assert
            Assert.That(VerifyMessageWrittenByProvider(WriteThruCommunication.AbsoluteExpirationVerifiedMessage), Is.True, "WriteThruBulk Provider did not write absolute expiration info to shared file.");

            var bulkResult = Cache.GetCacheItemBulk(_items.Keys);
            foreach (var kvp in bulkResult)
            {
                AssertForWriteThruAbsoluteExpiration(kvp.Key, kvp.Value);
            }
        }



        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruBulkMeta_WithBulkMetaInfo_MetaSuccessfulyReceivedByWriteThru(string mode, bool preAdd = false)
        {
            // ✅ Arrange 
            SetBulkMetaInfoInItems();
            var caheItemsWithBulkMetaInfo = TransformKeys(_items, WriteThruCommunication.KeyForVerifyingMetaInfoBulk);

            // ✅ Act
            BulkAct(mode, preAdd, caheItemsWithBulkMetaInfo);
            WaitForWriteBehindCompletionIfNeeded(mode);
            Thread.Sleep(TimeSpan.FromSeconds(WriteThruFileWriteWaitTime));

            // ✅ Assert
            Assert.That(VerifyMessageWrittenByProvider(WriteThruCommunication.BulkMetaVerifiedMessage), Is.True, "WriteThruBulk Provider did not write absolute expiration info to shared file.");

            var bulkResult = Cache.GetCacheItemBulk(_items.Keys);
            foreach (var kvp in bulkResult)
            {
                AssertWriteThruMetaBulk(kvp.Key, kvp.Value);
            }
        }


        #region Helper Methods  
        private void SetBulkMetaInfoInItems()
        {
            foreach (var item in _items)
            {
                SetBulkMetaInfoForWriteThruVerification(item.Value);
            }
        }

        private void SetAbsoluteExpirationInItems()
        {
            foreach (var item in _items)
            {
                SetAbsoluteExpirationForWriteThruVerification(item.Value);
            }
        }
       
        #endregion
    }
}

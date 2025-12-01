using Alachisoft.NCache.Runtime.Caching;
using Common;
using Common.WriteThru;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.WriteThru.DataStructures
{
    [TestFixture]
    internal class WriteThruDataStructure: WriteThruDataStructureBase
    {
        [SetUp]
        public void SetUp()
        {
            Cache.Clear();
            File.WriteAllText(WriteThruCommunication.WRITE_THRU_SHARED_FILE, string.Empty);
        }

        // -----------------------------
        //      LIST TESTS
        // -----------------------------

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void CreateListWithMetaInfo_WhenNotPresentInCache_WriteThruProviderCalled(string mode)
        {
            string key = WriteThruCommunication.DataStructureKeyForList;

            var list = Cache.DataTypeManager.CreateList<Product>(key, GetDataTypeAttributes(key), GetWriteThruOptions(mode));
            WaitForWriteBehindCompletionIfNeeded(mode);

            Assert.That(list, Is.Not.Null, "List obtained from write thru provider is null.");
            VerifyDataTypeAttributesAreSame(key);
            VerifyMessageWrittenByProvider(key);           
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void AddItemInList_WithWriteThru_WriteThruProviderCalled(string mode)
        {
            string key = WriteThruCommunication.DataStructureKeyForList;

            Alachisoft.NCache.Client.DataTypes.Collections.IDistributedList<Product>? list = Cache.DataTypeManager.CreateList<Product>(key,null,GetWriteThruOptions(mode));
            list.Add(Util.GetProductForCache(key));

            WaitForWriteBehindCompletionIfNeeded(mode);
                   
            VerifyMessagesWrittenByProvider(key,2);

        }

        // -----------------------------
        //      DICTIONARY TESTS
        // -----------------------------

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void CreateDictionaryWithMetaInfo_WhenNotPresentInCache_WriteThruProviderCalled(string mode)
        {
            string key = WriteThruCommunication.DataStructureKeyForDictionary;

            var dict = Cache.DataTypeManager.CreateDictionary<string, Product>(
                key,
                GetDataTypeAttributes(key),
                GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);

            Assert.That(dict, Is.Not.Null);
            VerifyDataTypeAttributesAreSame(key);
            VerifyMessageWrittenByProvider(key);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void AddItemInDictionary_WithWriteThru_WriteThruProviderCalled(string mode)
        {
            string key = WriteThruCommunication.DataStructureKeyForDictionary;

            var dict = Cache.DataTypeManager.CreateDictionary<string, Product>(
                key,
                null,
                GetWriteThruOptions(mode));

            dict.Add("item-1", Util.GetProductForCache(key));

            WaitForWriteBehindCompletionIfNeeded(mode);

            // 1 = dictionary create  
            // 1 = item insert  
            VerifyMessagesWrittenByProvider(key, 2);
        }


        // -----------------------------
        //      HASHSET TESTS
        // -----------------------------

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void CreateHashSetWithMetaInfo_WhenNotPresentInCache_WriteThruProviderCalled(string mode)
        {
            string key = WriteThruCommunication.DataStructureKeyForSet;

            var set = Cache.DataTypeManager.CreateHashSet<string>(
                key,
                GetDataTypeAttributes(key),
                GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);

            Assert.That(set, Is.Not.Null);
            VerifyDataTypeAttributesAreSame(key);
            VerifyMessageWrittenByProvider(key);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void AddItemInHashSet_WithWriteThru_WriteThruProviderCalled(string mode)
        {
            string key = WriteThruCommunication.DataStructureKeyForSet;

            var set = Cache.DataTypeManager.CreateHashSet<string>(
                key,
                null,
                GetWriteThruOptions(mode));

            set.Add("Test Item");

            WaitForWriteBehindCompletionIfNeeded(mode);

            VerifyMessagesWrittenByProvider(key, 2);
        }

        // -----------------------------
        //      QUEUE TESTS
        // -----------------------------

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void CreateQueueWithMetaInfo_WhenNotPresentInCache_WriteThruProviderCalled(string mode)
        {
            string key = WriteThruCommunication.DataStructureKeyForQueue;

            var queue = Cache.DataTypeManager.CreateQueue<Product>(
                key,
                GetDataTypeAttributes(key),
                GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);

            Assert.That(queue, Is.Not.Null);
            VerifyDataTypeAttributesAreSame(key);
            VerifyMessageWrittenByProvider(key);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void EnqueueItemInQueue_WithWriteThru_WriteThruProviderCalled(string mode)
        {
            string key = WriteThruCommunication.DataStructureKeyForQueue;

            var queue = Cache.DataTypeManager.CreateQueue<Product>(
                key,
                null,
                GetWriteThruOptions(mode));

            queue.Enqueue(Util.GetProductForCache(key));

            WaitForWriteBehindCompletionIfNeeded(mode);

            VerifyMessagesWrittenByProvider(key, 2);
        }
    }
}

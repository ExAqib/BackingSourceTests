using BackingSourceTests.ReadThru;
using Common;

namespace BackingSourceTests.ZeeChange
{

    [TestFixture]
    internal class ReadThruDataStructure : ReadThruDataStructureBase
    {
        [SetUp]
        public void SetUp()
        {
            Cache.Clear();
            // Forcing ZEE change scenario
            StopSecondNode();

            PerformAddBulk(GetRandomKeysForReadThruBulk(10000));

            // Ensures that at least some DS items are included in state transfer
            AddFewItemsOfAllDataStructures(10000);
        }


        [Test]
        public void GetListFromDataSource_WhenNotPresentInCache_ListIsObtainedFromDB()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForList);

            AsyncStartSecondNode();
            var list = Cache.DataTypeManager.GetList<string>(key, GetReadThruOptions());

            VerifyDSListObtainedFromBackingSource(key, list);
        }


        [Test]
        public void GetDictionary_FromDB_WhenMissing_DuringZeeChange()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForDictionary);

            AsyncStartSecondNode();

            var dict = Cache.DataTypeManager.GetDictionary<string, object>(key, GetReadThruOptions());
            VerifyDSDictionaryObtainedFromBackingSource(key, dict);
        }

        [Test]
        public void GetHashSet_FromDB_WhenMissing_DuringZeeChange()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForSet);

            AsyncStartSecondNode();

            var set = Cache.DataTypeManager.GetHashSet<string>(key, GetReadThruOptions());
            VerifyDSHashSetObtainedFromBackingSource(key, set);
        }

        [Test]

        public void GetQueue_FromDB_WhenMissing_DuringZeeChange()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForQueue);

            AsyncStartSecondNode();

            var queue = Cache.DataTypeManager.GetQueue<string>(key, GetReadThruOptions());

            VerifyDSQueueObtainedFromBackingSource(key, queue);
        }

        [Test]
        public void GetCounter_FromDB_WhenMissing_DuringZeeChange()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForCounter);

            AsyncStartSecondNode();

            var counter = Cache.DataTypeManager.GetCounter(key, GetReadThruOptions());

            VerifyDSCounterObtainedFromBackingSource(counter);
        }

        // ================================================================
        // HELPER METHODS (FULL IMPLEMENTATION)
        // ================================================================

        private void AddFewItemsOfAllDataStructures(int count)
        {
            var list = Cache.DataTypeManager.GetList<string>("ZT_List", GetReadThruOptions());
            var dictionary = Cache.DataTypeManager.GetDictionary<string, string>("ZT_Dict", GetReadThruOptions());
            var set = Cache.DataTypeManager.GetHashSet<string>("ZT_Set", GetReadThruOptions());
            //UNCOMMENT WHEN Q BUG FIXED
            var q = Cache.DataTypeManager.GetQueue<string>("ZT_Queue", GetReadThruOptions());
            var counter = Cache.DataTypeManager.GetCounter("ZT_Counter", GetReadThruOptions());

            for (int i = 0; i < count; i++)
            {
                //string key = $"ZeeChange_Testing_Key_{i}";
                string key = i.ToString();
                list.Add(key);
                dictionary.Add(key, key);
                set.Add(key);
                q.Enqueue(key);
                counter.Increment();
            }

        }
    }
}
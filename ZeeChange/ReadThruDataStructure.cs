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
            StopNode();

            // Ensures that at least some DS items are included in state transfer
            AddItemsForDataStructreZeeChange(10000);
        }


        [Test]
        public void GetListFromDataSource_WhenNotPresentInCache_ListIsObtainedFromDB()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForList);

            AsyncStartNode();
            var list = Cache.DataTypeManager.GetList<string>(key, GetReadThruOptions());

            VerifyDSListObtainedFromBackingSource(key, list);
        }


        [Test]
        public void GetDictionary_FromDB_WhenMissing_DuringZeeChange()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForDictionary);

            AsyncStartNode();

            var dict = Cache.DataTypeManager.GetDictionary<string, object>(key, GetReadThruOptions());
            VerifyDSDictionaryObtainedFromBackingSource(key, dict);
        }

        [Test]
        public void GetHashSet_FromDB_WhenMissing_DuringZeeChange()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForSet);

            AsyncStartNode();

            var set = Cache.DataTypeManager.GetHashSet<string>(key, GetReadThruOptions());
            VerifyDSHashSetObtainedFromBackingSource(key, set);
        }

        [Test]

        public void GetQueue_FromDB_WhenMissing_DuringZeeChange()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForQueue);

            AsyncStartNode();

            var queue = Cache.DataTypeManager.GetQueue<string>(key, GetReadThruOptions());

            VerifyDSQueueObtainedFromBackingSource(key, queue);
        }

        [Test]
        public void GetCounter_FromDB_WhenMissing_DuringZeeChange()
        {
            string key = Util.GetAppendedZeeChangeKey(ReadThruCacheCommunication.DataStructureKeyForCounter);

            AsyncStartNode();

            var counter = Cache.DataTypeManager.GetCounter(key, GetReadThruOptions());

            VerifyDSCounterObtainedFromBackingSource(counter);
        }        
    }
}
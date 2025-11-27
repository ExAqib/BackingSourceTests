using Alachisoft.NCache.Runtime.Caching;
using BackingSourceTests.ReadThru;
using Common;

namespace BackingSourceTests.ReadThru.DataStructures
{

    [TestFixture]
    internal class DataStructures : ReadThruDataStructureBase
    {
        [SetUp]
        public void SetUp()
        {
            Cache.Clear();
        }

        // -----------------------------
        //      LIST TESTS
        // -----------------------------
        [Test]
        public void GetListFromDataSource_WhenNotPresentInCache_ListIsObtainedFromDB()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForList;

            var list = Cache.DataTypeManager.GetList<string>(key, GetReadThruOptions());
            
            
            VerifyDSListObtainedFromBackingSource(key, list);
        }
        
        [Test]
        public void GetList_WhenPresentInCache_ShouldNOTCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForList;

            // Seed into cache manually
            var listIntoCache = Cache.DataTypeManager.CreateList<string>(key);
            listIntoCache.Add("cached-value");

            // This should return cached value immediately
            var list = Cache.DataTypeManager.GetList<string>(key, GetReadThruOptions());

            Assert.That(list, Has.Count.EqualTo(1));
            Assert.That(list[0], Is.EqualTo("cached-value"));
        }

        [Test]
        public void GetListWithReadThruForced_WhenPresentInCache_ShouldCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForList;

            // Seed into cache manually
            var listIntoCache = Cache.DataTypeManager.CreateList<string>(key);
            listIntoCache.Add("cached-value");

            // This should return cached value immediately
            var list = Cache.DataTypeManager.GetList<string>(key, GetReadThruOptions());

            Assert.That(list, Has.Count.EqualTo(1));
            Assert.That(list[0], Is.EqualTo("cached-value"));

            // Now get with ReadThruForced option
            list = Cache.DataTypeManager.GetList<string>(key, GetReadThruForcedOptions());
            VerifyDSListObtainedFromBackingSource(key,list);

        }


        [Test]
        public void GetList_WithoutReadThruOptions_ReturnsNull()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForList;

            var list = Cache.DataTypeManager.GetList<string>(key);

            Assert.That(list, Is.Null, "List should be null when no backing-source options are provided.");
        }

        // -----------------------------
        //      DICTIONARY TESTS
        // -----------------------------
        [Test]
        public void GetDictionaryFromDataSource_WhenNotPresentInCache_DictionaryIsObtainedFromDB()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForDictionary;
            var dict = Cache.DataTypeManager.GetDictionary<string, object>(key, GetReadThruOptions());

            VerifyDSDictionaryObtainedFromBackingSource(key, dict);
        }        

        [Test]
        public void GetDictionary_WhenPresentInCache_ShouldNOTCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForDictionary;

            var existing = new Dictionary<string, object> { { "in-cache", 10 } };
            Cache.DataTypeManager.CreateDictionary<string, object>(key);
            Cache.DataTypeManager.GetDictionary<string, object>(key).Add("in-cache", 10);

            var dict = Cache.DataTypeManager.GetDictionary<string, object>(key, GetReadThruOptions());

            Assert.That(dict, Has.Count.EqualTo(1));
            Assert.That(dict.ContainsKey("in-cache"));
        }


        [Test]
        public void GetDictionaryWithReadThruForced_WhenPresentInCache_ShouldCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForDictionary;

            var existing = new Dictionary<string, object> { { "in-cache", 10 } };
            Cache.DataTypeManager.CreateDictionary<string, object>(key);
            Cache.DataTypeManager.GetDictionary<string, object>(key).Add("in-cache", 10);

            var dict = Cache.DataTypeManager.GetDictionary<string, object>(key, GetReadThruOptions());

            Assert.That(dict, Has.Count.EqualTo(1));
            Assert.That(dict.ContainsKey("in-cache"));

            // Now get with ReadThruForced option
            dict = Cache.DataTypeManager.GetDictionary<string, object>(key, GetReadThruForcedOptions());
            VerifyDSDictionaryObtainedFromBackingSource(key, dict);
        }


        [Test]
        public void GetDictionary_WithoutReadThruOptions_ReturnsNull()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForDictionary;

            var map = Cache.DataTypeManager.GetDictionary<string, string>(key);

            Assert.That(map, Is.Null);
        }

        // -----------------------------
        //      QUEUE TESTS
        // -----------------------------
       
        [Test]
        public void GetQueueFromDataSource_WhenNotPresentInCache_QueueIsObtainedFromDB()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForQueue;

            var queue = Cache.DataTypeManager.GetQueue<string>(key, GetReadThruOptions());

            VerifyDSQueueObtainedFromBackingSource(key, queue);
        }

        

        [Test]
        public void GetQueueWithReadThruForced_WhenPresentInCache_ShouldCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForQueue;

            var q = Cache.DataTypeManager.CreateQueue<string>(key);
            q.Enqueue("cached-value");

            var queue = Cache.DataTypeManager.GetQueue<string>(key, GetReadThruOptions());

            Assert.That(queue, Has.Count.EqualTo(1));
            Assert.That(queue.Dequeue(), Is.EqualTo("cached-value"));
        }


        [Test]
        public void GetQueue_WhenPresentInCache_ShouldNOTCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForQueue;

            var q = Cache.DataTypeManager.CreateQueue<string>(key);
            q.Enqueue("cached-value");

            var queue = Cache.DataTypeManager.GetQueue<string>(key, GetReadThruOptions());

            Assert.That(queue, Has.Count.EqualTo(1));
            Assert.That(queue.Dequeue(), Is.EqualTo("cached-value"));

            // Now get with ReadThruForced option
            queue = Cache.DataTypeManager.GetQueue<string>(key, GetReadThruForcedOptions());
            VerifyDSQueueObtainedFromBackingSource(key, queue);
        }

        [Test]
        public void GetQueue_WithoutReadThruOptions_ReturnsNull()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForQueue;

            var queue = Cache.DataTypeManager.GetQueue<string>(key);

            Assert.That(queue, Is.Null);
        }

        // -----------------------------
        //      SET TESTS
        // -----------------------------
        [Test]
        public void GetSetFromDataSource_WhenNotPresentInCache_SetIsObtainedFromDB()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForSet;

            var set = Cache.DataTypeManager.GetHashSet<string>(key, GetReadThruOptions());

            VerifyDSHashSetObtainedFromBackingSource(key, set);
        }
                

        [Test]
        public void GetSet_WhenPresentInCache_ShouldNOTCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForSet;

            var existing = new HashSet<string> { "cached-value" };
            var setIn = Cache.DataTypeManager.CreateHashSet<string>(key);
            setIn.Add("cached-value");

            var set = Cache.DataTypeManager.GetHashSet<string>(key, GetReadThruOptions());

            Assert.That(set, Has.Count.EqualTo(1));
            Assert.That(set.Contains("cached-value"));
        }

        [Test]
        public void GetSetWithReadThruForced_WhenPresentInCache_ShouldCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForSet;

            var existing = new HashSet<string> { "cached-value" };
            var setIn = Cache.DataTypeManager.CreateHashSet<string>(key);
            setIn.Add("cached-value");

            var set = Cache.DataTypeManager.GetHashSet<string>(key, GetReadThruOptions());

            Assert.That(set, Has.Count.EqualTo(1));
            Assert.That(set.Contains("cached-value"));

            // Now get with ReadThruForced option
            set = Cache.DataTypeManager.GetHashSet<string>(key, GetReadThruForcedOptions());
            VerifyDSHashSetObtainedFromBackingSource(key, set);
        }

        [Test]
        public void GetHashSet_WithoutReadThruOptions_ReturnsNull()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForSet;

            var set = Cache.DataTypeManager.GetHashSet<string>(key);

            Assert.That(set, Is.Null);
        }

        // -----------------------------
        //      COUNTER TESTS
        // -----------------------------
        [Test]
        public void GetCounterFromDataSource_WhenNotPresentInCache_CounterIsObtainedFromDB()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForCounter;

            var counter = Cache.DataTypeManager.GetCounter(key, GetReadThruOptions());

            VerifyDSCounterObtainedFromBackingSource(counter);
        }

        
        [Test]
        public void GetCounter_WhenPresentInCache_ShouldNOTCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForCounter;

            Cache.DataTypeManager.CreateCounter(key, 500);

            var cachedValue = Cache.DataTypeManager.GetCounter(key, GetReadThruOptions());

            Assert.That(cachedValue.Value, Is.EqualTo(500));
        }

        [Test]
        public void GetCounterWithReadThruForced_WhenPresentInCache_ShouldCallDataSource()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForCounter;

            Cache.DataTypeManager.CreateCounter(key, 500);

            var cachedValue = Cache.DataTypeManager.GetCounter(key, GetReadThruOptions());

            Assert.That(cachedValue.Value, Is.EqualTo(500));

            // Now get with ReadThruForced option
            var counter = Cache.DataTypeManager.GetCounter(key, GetReadThruForcedOptions());
            VerifyDSCounterObtainedFromBackingSource(counter);
        }

        [Test]
        public void GetCounter_WithoutReadThruOptions_ReturnsNull()
        {
            string key = ReadThruCacheCommunication.DataStructureKeyForCounter;

            var counter = Cache.DataTypeManager.GetCounter(key);

            Assert.That(counter, Is.Null);
        }

    }
}

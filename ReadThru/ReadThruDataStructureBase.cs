using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ReadThru
{
    internal class ReadThruDataStructureBase : ReadThruBase
    {
        public virtual void VerifyDSListObtainedFromBackingSource(string key, Alachisoft.NCache.Client.DataTypes.Collections.IDistributedList<string> list)
        {
            Assert.That(list, Is.Not.Null);
            Assert.That(list, Has.Count.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(key));
        }

        public virtual void VerifyDSDictionaryObtainedFromBackingSource(string key, Alachisoft.NCache.Client.DataTypes.Collections.IDistributedDictionary<string, object> dict)
        {
            Assert.Multiple(() =>
            {
                Assert.That(dict, Is.Not.Null);
                Assert.That(dict, Has.Count.EqualTo(1));
                Assert.That(dict.ContainsKey(key));
                Assert.That(dict[key], Is.EqualTo(key));
            });
        }

        public virtual void VerifyDSHashSetObtainedFromBackingSource(string key, Alachisoft.NCache.Client.DataTypes.Collections.IDistributedHashSet<string> set)
        {
            Assert.That(set, Is.Not.Null);
            Assert.That(set, Has.Count.EqualTo(1));
            Assert.That(set.Contains(key));
        }

        public virtual void VerifyDSQueueObtainedFromBackingSource(string key, Alachisoft.NCache.Client.DataTypes.Collections.IDistributedQueue<string> queue)
        {
            Assert.That(queue, Is.Not.Null);
            Assert.That(queue, Has.Count.EqualTo(1));
            Assert.That(queue.Dequeue(), Is.EqualTo(key));
        }

        public virtual void VerifyDSCounterObtainedFromBackingSource(Alachisoft.NCache.Client.DataTypes.Counter.ICounter counter)
        {
            Assert.That(counter, Is.Not.Null);
            Assert.That(counter.Value, Is.EqualTo(ReadThruCacheCommunication.DefaultCounterValue));
        }

        internal void VerifyMetaInfoInDataStructure(string key)
        {
            var cacheItem = Cache.GetCacheItem(key);
            ReadThruCacheCommunication.VerifyMetaInfoInProviderDataType(cacheItem, key);
        }
    }
}

using Alachisoft.NCache.Client;
using Alachisoft.NCache.Config.Dom;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Runtime.Caching;
using Common;
using Common.WriteThru;
using Quartz;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.WriteThru.Atomic
{
    internal class WriteThruMetaTestCases : WriteThruBase
    {
        public WriteThruMetaTestCases() : base() { }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            File.WriteAllText(WriteThruCommunication.WRITE_THRU_SHARED_FILE, string.Empty);
        }     


        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruMeta_WithBulkMetaInfo_MetaSuccessfulyReceivedByWriteThru(string mode, bool preAdd = false)
        {
            // ✅ Arrange  

            string metaKey = WriteThruCommunication.KeyForVerifyingMetaInfoBulk;       

            var writeThruOptions = GetWriteThruOptions(mode);

            // ✅ Act 

            if (preAdd)
            {
                Cache.Add(metaKey, GetProductForMetaVerificationBulk(metaKey));
                Cache.Insert(metaKey, GetProductForMetaVerificationBulk(metaKey), writeThruOptions);
            }
            else
                Cache.Add(metaKey, GetProductForMetaVerificationBulk(metaKey), writeThruOptions);

            WaitForWriteBehindCompletionIfNeeded(mode);
            Thread.Sleep(TimeSpan.FromSeconds(WriteThruFileWriteWaitTime));

            // ✅ Assert

            Assert.Multiple(() => 
            {
                Assert.That(VerifyBulkMetaInfoWritten(), Is.True, "WriteThru Provider did not write meta info to shared file.");
                Assert.That(Cache.GetCacheItem(metaKey), Is.Not.Null, "Cache item should not be null after WriteThru operation.");
                Assert.That(Cache.GetCacheItem(metaKey).Expiration, Is.Not.Null, "Cache item expiration should not be null after WriteThru operation.");
                Assert.That(Cache.GetCacheItem(metaKey).Expiration.Type, Is.EqualTo(ExpirationType.Sliding), "Cache item expiration should be sliding after WriteThru operation.");
                Assert.That(Cache.GetCacheItem(metaKey).Priority, Is.EqualTo(WriteThruCommunication.ItemPriority), "Cache item priority should not be changed after WriteThru operation.");
                Assert.That(Cache.GetCacheItem(metaKey).Tags?.First()?.TagName, Is.EqualTo(WriteThruCommunication.TagName), "Cache item tags should not be changed after WriteThru operation.");
                Assert.That(Cache.GetCacheItem(metaKey).NamedTags, Is.Not.Null, "Cache item named tags should not be null after WriteThru operation.");
            });
        }

        public bool VerifyBulkMetaInfoWritten()
        {
            var lines = File.ReadAllLines(WriteThruCommunication.WRITE_THRU_SHARED_FILE);

            if (lines == null || lines.Length == 0)            
                return false;
            

            var lastLine = lines.Last();
            return lastLine.Contains(WriteThruCommunication.BulkMetaVerifiedMessage);
        }

        private CacheItem GetProductForMetaVerificationBulk(string metaKey)
        {
            var product = Util.GetProductForCache(metaKey);
            CacheItem = GetCacheItem(product);

            CacheItem.Priority = WriteThruCommunication.ItemPriority;
            CacheItem.Expiration = new Expiration(ExpirationType.Sliding, TimeSpan.FromMinutes(WriteThruCommunication.SlidingExpirationTime));
            CacheItem.Tags = [new Tag(WriteThruCommunication.TagName)];

            var namedTagDictionary = new NamedTagsDictionary();
            namedTagDictionary.Add(WriteThruCommunication.NamedTagKey,WriteThruCommunication.NamedTagValue);
            CacheItem.NamedTags = namedTagDictionary;

            return CacheItem;
        }
    }
}

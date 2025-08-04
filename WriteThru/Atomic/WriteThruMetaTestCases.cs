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
      

        [SetUp]
        public void SetUp()
        {
            Cache.Clear();
            File.WriteAllText(WriteThruCommunication.WRITE_THRU_SHARED_FILE, string.Empty);
        }


        [TestCase(WriteThru)]
        [TestCase(WriteThru, true)]
        [TestCase(WriteBehind)]
        [TestCase(WriteBehind, true)]
        public void WriteThruMeta_WithAbsoluteExpiration_SuccesfullyReceivedByProvider(string mode, bool preAdd = false)
        {
            // ✅ Arrange  
            string metaKey = WriteThruCommunication.KeyForAbsoluteExpiration;
            var writeThruOptions = GetWriteThruOptions(mode);

            // ✅ Act 
            if (preAdd)
            {
                Cache.Add(metaKey, GetProductForAbsoluteExpirationVerification(metaKey));
                Cache.Insert(metaKey, GetProductForAbsoluteExpirationVerification(metaKey), writeThruOptions);
            }
            else
                Cache.Add(metaKey, GetProductForAbsoluteExpirationVerification(metaKey), writeThruOptions);

            WaitForWriteBehindCompletionIfNeeded(mode);
            Thread.Sleep(TimeSpan.FromSeconds(WriteThruFileWriteWaitTime));


            // ✅ Assert
            Assert.That(VerifyMessageWritten(WriteThruCommunication.AbsoluteExpirationVerifiedMessage), Is.True, "WriteThru Provider did not write absolute expiration info to shared file.");
            AssertForWriteThruAbsoluteExpiration(metaKey, Cache.GetCacheItem(metaKey));                      
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
            Assert.That(VerifyMessageWritten(WriteThruCommunication.BulkMetaVerifiedMessage), Is.True, "WriteThru Provider did not write meta info to shared file.");
            AssertWriteThruMetaBulk(metaKey, Cache.GetCacheItem(metaKey));

        }

        #region Helper Methods 

        public bool VerifyMessageWritten(string message)
        {
            var lines = File.ReadAllLines(WriteThruCommunication.WRITE_THRU_SHARED_FILE);

            if (lines == null || lines.Length == 0)
                return false;


            var lastLine = lines.Last();
            return lastLine.Contains(message);
        }       

        private CacheItem GetProductForMetaVerificationBulk(string metaKey)
        {
            var product = Util.GetProductForCache(metaKey);
            CacheItem = GetCacheItem(product);
            SetBulkMetaInfoForWriteThruVerification(CacheItem);         

            return CacheItem;
        }       
        #endregion
    }
}

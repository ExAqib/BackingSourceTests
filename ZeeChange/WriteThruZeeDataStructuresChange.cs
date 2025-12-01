using Alachisoft.NCache.Common.Monitoring;
using BackingSourceTests.WriteThru;
using BackingSourceTests.WriteThru.DataStructures;
using Common;
using Common.WriteThru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ZeeChange
{
    [TestFixture]
    internal class WriteThruZeeDataStructuresChange : WriteThruDataStructureBase
    {
        [SetUp]
        public void SetUp()
        {
            Cache.Clear();
            StopNode();
            AddItemsForDataStructreZeeChange(10000);
        }

        [TestCase(WriteThru)]
        [TestCase(WriteBehind)]
        public void CreateList_WithWriteThru_WriteThruCalled(string mode)
        {
            string key = Util.GetAppendedZeeChangeKey(WriteThruCommunication.DataStructureKeyForList);

            AsyncStartNode();
            var list = Cache.DataTypeManager.CreateList<Product>(key, GetDataTypeAttributes(key), GetWriteThruOptions(mode));

            WaitForWriteBehindCompletionIfNeeded(mode);

            Assert.That(list, Is.Not.Null, "List obtained from write thru provider is null.");
           // Assert.That(list[0], Is.EqualTo(Util.GetProductForBackingSource(key)));

            VerifyDataTypeAttributesAreSame(key);

            Assert.That(list, Is.Not.Null, "List obtained from write thru provider is null.");
            Assert.That(list, Has.Count.EqualTo(0), "List should be empty due to write thru failure during zee change.");
            //VerifyDataTypeAttributesAreSame(key);
        }

    }
}

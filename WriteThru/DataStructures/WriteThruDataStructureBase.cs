using Alachisoft.NCache.Client.DataTypes;
using Alachisoft.NCache.Runtime;
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
    internal class WriteThruDataStructureBase : WriteThruBase
    {
        public DataTypeAttributes GetDataTypeAttributes(string key)
        {
            ProviderDataTypeItem<IEnumerable> item = new ProviderDataTypeItem<IEnumerable>(new List<string> { key });
            ReadThruCacheCommunication.AddMetaInfoInProviderDataType(item,key);

            var options = new DataTypeAttributes
            {
                Tags = item.Tags,
                Expiration = item.Expiration,
                Priority = item.ItemPriority,
                Group = item.Group,
                ResyncOptions = item.ResyncOptions,
                NamedTags = item.NamedTags
            };
  
            return options;
        }

        internal void VerifyDataTypeAttributesAreSame(string key)
        {
            var item  = Cache.GetCacheItem(key);
            ReadThruCacheCommunication.VerifyMetaInfoInProviderDataType(item, key);
        }

        public void VerifyMessagesWrittenByProvider(string message, int count)
        {
            var lines = File.ReadAllLines(WriteThruCommunication.WRITE_THRU_SHARED_FILE);
                    

            Assert.That(lines, Has.Length.EqualTo(count), $"Expected {count} messages in the shared file, but found {lines.Length}.");
          
            foreach (var line in lines)
            {
                Assert.That(line, Is.EqualTo(message), $"Expected `{message}` messages in the shared file, but found {line}.");
            }
        }
    }
}

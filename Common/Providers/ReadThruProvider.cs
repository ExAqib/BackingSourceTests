using Alachisoft.NCache.JNIBridge.Net.JavaWrappers;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.DatasourceProviders;
using Common.WriteThru;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using List = System.Collections.Generic.List<object>;

namespace Common.Providers
{
    class ReadThruProvider : IReadThruProvider
    {
        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
            // throw new NotImplementedException();
        }

        public void Init(IDictionary parameters, string cacheId)
        {
            //throw new NotImplementedException();
        }

        public ProviderDataTypeItem<IEnumerable> LoadDataTypeFromSource(string key, DistributedDataType dataType)
        {
            if (Util.IsDataSourceZeeChangeKey(key))
                Thread.Sleep(WriteThruCommunication.ZEE_CHANGE_WAIT_TIME_IN_SECONDS * 1000);

            return dataType switch
            {
                DistributedDataType.List =>
                    new ProviderDataTypeItem<IEnumerable>(new List<string> { key }),

                DistributedDataType.Dictionary =>
                    new ProviderDataTypeItem<IEnumerable>(new Dictionary<string, object> { { key, key } }),

                DistributedDataType.Counter =>
                    new ProviderDataTypeItem<IEnumerable>(ReadThruCacheCommunication.DefaultCounterValue),

                DistributedDataType.Queue =>
                    new ProviderDataTypeItem<IEnumerable>(CreateQueue(key)),

                DistributedDataType.Set =>
                    new ProviderDataTypeItem<IEnumerable>(new HashSet<string> { key }),

                _ => throw new NotSupportedException($"Unsupported data type: {dataType}")
            };
        }

        private static Queue<string> CreateQueue(string key)
        {
            var q = new Queue<string>();
            q.Enqueue(key);
            return q;
        }

        public ProviderCacheItem LoadFromSource(string key)
        {
            var product = Util.GetProductForBackingSource(key);
            var providerCacheItem = new ProviderCacheItem(product);

            switch (ReadThruCacheCommunication.GetCaseForKey(key))
            {
                case ReadThruCacheCommunicationCases.ReturnNull:
                    return null;

                case ReadThruCacheCommunicationCases.ThrowException:
                    throw new Exception(string.Format(ReadThruCacheCommunication.ReadThruExceptionMessage, key));

                case ReadThruCacheCommunicationCases.SetAbsoluteExpiration:
                    providerCacheItem.Expiration = new Expiration(ExpirationType.Absolute, TimeSpan.FromMinutes(ReadThruCacheCommunication.GetExpirationValueFromKey(key)));
                    break;
                case ReadThruCacheCommunicationCases.SetSlidingExpiration:
                    providerCacheItem.Expiration = new Expiration(ExpirationType.Sliding, TimeSpan.FromMinutes(ReadThruCacheCommunication.GetExpirationValueFromKey(key)));
                    break;

                case ReadThruCacheCommunicationCases.SetPriority:
                    providerCacheItem.ItemPriority = ReadThruCacheCommunication.GetCacheItemPriorityFromKey(key);
                    break;

                case ReadThruCacheCommunicationCases.SetTag:
                    providerCacheItem.Tags = [new Tag(ReadThruCacheCommunication.GetCacheItemTagFromKey(key))];
                    break;

                case ReadThruCacheCommunicationCases.SetNamedTag:
                    var namedTag = new NamedTagsDictionary();
                    var pair = ReadThruCacheCommunication.GetEncodedKeyValuePair(key);
                    namedTag.Add(pair.Keys.First(), double.Parse(pair.Values.First().ToString()));
                    providerCacheItem.NamedTags = namedTag;
                    break;

                case ReadThruCacheCommunicationCases.SetQueryInfo:
                    ReadThruCacheCommunication.SetQueryInfo(key, product); // product is passed by reference (object type) changes will automatically reflected in ProviderCacheItem 
                    break;

                case ReadThruCacheCommunicationCases.CaseForValidKey:
                default:
                    break;
            }

            return providerCacheItem;
        }

        public IDictionary<string, ProviderCacheItem> LoadFromSource(ICollection<string> keys)
        {
            var result = new Dictionary<string, ProviderCacheItem>();

            if (keys != null && keys.Any(k => ReadThruCacheCommunication.GetCaseForKey(k).Equals(ReadThruCacheCommunicationCases.StartNodeForStateTransfer)))
            {
                return ZeeChangeCase(keys, result);
            }

            foreach (string key in keys)
                result.Add(key, LoadFromSource(key));


            return result;
        }

        private static IDictionary<string, ProviderCacheItem> ZeeChangeCase(ICollection<string> keys, Dictionary<string, ProviderCacheItem> result)
        {
            Thread.Sleep(WriteThruCommunication.ZEE_CHANGE_WAIT_TIME_IN_SECONDS * 1000);  // Simulate delay for state transfer

            foreach (string key in keys)
            {
                var product = Util.GetProductForBackingSource(key);
                var providerCacheItem = new ProviderCacheItem(product);
                result.Add(key, providerCacheItem);
            }

            return result;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

    }
}

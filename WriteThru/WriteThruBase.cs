using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Common;
using Common.WriteThru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.WriteThru
{
    class WriteThruBase : TestBase
    {
        public const string WriteThru = "WriteThru";
        public const string WriteBehind = "WriteBehind";
        public const string InvalidWriteThruProviderExceptionMessage = "Could not found IWriteThruProvider";// {0}";


        public WriteThruBase() : base()
        {
        }

        public CacheItem? CacheItem { get; set; }

        public static WriteThruOptions GetWriteThruOptions()
        {
            var writeThruOptions = new WriteThruOptions
            {
                ProviderName = WriteThruProviderName,
                Mode =  WriteMode.WriteThru,
            };
            return writeThruOptions;
        }

        public static WriteThruOptions GetWriteBehindOptions()
        {
            var writeThruOptions = GetWriteThruOptions();
            writeThruOptions.Mode = WriteMode.WriteBehind;
            return writeThruOptions;
        } 
        
        public WriteThruOptions GetWriteThruOptions(string mode)
        {
            if (string.IsNullOrEmpty(mode))
                throw new ArgumentException("Mode cannot be null or empty.", nameof(mode));

            return mode switch
            {
                WriteThru => GetWriteThruOptions(),
                WriteBehind => GetWriteBehindOptions(),
                _ => throw new ArgumentException($"Invalid mode: {mode}. Expected 'WriteThru' or 'WriteBehind'.", nameof(mode)),
            };
        }

        public WriteThruOptions GetWriteThruOptionsWithDefaultProviderName(string mode)
        {
            if (string.IsNullOrEmpty(mode))
                throw new ArgumentException("Mode cannot be null or empty.", nameof(mode));

            var provider = mode switch
            {
                WriteThru => GetWriteThruOptions(),
                WriteBehind => GetWriteBehindOptions(),
                _ => throw new ArgumentException($"Invalid mode: {mode}. Expected 'WriteThru' or 'WriteBehind'.", nameof(mode)),
            };

            provider.ProviderName = null;
            return provider;
        }

        internal virtual void WaitForWriteBehindCompletionIfNeeded(string mode)
        {
            if (mode.Equals(WriteBehind))
            {
                TestContext.WriteLine($"Waiting for {WriteBehindCompletionWaitTime} seconds for WriteBehind completion.");  
                Thread.Sleep(TimeSpan.FromSeconds(WriteBehindCompletionWaitTime));
            }
        }

        internal WriteThruOptions GetWriteThruOptionsWithWrongProviderName(string mode)
        {
            if (string.IsNullOrEmpty(mode))
                throw new ArgumentException("Mode cannot be null or empty.", nameof(mode));

            WriteThruOptions writeThruOptions = mode switch
            {
                WriteThru =>  GetWriteThruOptions(),
                WriteBehind =>  GetWriteBehindOptions(),
                _ => throw new ArgumentException($"Invalid mode: {mode}. Expected 'WriteThru' or 'WriteBehind'.", nameof(mode)),
            };

            writeThruOptions.ProviderName = Guid.NewGuid().ToString();
            return writeThruOptions;
        }

        public void Act(string mode, bool preAdd, string updatedKey)
        {
            if (preAdd)
            {
                Cache.Add(updatedKey, CacheItem);
                Cache.Insert(updatedKey, CacheItem, GetWriteThruOptions(mode));
                return;
            }

            Cache.Add(updatedKey, CacheItem, GetWriteThruOptions(mode));
        }


        public CacheItem GetProductForAbsoluteExpirationVerification(string metaKey)
        {
            var product = Util.GetProductForCache(metaKey);
            CacheItem = GetCacheItem(product);
            SetAbsoluteExpirationForWriteThruVerification(CacheItem);

            return CacheItem;
        }

        public void SetAbsoluteExpirationForWriteThruVerification(CacheItem cacheItem)
        {
            cacheItem.Expiration = new Expiration(ExpirationType.Absolute, TimeSpan.FromMinutes(WriteThruCommunication.AbsoluteExpirationTime));
        }

        public void SetBulkMetaInfoForWriteThruVerification(CacheItem cacheItem)
        {
            cacheItem.Priority = WriteThruCommunication.ItemPriority;
            cacheItem.Expiration = new Expiration(ExpirationType.Sliding, TimeSpan.FromMinutes(WriteThruCommunication.SlidingExpirationTime));
            cacheItem.Tags = [new Tag(WriteThruCommunication.TagName)];

            var namedTagDictionary = new NamedTagsDictionary();
            namedTagDictionary.Add(WriteThruCommunication.NamedTagKey, WriteThruCommunication.NamedTagValue);
            cacheItem.NamedTags = namedTagDictionary;
        }


        public void AssertForWriteThruAbsoluteExpiration(string key, CacheItem item)
        {
            
            Assert.That(item, Is.Not.Null,
                $"Cache item for key '{key}' should not be null after WriteThru operation.");

            Assert.That(item.Expiration, Is.Not.Null,
                $"Cache item expiration for key '{key}' should not be null after WriteThru operation.");

            Assert.That(item.Expiration.Type, Is.EqualTo(ExpirationType.Absolute),
                $"Cache item expiration type for key '{key}' should be absolute after WriteThru operation.");

            var actualMinutes = item.Expiration.ExpireAfter.TotalMinutes;
            var expected = WriteThruCommunication.AbsoluteExpirationTime;
            Assert.That(actualMinutes, Is.InRange(expected - 1, expected + 1),
                $"Cache item expiration for key '{key}' should be approximately {expected} minutes, but was {actualMinutes}.");
            
        }

        public void AssertWriteThruMetaBulk(string key, CacheItem item)
        {
           
            Assert.That(item, Is.Not.Null,
                $"Cache item for key '{key}' should not be null after WriteThru operation.");

            Assert.That(item.Expiration, Is.Not.Null,
                $"Cache item expiration for key '{key}' should not be null after WriteThru operation.");

            Assert.That(item.Expiration.Type, Is.EqualTo(ExpirationType.Sliding),
                $"Cache item expiration type for key '{key}' should be sliding after WriteThru operation.");

            Assert.That(item.Priority, Is.EqualTo(WriteThruCommunication.ItemPriority),
                $"Cache item priority for key '{key}' should be '{WriteThruCommunication.ItemPriority}' after WriteThru operation.");

            Assert.That(item.Tags?.First()?.TagName, Is.EqualTo(WriteThruCommunication.TagName),
                $"Cache item tag name for key '{key}' should be '{WriteThruCommunication.TagName}' after WriteThru operation.");

            Assert.That(item.NamedTags, Is.Not.Null,
                $"Cache item named tags for key '{key}' should not be null after WriteThru operation.");
           
        }

    }
}

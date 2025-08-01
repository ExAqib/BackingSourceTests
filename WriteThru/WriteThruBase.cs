using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Common;
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
        public ITopic Topic { get; set; }

        public IList<string> Messages = new List<string>();

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

        internal void WaitForWriteBehindCompletionIfNeeded(string mode)
        {
            if (mode.Equals(WriteBehind))
                Thread.Sleep(TimeSpan.FromSeconds(WriteBehindCompletionWaitTime));
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


        public void OnMessgeReceived(object sender, MessageEventArgs args)
        {
            string message = args.Message.Payload as string;
            Messages.Add(message);
        }

        public bool VerifyMessageReceived(string message)
        {
            Thread.Sleep(TimeSpan.FromSeconds(base.PubSubMessageWaitTime));

            return Messages.Remove(message);           
        }
    }
}

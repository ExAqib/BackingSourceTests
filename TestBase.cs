using Alachisoft.NCache.Client;
using Common;


namespace BackingSourceTests
{

    /*  #region BackingSource
    _errorMessageMap.Add(ErrorCodes.BackingSource.BACKING_SOURCE_NOT_AVAILABLE, "Backing source not available. Verify backing source settings.");
    _errorMessageMap.Add(ErrorCodes.BackingSource.SYNCHRONIZATION_WITH_DATASOURCE, "Error occurred while synchronization with data source: '{0}'");
    _errorMessageMap.Add(ErrorCodes.BackingSource.SYNCHRONIZATION_CACHE_WITH_DATASOURCE, "error while trying to synchronize the cache with data source. Error: '{0}'");
    _errorMessageMap.Add(ErrorCodes.BackingSource.WRITE_OPERATION_FAILED, "Data Source write operation failed. Error: '{0}'");
    _errorMessageMap.Add(ErrorCodes.BackingSource.IWRITE_THRU_PROVIDER_FAILED, "IWriteThruProvider failed.'{0}'");
    _errorMessageMap.Add(ErrorCodes.BackingSource.INVALID_IWRITE_THRU_PROVIDER, "Could not found IWriteThruProvider '{0}'.");
    _errorMessageMap.Add(ErrorCodes.BackingSource.INVALID_IREAD_THRU_PROVIDER, "Could not found IReadThruProvider '{0}'.");           
    #endregion */
    internal class TestBase
    {
        public ICache Cache { get; set; }
        public int WriteBehindCompletionWaitTime = 3; // (seconds) Wait time for write-behind completion to ensure data is properly added in cache.

        public string CacheName = "BackingSourceTests";
        public int CleanInterval = 30; //(seconds) Assign value that is double of actual clean interval to ensure that clean interanval does not interfere with tests.
        public char Tilda = '~';

        #region Constants 
        public const string ReadThruProviderName = "ReadThruProvider";
        public const string WriteThruProviderName = "WriteThruProvider";
        public const string Key = "Product:1";
        public const string BackingSourceNotAvailable = "Backing source not available.Verify backing source settings.";
        #endregion

        internal TestBase()
        {
            Cache ??= CacheManager.GetCache(CacheName);
        }

        public static string GetRandomKey()
        {
            return $"Product:{Guid.NewGuid()}";
        }

        public static void VerifyItemObtainedFromBackingSource(string key, Product product)
        {
            Assert.That(product, Is.Not.Null, $"Product against key `{key}` should not be null when BackingSource is configured.");
            Assert.That(product, Is.EqualTo(Util.GetProductForBackingSource(key)), "Product should match the expected product from BackingSouce.");
        }
        public static void VerifyItemObtainedByUpdateInCache(string key, Product product)
        {
            Assert.That(product, Is.Not.Null, $"Product against key `{key}` should not be null when BackingSource is configured for verifying update in cache.");
            Assert.That(product, Is.EqualTo(Util.GetProductToVerifyUpdateInCacheWorks(key)), "Product should match the expected product from BackingSouce to verify update in cache.");
        }

        public static void VerifyItemsObtainedFromBackingSource(IDictionary<string, Product> getBulkResult)
        {
            Assert.That(getBulkResult, Is.Not.Null, "Bulk result should not be null when BackingSource is configured.");   
            foreach (var pair in getBulkResult)
            {
                VerifyItemObtainedFromBackingSource(pair.Key, pair.Value);
            }
        }

        public static void VerifyItemObtainedFromCache(Product productObtained)
        { 
            Assert.Multiple(() =>
            {
                Assert.That(productObtained, Is.Not.Null, "Item obtained from ReadThru should not be null.");

                if (productObtained?.Name != null)
                    Assert.That(ProductNameParser.IsItemObtainedFromCache(productObtained.Name), Is.Not.True);
                else
                    throw new ArgumentNullException(nameof(productObtained), "Name of item added to cache should not be null.");                
            });
        }

        public void SleepForCleanInterval(string testName = "")
        {
            TestContext.WriteLine(
                $"{(string.IsNullOrEmpty(testName) ? "" : $"for test {testName}")}. -> Sleeping for {CleanInterval + 5} seconds."
            );

            // Wait slightly longer than CleanInterval to ensure resync has completed
            Thread.Sleep(TimeSpan.FromSeconds(CleanInterval + 5));
        }

        public CacheItem GetCacheItem(Product product)
        {
            var cacheItem = new CacheItem(product);
            return cacheItem;
        }

        public string[] GetHalfKeys(string[] keys)
        {
            var half = keys.Take(keys.Length / 2).ToArray();
            return half;
        }

        public string UpdateKeyWith(string key, string updatedValue)
        {
            return string.Join(Tilda, [key, updatedValue]);
        }

        public string[] UpdateKeysWith(string[] keys, string updatedValue)
        {
            string[] updatedKeys = new string[keys.Length];

            for (int i = 0; i < keys.Length; i++)
            {
                updatedKeys[i] = UpdateKeyWith(keys[i], updatedValue);
            }

            return updatedKeys;
        }

        public string[] GetUpdatedHalfKeys(string[] keys, string updatedMessage)
        {
            var halfKeys = GetHalfKeys(keys);
            var updatedKeys = UpdateKeysWith(halfKeys, updatedMessage);
            return updatedKeys;
        }

    }
}

using Alachisoft.NCache.Client;
using Common;


namespace BackingSourceTests
{
    internal class TestBase
    {
        public ICache Cache { get; set; }  
        public string CacheName = "BackingSourceTests";

        #region Constants 
        public const string ReadThruProviderName = "ReadThruProvider";
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

        public static void VerifyItemObtainedFromReadThru(string key, Product product)
        {
            Assert.That(product, Is.Not.Null, $"Product against key `{key}` should not be null when ReadThru is configured.");
            Assert.That(product, Is.EqualTo(Util.GetProductForReadThru(key)), "Product should match the expected product from ReadThru provider.");
        }
        
        public static void VerifyItemsObtainedFromReadThru(IDictionary<string, Product> getBulkResult)
        {
            Assert.That(getBulkResult, Is.Not.Null, "Bulk result should not be null when ReadThru is configured.");   
            foreach (var pair in getBulkResult)
            {
                VerifyItemObtainedFromReadThru(pair.Key, pair.Value);
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
    }
}

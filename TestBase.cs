using Alachisoft.NCache.Client;
using Alachisoft.NCache.Common.Monitoring.MetricsServer.PublishingData;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

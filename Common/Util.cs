using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Util
    {

        public readonly static DateTime BirthDay = new DateTime(2000,01,05);
        public readonly static string ZEE_CHANGE_PREFIX = "ZEE_CHANGE";
        public static Product GetProductForBackingSource(string key)
        {
            return new Product()
            {
                Id = 1,
                Name = ProductNameParser.PrepareProductNameForBackingSource(key),
                Category = "ReadThru",
                Price = 19.99m,
                CreatedOn = BirthDay
            };
        }

        public static Product GetProductToVerifyUpdateInCacheWorks(string key)
        {
            return new Product()
            {
                Id = 1,
                Name = ProductNameParser.PrepareProductNameForBackingSource(key),
                Category = "ReadThru|VerifyUpdateInCacheWorks",
                Price = 19.99m,
                CreatedOn = BirthDay
            };
        }


        public static Product GetProductForCache(string key)
        {
            return new Product()
            {
                Id = 1,                
                Name = ProductNameParser.PrepareProductNameForCache(key),
                Category = "Cache",
                Price = 19.99m,
                CreatedOn = BirthDay
            };
        }
                
        public static bool IsDataSourceZeeChangeKey(string key)
        {
            return key.Contains(ZEE_CHANGE_PREFIX);
        }

        internal static string GetAppendedZeeChangeKey(string dataStructureKeyForList)
        {
            // guid is to make sure that item is moved during state transfer
            return $"{Guid.NewGuid().ToString().Split('-')[0]}|{ZEE_CHANGE_PREFIX}|{dataStructureKeyForList}";
        }
    }

}

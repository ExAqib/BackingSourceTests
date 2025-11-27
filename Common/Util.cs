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
    }

}

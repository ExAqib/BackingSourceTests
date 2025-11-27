using Alachisoft.NCache.Common;
using Alachisoft.NCache.Runtime.Caching;
using Common.WriteThru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    static class ProviderItemBaseExtension
    {
        //WriteThruKeyForVerifyingMetaInfoBulk = "WriteThru|SLD_EXP=5|PRIORITY=High|TAG=Sale|NAMED_TAG=discount:3.5";

        public static bool VerifySlidingExpiration(this ProviderItemBase providerItemBase)
        {
            bool isTimeValid = providerItemBase.Expiration.ExpireAfter.TotalMinutes <= WriteThruCommunication.SlidingExpirationTime;
            var isTimeApproxitmatelyCorrect = Math.Abs(providerItemBase.Expiration.ExpireAfter.TotalMinutes - WriteThruCommunication.SlidingExpirationTime) < 2;

            return providerItemBase.Expiration.Type == ExpirationType.Sliding && isTimeValid &&
                isTimeApproxitmatelyCorrect;
            
        }

        public static bool VerifyAbsoluteExpiration(this ProviderItemBase providerItemBase)
        {
            bool isTimeValid = providerItemBase.Expiration.ExpireAfter.TotalMinutes <= WriteThruCommunication.AbsoluteExpirationTime;
            var isTimeApproxitmatelyCorrect = Math.Abs(providerItemBase.Expiration.ExpireAfter.TotalMinutes - WriteThruCommunication.AbsoluteExpirationTime) < 2;
            return providerItemBase.Expiration.Type == ExpirationType.Absolute && isTimeValid &&
                 isTimeApproxitmatelyCorrect;

        }


        public static bool VerifyItemPriority(this ProviderItemBase providerItemBase)
        {
            return providerItemBase.ItemPriority == WriteThruCommunication.ItemPriority;
        }

        public static bool VerifyTag(this ProviderItemBase providerItemBase)
        {
            return providerItemBase.Tags != null && providerItemBase.Tags.Length > 0 &&
                   providerItemBase.Tags[0].TagName == WriteThruCommunication.TagName;
        }

        public static bool VerifyNamedTag(this ProviderItemBase providerItemBase)
        {
            bool namedTagExist = providerItemBase.NamedTags != null && providerItemBase.NamedTags.Count > 0 &&
                   providerItemBase.NamedTags.Contains(WriteThruCommunication.NamedTagKey);

            if (!namedTagExist)
                return false;

            var namedTagValue = providerItemBase?.NamedTags?.ToDictionary()?[WriteThruCommunication.NamedTagKey]?.ToString();

            if (namedTagValue != null )
                return (double.Parse(namedTagValue.ToString())).Equals(WriteThruCommunication.NamedTagValue) == true;

            return false;
        }
    }
}

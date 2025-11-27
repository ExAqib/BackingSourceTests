using Alachisoft.NCache.Runtime.Caching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    static class NamedTagDictionaryExtension
    {
        public static IDictionary<string, object> ToDictionary(this NamedTagsDictionary namedTagDictionary)
        {
            Dictionary<string, Object> result = [];
            
            if (namedTagDictionary == null)
                return result;

            var enumerator = namedTagDictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                if (current is DictionaryEntry entry)                
                    result.Add((string)entry.Key, entry.Value);               


                else if (current is KeyValuePair<string, object> kvp)                
                    result.Add(kvp.Key, kvp.Value);                  
                
            }

            return result;         
        }

    }

}

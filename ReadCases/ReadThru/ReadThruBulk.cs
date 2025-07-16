using Alachisoft.NCache.Common.Protobuf;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BackingSourceTests.ReadCases.ReadThru
{
    class ReadThruBulk : ReadThruBase
    {

        //  Verify all missing keys are obtained from DS in a single bulk call
        // Verify some keys in cache → only missing keys fetched
        // Verify forced bulk read-thru refreshes even existing keys
        // Verify proper exception handling if one key fails(throws)
        //Verify proper handling if some keys return null

    }
}

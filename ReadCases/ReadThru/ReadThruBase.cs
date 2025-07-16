using Alachisoft.NCache.Runtime.Caching;

namespace BackingSourceTests.ReadCases.ReadThru
{
    internal class ReadThruBase : TestBase
    {
        public static ReadThruOptions GetReadThruOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.ReadThru,
                ProviderName = ReadThruProviderName
            };

            return readThruOptions;
        }

        public static ReadThruOptions GetDefaultReadThruOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.ReadThru,
            };

            return readThruOptions;
        }

        public static ReadThruOptions GetReadThruForcedOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.ReadThruForced,
                ProviderName = ReadThruProviderName
            };

            return readThruOptions;
        }

        public static ReadThruOptions GetInvalidReadOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.ReadThru,
                ProviderName = Guid.NewGuid().ToString() // Random invalid provider name
            };

            return readThruOptions;
        }
        public static ReadThruOptions GetNoneReadThruOptions()
        {
            ReadThruOptions readThruOptions = new()
            {
                Mode = ReadMode.None,
                ProviderName = ReadThruProviderName
            };

            return readThruOptions;
        }
    }
}
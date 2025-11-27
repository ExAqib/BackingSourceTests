using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.WriteThru
{
    public class WriteThruCommunication
    {
        public const string WRITE_THRU_SHARED_FILE = @"\\pdc\File Share\DEV\Aqib\WriteThruTestCase\WriteThruTestCaseFile.txt";
        public const int ZEE_CHANGE_WAIT_TIME_IN_SECONDS = 30;

        public const string BulkMetaVerifiedMessage = "Bulk Meta info successfully verified for WriteThru.";    
        public const string AbsoluteExpirationVerifiedMessage = "Absolute Expiraion successfully verified for WriteThru.";    

        public const string KeyForUpdateInCache = "WriteThru|UPDATE_IN_CACHE";
        public const string KeyForSuccess = "WriteThru|SUCCESS";
        public const string KeyForFailure = "WriteThru|FAILURE_ONLY";
        public const string KeyForFailureRetry = "WriteThru|FAILURE_RETRY";
        public const string KeyForFailureDontRemove = "WriteThru|FAILURE_DONT_REMOVE";
        public const string KeyForThrowException = "WriteThru|THROW_EXCEPTION";
        public const string KeyForErrorMessage = "WriteThru|VALIDATE_ERROR_MESSAGE";
        public const string KeyForRemovedFromCache = "WriteThru|REMOVED_FROM_CACHE";

        #region ZeeChange 

        public const string KeyForZeeChangeCacheWriteThruFailure = "WriteThru|ZEE_CHANGE_CACHE_WRITE_THRU_FAILURE";
        public const string KeyForZeeChangeUpdateInCache = "WriteThru|ZEE_CHANGE_UPDATE_IN_CACHE";
        public const string KeyForZeeChangeRemoveFromCache = "WriteThru|ZEE_CHANGE_REMOVE_FROM_CACHE";
        public const string KeyForZeeChangeSuccess = "WriteThru|ZEE_CHANGE_SUCCESS";
        public const string KeyForZeeChangeFailureDontRemove = "WriteThru|ZEE_CHANGE_FAILURE_DONT_REMOVE";
        public const string KeyForZeeChangeFailureRetry = "WriteThru|ZEE_CHANGE_FAILURE_RETRY";
        #endregion


        public const string FailureErrorMessage = "Simulated WriteThru Error Message.";

        #region MetaData 
        public const string KeyForVerifyingMetaInfoBulk = "WriteThru|SLD_EXP=5|PRIORITY=High|TAG=Sale|NAMED_TAG=discount:3.5";
        public const string KeyForAbsoluteExpiration = "WriteThru|ABS_EXP=5";

        public const int SlidingExpirationTime = 5;
        public const int AbsoluteExpirationTime = 5; 
        public const Alachisoft.NCache.Runtime.CacheItemPriority ItemPriority = Alachisoft.NCache.Runtime.CacheItemPriority.High;
        public const string TagName = "Sale";
        public const string NamedTagKey = "discount"; 
        public const double NamedTagValue = 3.5; // Named tag with value 3.5

        #endregion



        public const string ErrorMessage = "Simulated provider error";
        public const string ExceptionMessage = "Simulated provider exception";
        public const string InvalidKeyExceptionMessage = "Invalid key for WriteThruCommunication case retrieval.";
        public const int RetryCount = 3; // Number of retries for failure scenarios

        private const string WriteThruKeyForRemoveForExpirationVerification = "WriteThru|Exp|";
        

        internal static WriteThruCommunicationCases GetCase(string key)
        {
            return key switch
            {
                var _ when key.Contains(KeyForUpdateInCache) => WriteThruCommunicationCases.UpdateInCache,
                var _ when key.Contains (KeyForSuccess) => WriteThruCommunicationCases.Success,
                var _ when key.Contains (KeyForFailureDontRemove) => WriteThruCommunicationCases.FailureDontRemove,
                var _ when key.Contains (KeyForFailureRetry) => WriteThruCommunicationCases.FailureRetry,
                var _ when key.Contains (KeyForFailure) => WriteThruCommunicationCases.Failure,
                var _ when key.Contains (KeyForThrowException) => WriteThruCommunicationCases.ThrowException,
                var _ when key.Contains (KeyForErrorMessage) => WriteThruCommunicationCases.ValidateErrorMessage,
                var _ when key.Contains (KeyForRemovedFromCache) => WriteThruCommunicationCases.RemovedFromCache,
                var _ when key.Contains (KeyForAbsoluteExpiration) => WriteThruCommunicationCases.VerifyAbsoluteExpiration, 
                var _ when key.Contains (KeyForVerifyingMetaInfoBulk) => WriteThruCommunicationCases.VerifyBulkMetaInfo,
                var _ when key.Contains (KeyForZeeChangeCacheWriteThruFailure) => WriteThruCommunicationCases.ZeeChangeCacheWriteThruFailure,
                var _ when key.Contains (KeyForZeeChangeUpdateInCache) => WriteThruCommunicationCases.ZeeChangeUpdateInCache,
                var _ when key.Contains (KeyForZeeChangeRemoveFromCache) => WriteThruCommunicationCases.ZeeChangeRemoveFromCache,
                var _ when key.Contains (KeyForZeeChangeFailureDontRemove) => WriteThruCommunicationCases.ZeeChangeFailureDontRemove,
                var _ when key.Contains (KeyForZeeChangeFailureRetry) => WriteThruCommunicationCases.ZeeChangeFailureRetry,
                var _ when key.Contains (KeyForZeeChangeSuccess) => WriteThruCommunicationCases.ZeeChangeSuccess,
                _ => throw new ArgumentException(InvalidKeyExceptionMessage, nameof(key))
            };
        }    
    }

    public enum WriteThruCommunicationCases
    {
        UpdateInCache,
        Success,
        Failure,
        FailureRetry,
        ThrowException,
        ValidateErrorMessage,
        FailureDontRemove,
        RemovedFromCache,
        VerifyAbsoluteExpiration,
        VerifyBulkMetaInfo,
        ZeeChangeCacheWriteThruFailure,
        ZeeChangeUpdateInCache,
        ZeeChangeRemoveFromCache,
        ZeeChangeFailureDontRemove,
        ZeeChangeFailureRetry,
        ZeeChangeSuccess,
    }
}

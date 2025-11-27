using Alachisoft.NCache.Licensing.DOM;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.DatasourceProviders;
using Common.Extensions;
using Common.WriteThru;
using Quartz.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Common.Providers
{
    class WriteThruProvider : IWriteThruProvider
    {
        public string filePath = @"\\pdc\File Share\DEV\Aqib\WriteThruTestCase\WriteThruTestCaseFile.txt";

        // for 5.4 and above
        public void Init(IDictionary<string, string> parameters, string cacheName)
        {
            if (parameters?.TryGetValue("path", out var sharedFilePath) == true)
                InitializeFilePath(sharedFilePath);

        }

        public void Init(IDictionary parameters, string cacheName)
        {
            if (parameters?.Contains("path") == true)
                InitializeFilePath(parameters["path"]?.ToString());
        }

        void InitializeFilePath(string path)
        {
            if (!string.IsNullOrEmpty(path))
                filePath = path;

            filePath ??= WriteThruCommunication.WRITE_THRU_SHARED_FILE;
        }

        public OperationResult WriteToDataSource(WriteOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation), "Write operation cannot be null.");

            string key = operation.Key;

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(operation.Key));

            var result = new OperationResult(operation, OperationResult.Status.Success);
            var product = Util.GetProductForBackingSource(key);

            switch (WriteThruCommunication.GetCase(key))
            {
                case WriteThruCommunicationCases.UpdateInCache:
                case WriteThruCommunicationCases.ZeeChangeUpdateInCache: // For write behind bulk, we handle ZeeChangeUpdateInCache here becase in WriteBehindBase we call WriteToDataSource for each operation individually.

                    result.UpdateInCache = true;
                    product = Util.GetProductToVerifyUpdateInCacheWorks(key);
                    break;

                case WriteThruCommunicationCases.Success:
                case WriteThruCommunicationCases.ZeeChangeSuccess:
                    result.OperationStatus = OperationResult.Status.Success;
                    break;

                case WriteThruCommunicationCases.Failure:
                case WriteThruCommunicationCases.ZeeChangeCacheWriteThruFailure:
                    result.OperationStatus = OperationResult.Status.Failure;
                    result.Error = WriteThruCommunication.FailureErrorMessage;
                    break;

                case WriteThruCommunicationCases.FailureRetry:
                case WriteThruCommunicationCases.ZeeChangeFailureRetry:
                    result.OperationStatus = OperationResult.Status.FailureRetry;

                    if (operation.RetryCount == WriteThruCommunication.RetryCount)
                        result.OperationStatus = OperationResult.Status.Success;
                    break;

                case WriteThruCommunicationCases.FailureDontRemove:
                case WriteThruCommunicationCases.ZeeChangeFailureDontRemove:
                    result.OperationStatus = OperationResult.Status.FailureDontRemove;
                    break;

                case WriteThruCommunicationCases.ValidateErrorMessage:
                    result.OperationStatus = OperationResult.Status.FailureRetry;
                    if (operation.RetryCount == 0)
                    {
                        operation.RetryCount = WriteThruCommunication.RetryCount;
                        result.Error = WriteThruCommunication.ErrorMessage;
                        return result;
                    }
                    else
                    {
                        if (result.Error.Equals(WriteThruCommunication.ErrorMessage))
                            throw new Exception(WriteThruCommunication.ErrorMessage); // Throwing this exception should pass the test case.

                        else
                            throw new Exception($"Error message set is different from error message received. Error Message Received: {result.Error}");
                    }

                case WriteThruCommunicationCases.ThrowException:
                    throw new Exception(WriteThruCommunication.ExceptionMessage);


                case WriteThruCommunicationCases.RemovedFromCache:
                case WriteThruCommunicationCases.ZeeChangeRemoveFromCache:
                    product = null;
                    break;

                case WriteThruCommunicationCases.VerifyAbsoluteExpiration:
                    VerifyAbsoluteExpitation(operation);
                    break;

                case WriteThruCommunicationCases.VerifyBulkMetaInfo:
                    VerifyBulkMetaInfo(operation);
                    break;

                //case WriteThruCommunicationCases.RemoveFromZeeChangeCache:
                //    Thread.Sleep();
                //    product = null;
                //    break;

                default:
                    throw new Exception($"No case is defined for key {key} in WriteThru provider. Please update the provider to handle the case or pass a valid key.");

            }

            if (product != null)
                operation.ProviderItem.SetValue(product);

            return result;
        }

        private void VerifyAbsoluteExpitation(WriteOperation operation)
        {
            if (operation.ProviderItem.VerifyAbsoluteExpiration())
                WriteInFile(WriteThruCommunication.AbsoluteExpirationVerifiedMessage);
            else
                throw new Exception("(DeployedWriteThruProvider) -> Absolute Expiration Meta info not verified successfuly by write thru provider. ");

        }

        private void VerifyBulkMetaInfo(WriteOperation operation)
        {
            // Clear any previos data in file.
            WriteInFile("");

            var providerItem = operation.ProviderItem;

            bool result = providerItem.VerifyItemPriority() &&
                providerItem.VerifySlidingExpiration() &&
                providerItem.VerifyNamedTag() &&
                providerItem.VerifyTag();

            if (result)
                WriteInFile(WriteThruCommunication.BulkMetaVerifiedMessage);
            else
                throw new Exception("(DeployedWriteThruProvider) -> Meta info not verified successfuly by write thru provider. ");
        }



        public ICollection<OperationResult> WriteToDataSource(ICollection<WriteOperation> operations)
        {

            var opsList = operations?.ToList();
            if (opsList == null || opsList.Count == 0)
                return [];

            #region ZEE CHANGE HANDLING
            // Find the case for the FIRST operation (or choose your rule)
            var caseType = WriteThruCommunication.GetCase(opsList[0].Key);

            // Mapping: case → handler
            var handlerMap = new Dictionary<WriteThruCommunicationCases, Func<List<WriteOperation>, List<OperationResult>>>
            {
                { WriteThruCommunicationCases.ZeeChangeCacheWriteThruFailure, HandleZeeChangeCacheWriteThruFailure },
                { WriteThruCommunicationCases.ZeeChangeUpdateInCache,         HandleZeeChangeUpdateInCache },
                { WriteThruCommunicationCases.ZeeChangeSuccess,               HandleZeeChangeSucces },
                { WriteThruCommunicationCases.ZeeChangeFailureRetry,          HandleZeeChangeFailureRetry },
                { WriteThruCommunicationCases.ZeeChangeFailureDontRemove,     HandleZeeChangeFailureDontRemove },
                { WriteThruCommunicationCases.ZeeChangeRemoveFromCache,       HandleZeeChangeRemoveFromCache },
            };

            // If case has a special handler
            if (handlerMap.TryGetValue(caseType, out var handler))
            {
                Thread.Sleep(WriteThruCommunication.ZEE_CHANGE_WAIT_TIME_IN_SECONDS * 1000);
                return handler(opsList);
            }
            #endregion

            // Default write-through behavior
            var results = new List<OperationResult>(opsList.Count);
            foreach (var op in opsList)
            {
                var result = WriteToDataSource(op);
                results.Add(result);
            }

            return results;
        }


        #region ZeeChange

        static List<OperationResult> HandleZeeChangeCacheWriteThruFailure(ICollection<WriteOperation> operations)
        {
            return HandleZeeChangeCore(
                operations,
                productFactory: null,  // no product assigned
                statusFactory: op => OperationResult.Status.Failure
            );
        }


        static List<OperationResult> HandleZeeChangeUpdateInCache(ICollection<WriteOperation> operations)
        {
            return HandleZeeChangeCore(
                operations,
                op => Util.GetProductToVerifyUpdateInCacheWorks(op.Key),
                op => OperationResult.Status.Success,
                updateInCache: true
            );
        }

        static List<OperationResult> HandleZeeChangeSucces(List<WriteOperation> operations)
        {
            return HandleZeeChangeCore(
                operations,
                op => Util.GetProductForBackingSource(op.Key),
                op => OperationResult.Status.Success
            );
        }


        static List<OperationResult> HandleZeeChangeFailureRetry(List<WriteOperation> operations)
        {
            return HandleZeeChangeCore(
                operations,
                op => Util.GetProductForBackingSource(op.Key),
                op =>
                {
                    // Retry until max
                    if (op.RetryCount == WriteThruCommunication.RetryCount)
                        return OperationResult.Status.Success;

                    return OperationResult.Status.FailureRetry;
                }
            );
        }


        static List<OperationResult> HandleZeeChangeFailureDontRemove(List<WriteOperation> operations)
        {
            return HandleZeeChangeCore(
                operations,
                op => Util.GetProductForBackingSource(op.Key),
                op => OperationResult.Status.FailureDontRemove
            );
        }


        static List<OperationResult> HandleZeeChangeRemoveFromCache(List<WriteOperation> operations)
        {
            return HandleZeeChangeCore(
                operations,
                op => null,                               // SetValue(null)
                op => OperationResult.Status.Success
            );
        }


        private static List<OperationResult> HandleZeeChangeCore(
    ICollection<WriteOperation> operations,
    Func<WriteOperation, object?> productFactory,
    Func<WriteOperation, OperationResult.Status> statusFactory,
    bool updateInCache = false)
        {
            var result = new List<OperationResult>(operations.Count);

            foreach (var operation in operations)
            {
                var product = productFactory?.Invoke(operation);

                // Assign product only if non-null OR explicitly required
                operation.ProviderItem.SetValue(product);

                var status = statusFactory(operation);

                var opResult = new OperationResult(operation, status)
                {
                    UpdateInCache = updateInCache
                };

                result.Add(opResult);
            }

            return result;
        }



        #endregion

        public ICollection<OperationResult> WriteToDataSource(ICollection<DataTypeWriteOperation> dataTypeWriteOperations)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }



        //public OperationResult WriteToDataSource(QueryWriteOperation queryWriteOperation)
        //{
        //    return default;
        //    //TODO: implement it 
        //    //throw new NotImplementedException();
        //}

        void WriteInFile(string message)
        {
            File.WriteAllText(filePath, message);
        }
    }


}

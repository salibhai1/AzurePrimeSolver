﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using PrimeSolverCommon;
using PrimeSolverRepository;

namespace PrimeSolverWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private CloudQueue _primesQueue;
        //private CloudBlobContainer primesBlobContainer;
        private PrimeNumbersRepository _repository;
        //private CloudTable _tableContainer;
        public string BaseUrl { get; set; }
        readonly HttpClient _httpClient = new HttpClient();
        private int _numItems;
        private int _numProcessed;

        public override void Run()
        {
            Trace.TraceInformation("PrimeSolverWorker entry point called");
            CloudQueueMessage msg = null;

            //todo - try http://stackoverflow.com/a/16739691/2343

            // To make the worker role more scalable, implement multi-threaded and 
            // asynchronous code. See:
            // http://msdn.microsoft.com/en-us/library/ck8bc5c6.aspx
            // http://www.asp.net/aspnet/overview/developing-apps-with-windows-azure/building-real-world-cloud-apps-with-windows-azure/web-development-best-practices#async
            while (true)
            {
                try
                {
                    // Retrieve a new message from the queue.
                    // A production app could be more efficient and scalable and conserve
                    // on transaction costs by using the GetMessages method to get
                    // multiple queue messages at a time. See:
                    // http://azure.microsoft.com/en-us/documentation/articles/cloud-services-dotnet-multi-tier-app-storage-5-worker-role-b/#addcode
                    msg = this._primesQueue.GetMessage();
                    if (msg != null)
                    {
                        ProcessQueueMessage(msg);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (StorageException e)
                {
                    if (msg != null && msg.DequeueCount > 5)
                    {
                        this._primesQueue.DeleteMessage(msg);
                        Trace.TraceError("Deleting poison queue item: '{0}'", msg.AsString);
                    }
                    Trace.TraceError("Exception in PrimeSolverWorker: '{0}'", e.Message);
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        private async Task ProcessQueueMessage(CloudQueueMessage msg)
        {
            Trace.TraceInformation("Processing queue message {0}", msg);

            var workPackage = JsonConvert.DeserializeObject<PrimeNumberWorkPackage>(msg.AsString);
            if (workPackage.WorkType == WorkType.NewWork)
            {
                
                _numItems = workPackage.NumEntries;
                _numProcessed = 0;
                this._primesQueue.DeleteMessage(msg);
                return;
            }

            var numberToTest = workPackage.Number;
            var isPrime = await Task.Run(() => PrimeSolver.IsPrime(numberToTest));
            var primeNumberCandidate = new PrimeNumberCandidate(numberToTest)
            {
                IsPrime = isPrime
            };
            await _repository.Add(primeNumberCandidate);

            _numProcessed++;
            await CommunicateResult(primeNumberCandidate);
            //var percent = (int)Math.Round((double)(100 * _numProcessed) / _numItems);
            await CommunicateProgress();
            // Remove message from queue.
            this._primesQueue.DeleteMessage(msg);
        }

        /// <summary>
        /// Send result to client endpoint
        /// </summary>
        /// <param name="primeCandidate"></param>
        /// <remarks>Thanks to http://www.jerriepelser.com/blog/communicate-from-azure-webjob-with-signalr for the sample</remarks>
        /// <returns></returns>
        private async Task CommunicateResult(PrimeNumberCandidate primeCandidate)
        {
            if (!primeCandidate.IsPrime.HasValue || !primeCandidate.IsPrime.Value)
                return;

            var queryString = $"?number={primeCandidate.Number}&isPrime={primeCandidate.IsPrime}";
            //var request = CloudConfigurationManager.GetSetting("ProgressNotificationEndpoint");
            var request = BaseUrl + "/PrimeNumberCandidate/ResultNotification" + queryString;
            await _httpClient.GetAsync(request);
        }

        /// <summary>
        /// Send update to client endpoint
        /// </summary>
        /// <returns></returns>
        private async Task CommunicateProgress()
        {
            var percent = (int)Math.Round((double)(100 * _numProcessed) / _numItems);
            //var percent = _primesQueue.ApproximateMessageCount;
            var queryString = $"?percent={percent}";
            //var request = CloudConfigurationManager.GetSetting("ProgressNotificationEndpoint");
            var request = BaseUrl + "/PrimeNumberCandidate/ProgressNotification" + queryString;
            await _httpClient.GetAsync(request);
        }



        // A production app would also include an OnStop override to provide for
        // graceful shut-downs of worker-role VMs.  See
        // http://azure.microsoft.com/en-us/documentation/articles/cloud-services-dotnet-multi-tier-app-storage-3-web-role/#restarts
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections.
            ServicePointManager.DefaultConnectionLimit = 12;

            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse
                (RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            Trace.TraceInformation("Creating primes blob container");
            //var blobClient = storageAccount.CreateCloudBlobClient();
            //var tableClient = storageAccount.CreateCloudTableClient();

            //_tableContainer = tableClient.GetTableReference("primes");
            //primesBlobContainer = blobClient.GetContainerReference("primes");
            //if (_tableContainer.CreateIfNotExists())
            //{
            // Enable public access on the newly created "primes" container.
            //_tableContainer.SetPermissions(new TablePermissions(), new TableRequestOptions(new TableRequestOptions() {}));
            //}
            var dbConnString = CloudConfigurationManager.GetSetting("PrimeSolverDbConnectionString");
            _repository = new PrimeNumbersRepository(dbConnString);
            //var webRoleEndPoint = RoleEnvironment.Roles["PrimeWorkerWeb"].Instances[0].InstanceEndpoints["WebPortalEndPointHttps"].ToString();

            BaseUrl = CloudConfigurationManager.GetSetting("ApplicationRoot");

            Trace.TraceInformation("Creating primes queue");
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            _primesQueue = queueClient.GetQueueReference("primes");
            _primesQueue.CreateIfNotExists();

            //Trace.TraceInformation("Storage initialized");
            return base.OnStart();
        }
    }
}

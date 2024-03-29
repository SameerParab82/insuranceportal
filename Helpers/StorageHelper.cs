﻿using InsuranceAzure.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceAzure.Helpers
{
    public class StorageHelper
    {
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudTableClient tableClient;
        private CloudQueueClient queueClient;

        public string ConnectionString
        {
            set
            {

                this.storageAccount = CloudStorageAccount.Parse(value);
                this.blobClient = storageAccount.CreateCloudBlobClient();
                //this.tableClient = storageAccount.CreateCloudTableClient(); // For storage table
                this.queueClient = storageAccount.CreateCloudQueueClient();
            }
        }

        public string CosmosConnectionString
        {
            set
            {
                var sa = CloudStorageAccount.Parse(value);
                this.tableClient = sa.CreateCloudTableClient();

            }
        }

        public async Task<string> UploadCustomerImageAsync(string ContainerName, string ImagePath)
        {
            var container = blobClient.GetContainerReference(ContainerName);
            await container.CreateIfNotExistsAsync();
            var ImageName = Path.GetFileName(ImagePath);
            var blob = container.GetBlockBlobReference(ImageName);
            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(ImagePath);
            return blob.Uri.AbsoluteUri;
        }

        public async Task<Customer> InsertCustomerAsync(string TableName, Customer customer)
        {
            var table = tableClient.GetTableReference(TableName);
            await table.CreateIfNotExistsAsync();
            TableOperation insertOperation = TableOperation.Insert(customer);
            var result = await table.ExecuteAsync(insertOperation);
            return result.Result as Customer;
        }

        public async Task AddMessageAsync(string queueName, Customer customer)
        {
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            var messageBody = JsonConvert.SerializeObject(customer);
            CloudQueueMessage message = new CloudQueueMessage(messageBody);
            await queue.AddMessageAsync(message, TimeSpan.FromDays(4), TimeSpan.Zero, null, null);

        }
    }
}

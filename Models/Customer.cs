using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceAzure.Models
{
    public class Customer : TableEntity//For Azure table
    {
        public Customer(string CustomerId, string InsuranceType)
        {
            this.RowKey = CustomerId;
            this.PartitionKey = InsuranceType;
        }

        public string FullName { get; set; }
        public string Email { get; set; }
        public double Amount { get; set; }
        public double Premium { get; set; }
        public DateTime AppDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ImageURL { get; set; }
    }
}

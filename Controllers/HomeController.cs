using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InsuranceAzure.Models;
using InsuranceAzure.Models.ViewModels;
using System.IO;
using InsuranceAzure.Helpers;
using Microsoft.Extensions.Configuration;

namespace InsuranceAzure.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerVM model)
        {
            if (ModelState.IsValid)
            {
                var customerid = Guid.NewGuid();
                StorageHelper storageHelper = new StorageHelper();
                storageHelper.ConnectionString = _configuration.GetConnectionString("StorageConnection");//blob, queue and table storage client
                storageHelper.CosmosConnectionString = _configuration.GetConnectionString("CosmosConnection");//cosmos table client
                //Save Customer image to Azure BLOB
                var tempFile = Path.GetTempFileName();
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    await model.Image.CopyToAsync(fs);
                }
                var fileName = Path.GetFileName(model.Image.FileName);
                var tmpPath = Path.GetDirectoryName(tempFile);
                var imagePath = Path.Combine(tmpPath, string.Concat(customerid, "_", fileName));
                System.IO.File.Move(tempFile, imagePath);//rename temp file
                var ImageUrl = await storageHelper.UploadCustomerImageAsync("imagecontainer", imagePath);

                //Save Customer data to Azure table
                Customer customer = new Customer(customerid.ToString(), model.InsuranceType);
                customer.FullName = model.FullName;
                customer.Email = model.Email;
                customer.Amount = model.Amount;
                customer.Premium = model.Premium;
                customer.AppDate = model.AppDate;
                customer.EndDate = model.EndDate;
                customer.ImageURL = ImageUrl;
                await storageHelper.InsertCustomerAsync("custinsurancetable", customer);


                //Add a confirmation message to Azure Queue

                await storageHelper.AddMessageAsync("insurance-request", customer);
                return RedirectToAction("Index");

            }
            return View();
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

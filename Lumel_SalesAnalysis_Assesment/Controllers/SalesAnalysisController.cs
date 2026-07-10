using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using Lumel_SalesAnalysis_Assesment.Models;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Lumel_SalesAnalysis_Assesment.Models.RequestModels;
using Microsoft.EntityFrameworkCore;

namespace Lumel_SalesAnalysis_Assesment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesAnalysisController : Controller
    {
        #region Declaration
        private readonly SalesAnalysisContext _context;
        #endregion

        #region Constructor
        public SalesAnalysisController(SalesAnalysisContext context)
        {
            _context = context;
        }
        #endregion


        #region UploadCSV
        [HttpPost("upload")]
        public async Task<IActionResult> UploadCSVFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a csv file");
            }
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecord<List<SalesCsvDTO>>().ToList();
            foreach (var item in records)
            {

                var customer = _context.Customers.FirstOrDefault(x => x.CustomerCode == item.CustomerId.ToString());
                if (customer == null)
                {
                    customer = new Customer
                    {
                        CustomerCode = item.CustomerId.ToString(),
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                var product = _context.Products.FirstOrDefault(x => x.ProductCode == item.ProductId);
                if (product == null)
                {
                    product = new Product
                    {
                        ProductCode = item.ProductId,
                        ProductName = item.ProductName,
                        Category = item.Category,
                        UnitPrice = item.UnitPrice,
                        UnitCost = item.UnitPrice * 0.70m
                    };
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                }

                var Orders = _context.Orders.FirstOrDefault(x => x.OrderId == item.OrderId);
                if (Orders == null)
                {
                    Orders = new Order()
                    {
                        OrderId = item.OrderId,
                        CustomerId = item.CustomerId,
                        Region = item.Region,
                        SaleDate = item.DateOfSale,
                        Discount = item.Discount,
                        ShippingCost = item.ShippingCost,
                        PaymentMethod = item.PaymentMethod,

                    };

                    _context.Orders.Add(Orders);
                    await _context.SaveChangesAsync();
                }
                bool exists = _context.OrderItems.Any(x => x.OrderId == Orders.OrderId && x.ProductId == product.ProductId);
                if (!exists)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = Orders.OrderId,
                        ProductId = product.ProductId,
                        Quantity = item.QuantitySold,
                    });

                }
            }
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = "CSV File Uploaded Sucessfully",
                TotalRecords = records.Count,
            });
        }
        #endregion


        #region Total Revenue
        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue(DateOnly? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
            {
                return BadRequest("Start Date cannot be greater than End Date");

            }
            var revenue = await (from o in _context.Orders
                                 join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                                 join P in _context.Products on oi.ProductId equals P.ProductId
                                 where o.SaleDate >= startDate && o.SaleDate <= endDate.Value.Date
                                 select new
                                 {
                                     Amount = (oi.Quantity * P.UnitPrice) - o.Discount + o.ShippingCost
                                 });

            return Ok(new
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = revenue.TotalRevenue,
            });
        }
        #endregion


        #region GetRevenueByCategory 
         [HttpGet("GetRevenueByCategory")]
        public async Task<IActionResult> GetRevenueByCategory(DateOnly saledate)
        {
            var result = await (from o in _context.Orders
                                join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                                join P in _context.Products on oi.ProductId equals P.ProductId
                                where o.SaleDate >= saledate && o.SaleDate < saledate.AddDays(1)
                                group new { oi, P, o } by P.Category into g
                                select new
                                {
                                    Category = g.Key,
                                    TotalQuantity = g.Sum(x => x.oi.Quantity),
                                    TotalReevenur = g.Sum(x => (x.oi.Quantity * x.P.UnitPrice) - x.o.Discount + x.o.ShippingCost)
                                }).OrderByDescending(x => x.TotalReevenur).ToListAsync();

            if (!result.Any())
            {
                return NotFound("No data found");
            }

            return Ok(result);  
        }
        #endregion

    }
}

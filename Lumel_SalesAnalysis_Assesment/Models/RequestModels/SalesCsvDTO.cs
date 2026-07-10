namespace Lumel_SalesAnalysis_Assesment.Models.RequestModels
{
    public class SalesCsvDTO
    {
        public int OrderId { get; set; } 
        public string ProductId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; } 
        public string Category {  get; set; }
        public string Region { get; set; }
        public DateOnly DateOfSale { get; set; }    
        public int QuantitySold { get; set; }   
        public decimal UnitPrice { get; set; }  
        public decimal Discount { get; set; }   
        public decimal ShippingCost     { get; set; }   
        public string PaymentMethod { get; set; }   
    }
}

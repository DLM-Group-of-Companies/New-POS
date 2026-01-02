namespace NLI_POS.Models.ViewModels
{
    public class PivotedOrderViewModel
    {
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustCode { get; set; }
        public string CustomerName { get; set; }
        public string OrderType { get; set; }
        public string ProdCategory { get; set; }
        public Dictionary<string, int> MainCounts { get; set; } = new();
        public int TotalMain { get; set; }        
        public string OfficeName { get; set; }
        public double Amount { get; set; }
        public int Qty { get; set; }
        public int Price { get; set; }
    }

    public class MonthlySalesRowVM
    {
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string OrderType { get; set; }
        public string OfficeName { get; set; }
        public decimal TotalAmount { get; set; }

        // Product item details (optional if you still want to list them)
        public List<OrderProductItemVM> ProductItems { get; set; } = new();

        // Main product counts (ProductName -> Quantity)
        public Dictionary<int, int> MainProductCounts { get; set; } = new();
    }

    public class OrderProductItemVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        // Navigation property
        public Product Product { get; set; }

        public string ProductCat { get; set; }
        public string ProductName { get; set; }
        public string ComboName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

}

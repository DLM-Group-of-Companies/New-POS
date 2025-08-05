namespace NLI_POS.Models.Dto
{
    public class InventoryStockDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int? StockQty { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public int OfficeId { get; set; }

        // For checking low stock based on warehouse min level
        public decimal WarehouseMinLevel { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string OfficeName { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string OfficeName { get; set; }
    }
}

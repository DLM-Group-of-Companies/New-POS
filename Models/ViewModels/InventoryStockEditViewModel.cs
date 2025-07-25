namespace NLI_POS.ViewModels
{
    public class InventoryStockEditViewModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        //public int OfficeId { get; set; }
        //public string OfficeName { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }

        public int StockQty { get; set; }
        public string? Remarks { get; set; }

        public string EncodedBy { get; set; }
        public DateTime EncodeDate { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}

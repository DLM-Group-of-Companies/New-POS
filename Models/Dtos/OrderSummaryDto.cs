namespace NLI_POS.Models.Dtos
{
    public class OrderSummaryDto
    {
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string MobileNumber { get; set; } = "";
        public string OrderType { get; set; } = "";
        public string ProductPurchased { get; set; } = "";
        public decimal Amount { get; set; }
    }

}

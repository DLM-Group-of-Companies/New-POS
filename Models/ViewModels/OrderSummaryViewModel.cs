using static NLI_POS.Pages.Orders.NewModel;

namespace NLI_POS.Models.ViewModels
{
    public class OrderSummaryViewModel
    {
        public Order Order { get; set; }
        public List<ProductItem> ProductItems { get; set; }
        public List<OrderPayment> Payments { get; set; }
    }
}

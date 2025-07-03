namespace NLI_POS.Models.ViewModels
{
    public class ProductWithPricesViewModel
    {
        public Product Product { get; set; }

        // For simplicity, one price per country. Change to `List<ProductPrice>` if needed.
        public ProductPrice ProductPrice { get; set; }

        // Optional: list of countries for dropdown
        public List<Country>? Countries { get; set; }
    }

}

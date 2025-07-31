using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models.ViewModels;

namespace NLI_POS.Pages.Inventory.Shared
{
    public class TransferViewModelBuilder
    {
        private readonly ApplicationDbContext _context;

        public TransferViewModelBuilder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TransferInventoryViewModel> BuildAsync(int fromLocationId, int? productId = null, bool lockFrom = true)
        {
            var fromLocations = await _context.InventoryLocations
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Name
                })
                .ToListAsync();

            var toLocations = await _context.InventoryLocations
                .Where(l => l.IsActive && l.Id != fromLocationId)
                .OrderBy(l => l.Name)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Name
                })
                .ToListAsync();

            List<SelectListItem> products;
            if (productId == null)
            {
                products = await _context.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.ProductName)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.ProductName
                    })
                    .ToListAsync();
            }
            else
            {
                var product = await _context.Products
                    .Where(p => p.Id == productId)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.ProductName
                    })
                    .FirstOrDefaultAsync();

                products = new List<SelectListItem>();
                if (product != null)
                {
                    products.Add(product);
                }
            }


            return new TransferInventoryViewModel
            {
                FromLocationId = fromLocationId,
                ProductId = productId ?? 0,
                LockFrom = lockFrom,
                LocationFromOptions = fromLocations,
                LocationToOptions = toLocations,
                ProductOptions = products
            };
        }

    }
}

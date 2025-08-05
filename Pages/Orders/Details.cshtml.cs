using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using NLI_POS.Models;
using MailKit.Net.Smtp;
using MimeKit.Text;
using NLI_POS.Services;
using NLI_POS.Models.ViewModels;
using static NLI_POS.Pages.Orders.NewModel;

namespace NLI_POS.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DetailsModel(NLI_POS.Data.ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Order Order { get; set; } = default!;
        public OrderSummaryViewModel OrderSummary { get; set; }
        public string EncodedByFullName { get; set; }

        public async Task<IActionResult> OnGetAsync(string? orderNo)
        {
            if (string.IsNullOrEmpty(orderNo))
            {
                return NotFound();
            }

            var baseOrder = await _context.Orders
                .Include(o => o.Office)
                .Include(o => o.Customers)
                    .ThenInclude(c => c.CustClasses)
                .FirstOrDefaultAsync(o => o.OrderNo == orderNo);

            if (baseOrder == null)
            {
                return NotFound();
            }

            Order = baseOrder;

            // Load associated line items (product rows)
            var productItems = await _context.ProductItems
                .Where(pi => pi.OrderId == baseOrder.Id)
                .Select(pi => new ProductItem
                {
                    ProductId = pi.ProductId,
                    ProductCat = pi.ProductCat,
                    ProductName = pi.ProductName,
                    ProductCombo = pi.ProductCombo,
                    ComboName = pi.ComboName,
                    Price = pi.Price,
                    Quantity = pi.Quantity,                    
                    ServiceChargeAmount = pi.ServiceChargeAmount,
                    ServiceChargePct = pi.ServiceChargePct,
                    Amount = pi.Price * pi.Quantity
                })
                .ToListAsync();

            var payments = await _context.OrderPayments
                .Where(p => p.OrderId == baseOrder.Id)
                .ToListAsync();

            OrderSummary = new OrderSummaryViewModel
            {
                Order = baseOrder,
                ProductItems = productItems,
                Payments = payments
            };

            var encoder = await _context.Users
    .FirstOrDefaultAsync(u => u.UserName == Order.EncodedBy);

            EncodedByFullName = encoder != null ? encoder.FullName : "Unknown";

            //await AuditHelpers.LogAsync(HttpContext, _context, User, $"Viewed Order Details of {Order.OrderNo}");
            return Page();
        }


        //public async Task<IActionResult> OnPostVoidAsync()
        //{
        //    if (Order == null || string.IsNullOrEmpty(Order.OrderNo))
        //        return NotFound();

        //    var order = await _context.Orders
        //        .Include(o => o.ProductItems)
        //        .FirstOrDefaultAsync(o => o.OrderNo == Order.OrderNo);

        //    if (order == null)
        //        return NotFound();

        //    if (order.IsVoided)
        //        return RedirectToPage(new { orderNo = order.OrderNo });

        //    // Flag as void
        //    order.IsVoided = true;
        //    order.VoidedDate = DateTime.Now;
        //    order.VoidedBy = User.Identity?.Name ?? "System";

        //    // Restock items
        //    // ✅ Reverse the inventory
        //    if (order.Products != null && order.Products.ProductCategory == "Package")
        //    {
        //        // Package: unpack the combo
        //        var combo = await _context.ProductCombos.FirstOrDefaultAsync(c => c.Id == order.ComboId);
        //        if (combo != null)
        //        {
        //            var productIds = combo.ProductIdList.Split(',').Select(int.Parse).ToList();
        //            var qtyList = combo.QuantityList.Split(',').Select(int.Parse).ToList();

        //            for (int i = 0; i < productIds.Count; i++)
        //            {
        //                int componentProductId = productIds[i];
        //                int restockQty = qtyList[i] * order.Qty.Value;

        //                var inventory = await _context.InventoryStocks
        //                    .FirstOrDefaultAsync(i => i.ProductId == componentProductId && i.OfficeId == order.OfficeId);

        //                if (inventory != null)
        //                {
        //                    inventory.StockQty += restockQty;
        //                    _context.InventoryStocks.Update(inventory);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // Regular product
        //        var inventory = await _context.InventoryStocks
        //            .FirstOrDefaultAsync(i => i.ProductId == order.ProductId && i.OfficeId == order.OfficeId);

        //        if (inventory != null)
        //        {
        //            inventory.StockQty += order.Qty.Value;
        //            _context.InventoryStocks.Update(inventory);
        //        }
        //    }

        //     await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] = "Order has been successfully voided.";
        //    return RedirectToPage(new { orderNo = order.OrderNo });
        //}

        public async Task<IActionResult> OnPostVoidAsync()
        {
            if (Order == null || string.IsNullOrEmpty(Order.OrderNo))
                return NotFound();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNo == Order.OrderNo);

            if (order == null)
                return NotFound();

            if (order.IsVoided)
                return RedirectToPage(new { orderNo = order.OrderNo });

            // Flag as void
            order.IsVoided = true;
            order.VoidedDate = DateTime.UtcNow;
            order.VoidedBy = User.Identity?.Name ?? "System";
            _context.Orders.Update(order);

            // ✅ Load all associated items
            var productItems = await _context.ProductItems
                .Where(p => p.OrderId == order.Id)
                .ToListAsync();

            foreach (var item in productItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) continue;

                if (product.ProductCategory == "Package")
                {
                    // Package: restock components
                    var combo = await _context.ProductCombos.FirstOrDefaultAsync(c => c.Id == item.ProductCombo);
                    if (combo != null)
                    {
                        var productIds = combo.ProductIdList.Split(',').Select(int.Parse).ToList();
                        var qtyList = combo.QuantityList.Split(',').Select(int.Parse).ToList();

                        for (int i = 0; i < productIds.Count; i++)
                        {
                            int componentProductId = productIds[i];
                            int restockQty = qtyList[i] * item.Quantity;

                            var inventory = await _context.InventoryStocks
                                .FirstOrDefaultAsync(inv => inv.ProductId == componentProductId && inv.Location.OfficeId == order.OfficeId);

                            if (inventory != null)
                            {
                                inventory.StockQty += restockQty;
                                _context.InventoryStocks.Update(inventory);

                                //Log Inventory Trans
                                _context.InventoryTransactions.Add(new InventoryTransaction
                                {
                                    OrderNo = Order.OrderNo,
                                    ProductId = componentProductId,
                                    FromLocationId = inventory.LocationId,
                                    ToLocationId = null,
                                    Quantity = -restockQty,
                                    TransactionType = "Void",
                                    TransactionDate = DateTime.UtcNow,
                                    EncodedBy = User.Identity?.Name ?? "SYSTEM"
                                });

                            }
                        }
                    }
                }
                else
                {
                    // Regular product restock
                    var inventory = await _context.InventoryStocks
                        .FirstOrDefaultAsync(inv => inv.ProductId == item.ProductId && inv.Location.OfficeId == order.OfficeId);

                    if (inventory != null)
                    {
                        inventory.StockQty += item.Quantity;
                        _context.InventoryStocks.Update(inventory);

                        //Log Inventory Trans
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            OrderNo = Order.OrderNo,
                            ProductId = item.ProductId,
                            FromLocationId = inventory.LocationId,
                            ToLocationId = null,
                            Quantity = -item.Quantity,
                            TransactionType = "Void",
                            TransactionDate = DateTime.UtcNow,
                            EncodedBy = User.Identity?.Name ?? "SYSTEM"
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Voided Order {Order.OrderNo}");

            TempData["SuccessMessage"] = "Order has been successfully voided.";
            return RedirectToPage(new { orderNo = order.OrderNo });
        }


        public class EmailRequest
        {
            public string Html { get; set; }
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostEmailModalAsync([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Html))
                return new JsonResult(new { success = false, error = "Missing email or HTML content." });

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("NobleLife POS", "pos.nlip@gmail.com"));
                message.To.Add(MailboxAddress.Parse(request.Email));
                message.Subject = "Purchase Order Receipt";

                var builder = new BodyBuilder
                {
                    HtmlBody = request.Html
                };

                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync("pos.nlip@gmail.com", "kqqjvtwtsgwckbnc");
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public class HtmlContentModel
        {
            public string Html { get; set; }
        }
    }
}

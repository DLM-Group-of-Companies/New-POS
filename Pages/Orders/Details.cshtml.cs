using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using NLI_POS.Models;
using MailKit.Net.Smtp;
using MimeKit.Text;

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

        public Order Order { get; set; } = default!;
        public IList<Order> OrderList { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync(string? orderNo)
        {
            if (orderNo == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.Include(o => o.Office).Include(o=>o.Customers).ThenInclude(c => c.CustClasses).Include(p=>p.ProductCombos).FirstOrDefaultAsync(m => m.OrderNo == orderNo);
            if (order == null)
            {
                return NotFound();
            }
            else
            {
                Order = order;
            }

            var orderlist =  _context.Orders.Include(o=>o.Products).Where(m => m.OrderNo == order.OrderNo).ToList();
            OrderList = orderlist;
            return Page();
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

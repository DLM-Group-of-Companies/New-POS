using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Countries
{
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Country> Country { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Country = await _context.Country.ToListAsync();
        }

        public async Task<JsonResult> OnGetGetAsync(int id)
        {
            var country = _context.Country.FirstOrDefault(c => c.Id == id);
            if (country == null)
                return new JsonResult(NotFound());

            //await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Countries List");

            return new JsonResult(new
            {
                id = country.Id,
                code = country.Code,
                name = country.Name,
                timezone = country.TimeZone,
                locale = country.Locale,
                currency = country.CurrencyCode,
                isActive = country.IsActive
            });

        }

    }
}

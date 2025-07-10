using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Services
{
    //I created this to trim down coding of getting Country and Office per User
    public abstract class BasePageModel : PageModel
    {
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BasePageModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class OfficeSelectItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
            public string Locale { get; set; }
            public string CurrencyCode { get; set; }
        }


        //public async Task<List<SelectListItem>> GetUserOfficesAsync()
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    List<SelectListItem> officeList;

        //    if (await _userManager.IsInRoleAsync(user, "Admin"))
        //    {
        //        // Admin can access all offices
        //        officeList = await _context.OfficeCountry
        //            .Include(o => o.Country) // to include the locale and currency
        //            .Select(o => new SelectListItem
        //            {
        //                Value = o.Id.ToString(),
        //                Text = o.Name
        //            })
        //            .ToListAsync();
        //    }
        //    else
        //    {
        //        // Non-admin: only assigned offices
        //        officeList = await _context.UserOfficesAccess
        //            .Where(x => x.UserId == user.Id)
        //            .Include(x => x.OfficeCountry).ThenInclude(o=>o.Country)
        //                .Select(x => new SelectListItem
        //                {
        //                    Value = x.OfficeId.ToString(),
        //                    Text = x.OfficeCountry.Name
        //                })
        //            .ToListAsync();
        //    }


        //    return officeList;
        //}

        public async Task<List<OfficeSelectItem>> GetUserOfficesAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return await _context.OfficeCountry
                    .Include(o => o.Country)
                    .Select(o => new OfficeSelectItem
                    {
                        Value = o.Id.ToString(),
                        Text = o.Name,
                        Locale = o.Country.Locale,
                        CurrencyCode = o.Country.CurrencyCode
                    })
                    .ToListAsync();
            }
            else
            {
                return await _context.UserOfficesAccess
                    .Where(x => x.UserId == user.Id)
                    .Include(x => x.OfficeCountry).ThenInclude(o => o.Country)
                    .Select(x => new OfficeSelectItem
                    {
                        Value = x.OfficeId.ToString(),
                        Text = x.OfficeCountry.Name,
                        Locale = x.OfficeCountry.Country.Locale,
                        CurrencyCode = x.OfficeCountry.Country.CurrencyCode
                    })
                    .ToListAsync();
            }
        }

    }

}

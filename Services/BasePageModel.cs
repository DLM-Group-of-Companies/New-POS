using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Services
{
    public abstract class BasePageModel : PageModel
    {
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BasePageModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<SelectListItem>> GetUserOfficesAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            //var officeList = await _context.UserOfficesAccess
            //    .Where(x => x.UserId == user.Id)
            //    .Include(x => x.OfficeCountry)
            //    .Select(x => new SelectListItem
            //    {
            //        Value = x.OfficeId.ToString(),
            //        Text = x.OfficeCountry.Name
            //    })
            //    .ToListAsync();

            List<SelectListItem> officeList;

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // Admin can access all offices
                officeList = await _context.OfficeCountry
                    .Select(o => new SelectListItem
                    {
                        Value = o.Id.ToString(),
                        Text = o.Name
                    })
                    .ToListAsync();
            }
            else
            {
                // Non-admin: only assigned offices
                officeList = await _context.UserOfficesAccess
                    .Where(x => x.UserId == user.Id)
                    .Include(x => x.OfficeCountry)
                    .Select(x => new SelectListItem
                    {
                        Value = x.OfficeId.ToString(),
                        Text = x.OfficeCountry.Name
                    })
                    .ToListAsync();
            }


            return officeList;
        }
    }

}

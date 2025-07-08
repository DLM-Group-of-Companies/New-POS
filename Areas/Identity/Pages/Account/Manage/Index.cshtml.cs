// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NLI_POS.Data;
using NLI_POS.Models;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private ApplicationDbContext _context;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public string Username { get; set; }

        [BindProperty]
        [Display(Name = "Full Name")]
        public string Fullname { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        [BindProperty]
        public List<string> SelectedRoles { get; set; } = new();
        public List<SelectListItem> AllRoles { get; set; }


        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync(string? pEmail)
        {
            if (pEmail == null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                SelectedRoles = userRoles.ToList();

                AllRoles = _roleManager.Roles
            .Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = userRoles.Contains(r.Name) // ✅ Preselect here too
            }).ToList();

                Fullname = user.FullName;
                var roles = await _userManager.GetRolesAsync(user);                
                await LoadAsync(user);
            }
            else
            {
                var sUser = await _userManager.FindByEmailAsync(pEmail);
                if (sUser != null)
                {
                    Fullname = sUser.FullName;
                    var userRoles = await _userManager.GetRolesAsync(sUser);
                    SelectedRoles = userRoles.ToList();
                    await LoadAsync(sUser);
                }
            }

            return Page();
        }

         public async Task<IActionResult> OnPostAsync(string? pEmail)
        {
            ApplicationUser user;
            if (pEmail == null)
            {
                user = await _userManager.GetUserAsync(User);
            }
            else
            {
                user = await _userManager.FindByEmailAsync(pEmail);
            }
            //var user = await _userManager.GetUserAsync(User);
            //var user = await _userManager.FindByEmailAsync(pEmail);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }

            }

            var oldRole = await _userManager.GetRolesAsync(user);
            if (oldRole.Count > 0)
            {
                await _userManager.RemoveFromRoleAsync(user, oldRole[0]);
            }

            string strDDLRole = Request.Form["ddlRole"].ToString();
            if (strDDLRole != null && strDDLRole != "")
            {
                await _userManager.AddToRoleAsync(user, strDDLRole);
            }
            user.FullName = Fullname;
            await _userManager.UpdateAsync(user);
            //await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";

            if (pEmail == null)
                return RedirectToPage();
            else
                return RedirectToPage("Index", new { pemail = pEmail });
        }
    }
}
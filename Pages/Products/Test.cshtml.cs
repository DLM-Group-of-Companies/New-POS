using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace NLI_POS.Pages.Products
{
    public class TestModel : PageModel
    {

    [BindProperty]
        public string EncodedList { get; set; }

        public List<string> DisplayedListValues { get; set; }

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(EncodedList))
            {
                DisplayedListValues = JsonSerializer.Deserialize<List<string>>(EncodedList);
            }
            else
            {
                DisplayedListValues = new List<string>();
            }
        }

        public IActionResult OnPost()
        {
            if (!string.IsNullOrEmpty(EncodedList))
            {
                var submittedList = JsonSerializer.Deserialize<List<string>>(EncodedList);
                // Process the submittedList as needed
                DisplayedListValues = submittedList; // For demonstration
            }
            else
            {
                DisplayedListValues = new List<string>();
            }

            return Page();
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwitchWebManager.Models;

namespace SwitchWebManager.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public SwitchModel Switch { get; set; } = new SwitchModel();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                return RedirectToPage("ManagePorts", new { ipAddress = Switch.IpAddress });
            }
            return Page();
        }
    }
}
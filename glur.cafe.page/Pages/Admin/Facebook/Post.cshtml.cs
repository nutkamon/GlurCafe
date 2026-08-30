using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace glur.cafe.page.Pages.Admin.Facebook;

[Authorize]
public class PostModel : PageModel
{
    public void OnGet() { }
}

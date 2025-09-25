using API.ViewModels.Token;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected string GetUserId()
        {
            return User.FindFirst("UserID")?.Value ?? string.Empty;
        }
    }
}

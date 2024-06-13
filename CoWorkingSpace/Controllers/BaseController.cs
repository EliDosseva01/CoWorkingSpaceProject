using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoWorkingSpace.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        ///  </summary>
        /// <returns>Returns the id of the currently logged in user.</returns>
        [HttpGet]   
        protected string GetLoggedUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}

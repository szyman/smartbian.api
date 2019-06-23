using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartRoomsApp.API.Controllers
{
    [AllowAnonymous]
    public class FallBack : Controller
    {
        public IActionResult Index(string challenge)
        {
            return PhysicalFile(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sslFile"),
                "text/plain"
            );
        }
    }
}
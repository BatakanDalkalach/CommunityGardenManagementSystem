using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        // Handles 404 Not Found responses
        [Route("404")]
        public IActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        // Handles all other error status codes (500, 403, etc.)
        [Route("{statusCode:int}")]
        public IActionResult HandleError(int statusCode)
        {
            Response.StatusCode = statusCode;
            return View("ServerError");
        }
    }
}

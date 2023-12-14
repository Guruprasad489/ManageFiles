using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManageFiles.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PgpController : ControllerBase
    {
        [HttpGet("Get")]
        public ActionResult Get()
        {
            return Ok();
        }
    }
}

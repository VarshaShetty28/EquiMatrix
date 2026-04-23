using Microsoft.AspNetCore.Mvc;

namespace EquityAwardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Controller is working!");
        }
    }
}
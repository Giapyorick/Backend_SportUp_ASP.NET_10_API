using Microsoft.AspNetCore.Mvc;

namespace SportUp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestHealthController : ControllerBase
{
    /// <summary>
    /// API kiểm tra xem Controller mới có được Scalar / Swagger quét thấy hay không
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            status = "Success",
            message = "TestHealthController is working fine!",
            serverTime = DateTime.UtcNow
        });
    }
}
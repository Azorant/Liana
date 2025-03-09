using Microsoft.AspNetCore.Mvc;

namespace Liana.API.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    [HttpGet("get")]
    public string Test()
    {
        return "Test";
    }
}
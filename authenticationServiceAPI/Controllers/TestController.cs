using authenticationServiceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using authenticationServiceAPI.Models;

namespace authenticationServiceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public TestController(ILogger<AuthController> logger, IConfiguration config)
        {
            _config = config;
            _logger = logger;
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("You're authorized");
        }


    }
}
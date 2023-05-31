using authenticationServiceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Permissions;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Net.Http;
using System.Threading.Tasks;

//Startup --> . ./startup.sh
//Token -->  https://jwt.io/
//Get --> Authorization --> Bearer token


namespace authenticationServiceAPI.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, IConfiguration config)
    {
        _config = config;
        _logger = logger;

        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"Taxabooking responding from {_ipaddr}");


    }




    //Metode til at oprette JWT tokens med brugerdefineret felt.
    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Secret"] ?? string.Empty));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, username)
        };
        var token = new JwtSecurityToken(
        _config["Issuer"],
        _config["ValidAudience" ?? "http://localhost"],
        claims,
        expires: DateTime.Now.AddMinutes(15),
        signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("version")]
    public IEnumerable<string> Get()
    {
        var properties = new List<string>();
        var assembly = typeof(Program).Assembly;
        foreach (var attribute in assembly.GetCustomAttributesData())
        {
            properties.Add($"{attribute.AttributeType.Name} - {attribute.ToString()}");
        }
        return properties;
    }

    //Post - Login
    [AllowAnonymous]  //Alle kan oprette
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        try
        {
            string url = _config["apiGetUser"] ?? string.Empty;
            _logger.LogInformation("Getting prepared to check login with: " + "http://" + url + $"?userName={login.UserName}&userPassword={login.UserPassword}");
            HttpResponseMessage response = await _httpClient.GetAsync("http://" + url + $"?userName={login.UserName}&userPassword={login.UserPassword}");
            
            response.EnsureSuccessStatusCode(); 

            string responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("ResponsseContennt: " + responseContent);
            UserWithToken userWithToken = JsonConvert.DeserializeObject<UserWithToken>(responseContent);

            if (login.UserName != userWithToken.UserName || login.UserPassword != userWithToken.UserPassword)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Now I have a token");
            var token = GenerateJwtToken(login.UserName);

            userWithToken.Token = token;

            return Ok(userWithToken);

        }
        catch (System.Exception ex)
        {

            _logger.LogInformation(ex.Message);
            return BadRequest(ex.Message);
        }
    }



    
    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateJwtToken([FromBody] string? token)
    {

        if (token.IsNullOrEmpty())
            return BadRequest("Invalid token submited.");
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Secret"]!);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountId = jwtToken.Claims.First(
            x => x.Type == ClaimTypes.NameIdentifier).Value;
            return Ok(accountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(404);
        }
    }

}
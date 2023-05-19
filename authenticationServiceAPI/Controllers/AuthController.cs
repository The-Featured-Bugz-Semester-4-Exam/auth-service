using authenticationServiceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Permissions;
using Newtonsoft.Json;
using authenticationServiceAPI.Models;
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
    }




    //Metode til at oprette JWT tokens med brugerdefineret felt.
    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Secret"]));
        
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, username)
        };
        var token = new JwtSecurityToken(
        _config["Issuer"],
        "http://localhost",
        claims,
        expires: DateTime.Now.AddMinutes(15),
        signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }



    //Post - Login
    [AllowAnonymous]  //Alle kan oprette
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        try
        {
            string url = _config["apiGetUser"] ?? string.Empty;

            HttpResponseMessage response = await _httpClient.GetAsync(url + $"?userName={login.UserName}&userPassword={login.UserPassword}");
            _logger.LogInformation("Lad mig logge ind: " + url + " og her er mit response: " + response);

            response.EnsureSuccessStatusCode(); // Kaster en exception, hvis responsen ikke er en succes (HTTP status 2xx)

            string responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("ResponsseContennt: " + responseContent);
            UserWithToken userWithToken = JsonConvert.DeserializeObject<UserWithToken>(responseContent);
        
            if (login.UserName != userWithToken.UserName || login.UserPassword != userWithToken.UserPassword)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Nu har jeg modtaget en token");
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



    //Valider et login
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
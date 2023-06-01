using authenticationServiceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;

//Startup --> . ./startup.sh
//Token -->  https://jwt.io/
//Get --> Authorization --> Bearer token


namespace authenticationServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, IConfiguration config)
    {
        _config = config;
        _logger = logger;

        // Get the host name and IP address for logging purposes
        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"Authentication responding from {_ipaddr}");
    }

    // Method to generate JWT token with custom field
    private string GenerateJwtToken(string username)
    {
        // Create a security key using the secret from the configuration
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Secret"] ?? string.Empty));

        // Create credentials for the token with the security key and algorithm
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Create claims for the token with the username
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, username)
    };

        // Create a JWT token with the specified issuer, audience, claims, expiration, and signing credentials
        var token = new JwtSecurityToken(
            _config["Issuer"],
            _config["ValidAudience"] ?? "http://localhost",
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);

        // Return the serialized JWT token as a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    // Get API endpoint to retrieve assembly version information
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

    // Login API endpoint
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        try
        {
            string url = _config["apiGetUser"] ?? string.Empty;
            _logger.LogInformation("Getting prepared to check login with: " + "http://" + url + $"?userName={login.UserName}&userPassword={login.UserPassword}");

            // Send a GET request to validate the login credentials
            HttpResponseMessage response = await _httpClient.GetAsync("http://" + url + $"?userName={login.UserName}&userPassword={login.UserPassword}");

            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            string responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("ResponseContent: " + responseContent);

            // Deserialize the response content into a UserWithToken object
            UserWithToken userWithToken = JsonConvert.DeserializeObject<UserWithToken>(responseContent);

            // Check if the provided login credentials match the retrieved user credentials
            if (login.UserName != userWithToken.UserName || login.UserPassword != userWithToken.UserPassword)
            {
                // Return an Unauthorized response if the credentials don't match
                return Unauthorized();
            }

            _logger.LogInformation("Now I have a token");

            // Generate a JWT token for the authenticated user
            var token = GenerateJwtToken(login.UserName);

            // Assign the generated token to the userWithToken object
            userWithToken.Token = token;

            // Return an OK response with the userWithToken object containing the token
            return Ok(userWithToken);
        }
        catch (System.Exception ex)
        {
            // Log the exception message
            _logger.LogInformation(ex.Message);

            // Return a NotFound response with the exception message
            return NotFound("Can't find the user");
        }
    }

    // Validate JWT token API endpoint
    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateJwtToken([FromBody] string? token)
    {
        // Check if the token is null or empty
        if (token.IsNullOrEmpty())
            return BadRequest("Invalid token submitted.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Secret"]!);
        try
        {
            // Validate the submitted token using the provided validation parameters
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // Extract the account ID from the validated token
            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

            // Return an OK response with the account ID
            return Ok(accountId);
        }
        catch (Exception ex)
        {
            // Log the exception and return a StatusCode 404 response
            _logger.LogError(ex, ex.Message);
            return StatusCode(404);
        }
    }
}
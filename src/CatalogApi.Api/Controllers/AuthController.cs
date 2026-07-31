using CatalogApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApi.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenGenerator _jwt;
    private readonly IConfiguration _config;

    public AuthController(IJwtTokenGenerator jwt, IConfiguration config)
    {
        _jwt = jwt;
        _config = config;
    }

    public record LoginRequest(string Username, string Password);

    [HttpPost("login")]
    public IActionResult Login(LoginRequest req)
    {
        var user = _config["SeedUser:Username"];
        var pass = _config["SeedUser:Password"];
        if (req.Username != user || req.Password != pass)
            return Unauthorized("Credenciales inválidas");

        return Ok(new { token = _jwt.Generate(req.Username, new[] { "admin" }) });
    }
}

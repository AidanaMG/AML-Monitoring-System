using Aml.Api.Services;
using Aml.Domain.Entities;
using Aml.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aml.Api.Dtos;

namespace Aml.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AmlDbContext _db;
        private readonly JwtTokenService _tokenService;
        public AuthController(AmlDbContext db, JwtTokenService tokenService) 
        {
            _db = db;
            _tokenService = tokenService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == request.UserName, ct);

            if(user is null || !user.IsActive)
                return Unauthorized("Invalid credentials");

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if(result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials");

            var token = _tokenService.CreateToken(user);

            return Ok(new { token, userName = user.UserName, role = user.Role.ToString()});
        }
    }
}

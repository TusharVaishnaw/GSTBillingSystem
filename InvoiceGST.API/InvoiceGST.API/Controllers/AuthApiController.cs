using InvoiceGST.API.Data;
using InvoiceGST.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InvoiceGST.API.Services;


namespace InvoiceGST.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AuthApiController(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context, EmailService emailService )
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // role validation
            if (model.Role != "Admin" && model.Role != "User")
                return BadRequest("Invalid role. Only Admin or User allowed.");

            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok($"User registered successfully with role {model.Role}");
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
             new Claim(ClaimTypes.Name, user.Email),
             new Claim("FullName", user.FullName),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

          
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_SUPER_SECRET_KEY_FOR_JWT_AUTHENTICATION_2026_INVOICE_PROJECT_123456789"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked);

            if (token == null || token.ExpiryDate < DateTime.Now)
                return Unauthorized("Invalid refresh token");

            var user = await _userManager.FindByIdAsync(token.UserId);

            // new access token generate
            var newAccessToken = GenerateJwtToken(user);

            return Ok(new
            {
                accessToken = newAccessToken
            });
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var token = Guid.NewGuid().ToString();

            // Save token in DB (User table ya alag table)

            await _emailService.SendEmail(dto.Email, "Reset Password", $"Token: {token}");

            return Ok("Reset email sent");
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_SUPER_SECRET_KEY_FOR_JWT_AUTHENTICATION_2026_INVOICE_PROJECT_123456789"));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("FullName", user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
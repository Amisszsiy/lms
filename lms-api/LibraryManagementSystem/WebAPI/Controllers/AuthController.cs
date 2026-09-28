using Application.Errors;
using Application.Features.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net;
using System.Security.Claims;
using WebAPI.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private const string _accessToken = "AccessToken";

        public AuthController(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.Register(request, cancellationToken);
            if (result.IsFailure) return BadRequest(result.Error.Code);

            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.Login(request, cancellationToken);
            if (result.IsFailure)
            {
                if (result.Error == UserServiceErrors.InvalidCredentials) return Unauthorized(result.Error.Code);
                if (result.Error == UserServiceErrors.UserInactive) return StatusCode(StatusCodes.Status403Forbidden, result.Error.Code);
                return BadRequest(result.Error.Code);
            }

            var tokenCookieOption = CreateTokenCookieOptions();
            tokenCookieOption.Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInMinutes"));

            Response.Cookies.Append(_accessToken, result.Value, tokenCookieOption);

            return NoContent();
        }

        // Token lives in an HttpOnly cookie, so the client calls this to learn whether it is still signed in and who it is.
        [HttpGet("check-user")]
        [Authorize]
        [ProducesResponseType(typeof(CheckUserResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckUser(CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId)) return SignOutUnauthorized();

            var result = await _userService.GetUser(userId, cancellationToken);
            // Token is still valid but the account was removed or deactivated after it was issued.
            if (result.IsFailure || !result.Value.IsActive) return SignOutUnauthorized();

            UserResponse user = result.Value;
            long expiresAt = long.Parse(User.FindFirst(JwtRegisteredClaimNames.Exp).Value);

            return Ok(new CheckUserResult
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Tel = user.Tel,
                // Roles come from the token. Used in client to decide which menu can be activated.
                Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAt).UtcDateTime
            });
        }

        // Anonymous so a client holding an expired cookie can still clear it.
        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(_accessToken, CreateTokenCookieOptions());

            return NoContent();
        }

        private IActionResult SignOutUnauthorized()
        {
            Response.Cookies.Delete(_accessToken, CreateTokenCookieOptions());
            return Unauthorized();
        }

        private static CookieOptions CreateTokenCookieOptions() => new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = true,
            Path = "/"
        };
    }
}

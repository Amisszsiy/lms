using Application.Common;
using Application.Errors;
using Application.Features.Users;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // e.g. api/User?search=john&isActive=true for picking a borrower.
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List([FromQuery] SearchUsersRequest request, CancellationToken cancellationToken)
        {
            Result<PagedResponse<UserResponse>> result = await _userService.SearchUsers(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId)) return Unauthorized();

            // Admins can view any user, end users only themselves.
            if (!User.IsInRole(RoleNames.Admin) && id != userId) return NotFound(RepositoryErrors.EntityNotFound.Code);

            Result<UserResponse> result = await _userService.GetUser(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        private IActionResult Failure(Result result)
        {
            if (result is ValidationResult<PagedResponse<UserResponse>> searchValidationResult) return BadRequest(searchValidationResult.Errors.Select(e => e.Code));
            if (result.Error == RepositoryErrors.EntityNotFound) return NotFound(result.Error.Code);
            return BadRequest(result.Error.Code);
        }
    }
}

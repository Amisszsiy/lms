using Application.Common;
using Application.Errors;
using Application.Features.Borrowings;
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
    public class BorrowController : ControllerBase
    {
        private readonly BorrowingService _borrowingService;
        public BorrowController(BorrowingService borrowingService)
        {
            _borrowingService = borrowingService;
        }

        // All transactions for admins, e.g. api/Borrow?status=ReturnRequested lists the returns waiting to be processed.
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(PagedResponse<BorrowingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List([FromQuery] SearchBorrowingsRequest request, CancellationToken cancellationToken)
        {
            Result<PagedResponse<BorrowingResponse>> result = await _borrowingService.SearchBorrowings(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        // Current user's own borrowings.
        [HttpGet("me")]
        [ProducesResponseType(typeof(PagedResponse<BorrowingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListMine([FromQuery] SearchBorrowingsRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out Guid userId)) return Unauthorized();

            request.UserId = userId;

            Result<PagedResponse<BorrowingResponse>> result = await _borrowingService.SearchBorrowings(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BorrowingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out Guid userId)) return Unauthorized();

            // Admins can view any borrowing, end users only their own.
            Guid? ownerId = User.IsInRole(RoleNames.Admin) ? null : userId;

            Result<BorrowingResponse> result = await _borrowingService.GetBorrowing(id, ownerId, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        // Current user borrows a copy.
        [HttpPost]
        [ProducesResponseType(typeof(BorrowingResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Borrow(BorrowRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out Guid userId)) return Unauthorized();

            Result<BorrowingResponse> result = await _borrowingService.Borrow(userId, request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        // Admin assigns a copy directly to a user.
        [HttpPost("assign")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(BorrowingResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Assign(AssignBorrowRequest request, CancellationToken cancellationToken)
        {
            Result<BorrowingResponse> result = await _borrowingService.AssignBorrow(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        // Current user sends a return request for their own borrowing: Borrowed -> IssueReturn.
        [HttpPost("{id:guid}/return-request")]
        [ProducesResponseType(typeof(BorrowingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestReturn(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out Guid userId)) return Unauthorized();

            Result<BorrowingResponse> result = await _borrowingService.RequestReturn(id, userId, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        // Admin processes the return: closes the loan and makes the copy Available.
        [HttpPost("{id:guid}/return")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(BorrowingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessReturn(Guid id, CancellationToken cancellationToken)
        {
            Result<BorrowingResponse> result = await _borrowingService.ProcessReturn(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        private bool TryGetCurrentUserId(out Guid userId) =>
            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);

        private IActionResult Failure(Result result)
        {
            if (result is ValidationResult<BorrowingResponse> validationResult) return BadRequest(validationResult.Errors.Select(e => e.Code));
            if (result is ValidationResult<PagedResponse<BorrowingResponse>> searchValidationResult) return BadRequest(searchValidationResult.Errors.Select(e => e.Code));
            if (result.Error == RepositoryErrors.EntityNotFound) return NotFound(result.Error.Code);
            return BadRequest(result.Error.Code);
        }
    }
}

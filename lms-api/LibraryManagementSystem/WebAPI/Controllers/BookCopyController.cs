using Application.Common;
using Application.Errors;
using Application.Features.BookCopies;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookCopyController : ControllerBase
    {
        private readonly BookCopyService _bookCopyService;
        public BookCopyController(BookCopyService bookCopyService)
        {
            _bookCopyService = bookCopyService;
        }

        // Copies of one book, e.g. api/BookCopy?bookId=1
        [HttpGet]
        [ProducesResponseType(typeof(List<BookCopyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List([FromQuery, BindRequired] long bookId, [FromQuery] bool includeWithdrawn, CancellationToken cancellationToken)
        {
            // Withdrawn copies are only visible to admins.
            includeWithdrawn &= User.IsInRole(RoleNames.Admin);

            Result<List<BookCopyResponse>> result = await _bookCopyService.GetBookCopies(bookId, includeWithdrawn, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            Result<BookCopyResponse> result = await _bookCopyService.GetBookCopy(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(CreateBookCopyRequest request, CancellationToken cancellationToken)
        {
            Result<BookCopyResponse> result = await _bookCopyService.AddBookCopy(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        // Moves the copy and/or changes its status. Borrowed/returned is handled by the borrowing flow.
        [HttpPut("{id:guid}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(Guid id, UpdateBookCopyRequest request, CancellationToken cancellationToken)
        {
            Result<BookCopyResponse> result = await _bookCopyService.UpdateBookCopy(id, request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        // Soft delete: withdraws the copy so its borrowing history is kept.
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            Result result = await _bookCopyService.DeleteBookCopy(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return NoContent();
        }

        private IActionResult Failure(Result result)
        {
            if (result is ValidationResult<BookCopyResponse> validationResult) return BadRequest(validationResult.Errors.Select(e => e.Code));
            if (result.Error == RepositoryErrors.EntityNotFound) return NotFound(result.Error.Code);
            return BadRequest(result.Error.Code);
        }
    }
}

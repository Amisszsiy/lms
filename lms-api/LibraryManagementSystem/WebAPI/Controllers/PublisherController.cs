using Application.Common;
using Application.Errors;
using Application.Features.Publishers;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PublisherController : ControllerBase
    {
        private readonly PublisherService _publisherService;
        public PublisherController(PublisherService publisherService)
        {
            _publisherService = publisherService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<PublisherResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List([FromQuery] SearchPublishersRequest request, CancellationToken cancellationToken)
        {
            Result<PagedResponse<PublisherResponse>> result = await _publisherService.SearchPublishers(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PublisherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            Result<PublisherResponse> result = await _publisherService.GetPublisher(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(PublisherResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(SavePublisherRequest request, CancellationToken cancellationToken)
        {
            Result<PublisherResponse> result = await _publisherService.AddPublisher(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(PublisherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, SavePublisherRequest request, CancellationToken cancellationToken)
        {
            Result<PublisherResponse> result = await _publisherService.UpdatePublisher(id, request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        // Hard delete: fails while any book still references the publisher.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            Result result = await _publisherService.DeletePublisher(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return NoContent();
        }

        private IActionResult Failure(Result result)
        {
            if (result is ValidationResult<PublisherResponse> validationResult) return BadRequest(validationResult.Errors.Select(e => e.Code));
            if (result is ValidationResult<PagedResponse<PublisherResponse>> searchValidationResult) return BadRequest(searchValidationResult.Errors.Select(e => e.Code));
            if (result.Error == RepositoryErrors.EntityNotFound) return NotFound(result.Error.Code);
            return BadRequest(result.Error.Code);
        }
    }
}

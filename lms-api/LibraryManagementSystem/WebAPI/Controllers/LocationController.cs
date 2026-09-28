using Application.Common;
using Application.Errors;
using Application.Features.Locations;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;
        public LocationController(LocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<LocationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List([FromQuery] SearchLocationsRequest request, CancellationToken cancellationToken)
        {
            Result<PagedResponse<LocationResponse>> result = await _locationService.SearchLocations(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            Result<LocationResponse> result = await _locationService.GetLocation(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(SaveLocationRequest request, CancellationToken cancellationToken)
        {
            Result<LocationResponse> result = await _locationService.AddLocation(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, SaveLocationRequest request, CancellationToken cancellationToken)
        {
            Result<LocationResponse> result = await _locationService.UpdateLocation(id, request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        // Hard delete: fails while any book copy is still shelved at the location.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            Result result = await _locationService.DeleteLocation(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return NoContent();
        }

        private IActionResult Failure(Result result)
        {
            if (result is ValidationResult<LocationResponse> validationResult) return BadRequest(validationResult.Errors.Select(e => e.Code));
            if (result is ValidationResult<PagedResponse<LocationResponse>> searchValidationResult) return BadRequest(searchValidationResult.Errors.Select(e => e.Code));
            if (result.Error == RepositoryErrors.EntityNotFound) return NotFound(result.Error.Code);
            return BadRequest(result.Error.Code);
        }
    }
}

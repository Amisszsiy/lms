using Application.Common;
using Application.Errors;
using Application.Features.Genres;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GenreController : ControllerBase
    {
        private readonly GenreService _genreService;
        public GenreController(GenreService genreService)
        {
            _genreService = genreService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<GenreResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List([FromQuery] SearchGenresRequest request, CancellationToken cancellationToken)
        {
            Result<PagedResponse<GenreResponse>> result = await _genreService.SearchGenres(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            Result<GenreResponse> result = await _genreService.GetGenre(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(SaveGenreRequest request, CancellationToken cancellationToken)
        {
            Result<GenreResponse> result = await _genreService.AddGenre(request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, SaveGenreRequest request, CancellationToken cancellationToken)
        {
            Result<GenreResponse> result = await _genreService.UpdateGenre(id, request, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return Ok(result.Value);
        }

        // Hard delete: fails while any book still references the genre.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            Result result = await _genreService.DeleteGenre(id, cancellationToken);
            if (result.IsFailure) return Failure(result);

            return NoContent();
        }

        private IActionResult Failure(Result result)
        {
            if (result is ValidationResult<GenreResponse> validationResult) return BadRequest(validationResult.Errors.Select(e => e.Code));
            if (result is ValidationResult<PagedResponse<GenreResponse>> searchValidationResult) return BadRequest(searchValidationResult.Errors.Select(e => e.Code));
            if (result.Error == RepositoryErrors.EntityNotFound) return NotFound(result.Error.Code);
            return BadRequest(result.Error.Code);
        }
    }
}

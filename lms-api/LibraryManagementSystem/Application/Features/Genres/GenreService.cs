using Application.Common;
using Application.Errors;
using Application.Persistance;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Genres
{
    public class GenreService
    {
        private readonly IGenreRepository _repository;
        private readonly IMapper _mapper;
        private readonly GenreValidator _validator;
        private readonly SearchGenresValidator _searchValidator;
        public GenreService(IGenreRepository repository, IMapper mapper, GenreValidator validator, SearchGenresValidator searchValidator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
            _searchValidator = searchValidator;
        }

        public async Task<Result<GenreResponse>> GetGenre(int id, CancellationToken cancellationToken = default)
        {
            Genre genre = await _repository.GetAsync(g => g.Id == id, cancellationToken: cancellationToken);
            if (genre is null) return Result<GenreResponse>.Failure(RepositoryErrors.EntityNotFound);

            return Result<GenreResponse>.Success(_mapper.Map<GenreResponse>(genre));
        }

        public async Task<Result<PagedResponse<GenreResponse>>> SearchGenres(SearchGenresRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _searchValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PagedResponse<GenreResponse>>.From(validateResult.Errors);

            string search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

            Expression<Func<Genre, bool>> filter = g => search == null || g.GenreName.Contains(search);

            int totalCount = await _repository.CountAsync(filter, cancellationToken);
            IEnumerable<Genre> genres = await _repository.GetAllAsync(filter, pageSize: request.PageSize, pageNo: request.PageNo,
                orderBy: q => q.OrderBy(g => g.GenreName).ThenBy(g => g.Id), tracked: false, cancellationToken: cancellationToken);

            return Result<PagedResponse<GenreResponse>>.Success(new PagedResponse<GenreResponse>
            {
                Items = _mapper.Map<List<GenreResponse>>(genres),
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<GenreResponse>> AddGenre(SaveGenreRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<GenreResponse>.From(validateResult.Errors);

            if (await _repository.NameExists(request.GenreName, 0, cancellationToken)) return Result<GenreResponse>.Failure(GenreServiceErrors.NameAlreadyExists);

            Result<Genre> createResult = Genre.Create(request.GenreName);
            if (createResult.IsFailure) return Result<GenreResponse>.Failure(createResult.Error);

            Genre genre = createResult.Value;

            await _repository.AddAsync(genre, cancellationToken);
            await _repository.CommitAsync(cancellationToken);

            return Result<GenreResponse>.Success(_mapper.Map<GenreResponse>(genre));
        }

        public async Task<Result<GenreResponse>> UpdateGenre(int id, SaveGenreRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<GenreResponse>.From(validateResult.Errors);

            Genre genre = await _repository.GetAsync(g => g.Id == id, tracked: true, cancellationToken: cancellationToken);
            if (genre is null) return Result<GenreResponse>.Failure(RepositoryErrors.EntityNotFound);

            if (await _repository.NameExists(request.GenreName, genre.Id, cancellationToken)) return Result<GenreResponse>.Failure(GenreServiceErrors.NameAlreadyExists);

            Result renameResult = genre.Rename(request.GenreName);
            if (renameResult.IsFailure) return Result<GenreResponse>.Failure(renameResult.Error);

            _repository.Update(genre);
            await _repository.CommitAsync(cancellationToken);

            return Result<GenreResponse>.Success(_mapper.Map<GenreResponse>(genre));
        }

        public async Task<Result> DeleteGenre(int id, CancellationToken cancellationToken = default)
        {
            Genre genre = await _repository.GetAsync(g => g.Id == id, tracked: true, cancellationToken: cancellationToken);
            if (genre is null) return Result.Failure(RepositoryErrors.EntityNotFound);

            bool isBeingReferenced = await _repository.IsUsing(genre.Id, cancellationToken);
            if (isBeingReferenced) return Result.Failure(RepositoryErrors.EntityStillBeingReferenced);

            _repository.Remove(genre);
            await _repository.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}

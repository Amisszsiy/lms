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

namespace Application.Features.Locations
{
    public class LocationService
    {
        private readonly ILocationRepository _repository;
        private readonly IMapper _mapper;
        private readonly LocationValidator _validator;
        private readonly SearchLocationsValidator _searchValidator;
        public LocationService(ILocationRepository repository, IMapper mapper, LocationValidator validator, SearchLocationsValidator searchValidator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
            _searchValidator = searchValidator;
        }

        public async Task<Result<LocationResponse>> GetLocation(int id, CancellationToken cancellationToken = default)
        {
            Location location = await _repository.GetAsync(l => l.Id == id, cancellationToken: cancellationToken);
            if (location is null) return Result<LocationResponse>.Failure(RepositoryErrors.EntityNotFound);

            return Result<LocationResponse>.Success(_mapper.Map<LocationResponse>(location));
        }

        public async Task<Result<PagedResponse<LocationResponse>>> SearchLocations(SearchLocationsRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _searchValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PagedResponse<LocationResponse>>.From(validateResult.Errors);

            string area = string.IsNullOrWhiteSpace(request.Area) ? null : request.Area.Trim();
            string lineNo = string.IsNullOrWhiteSpace(request.LineNo) ? null : request.LineNo.Trim();
            string shelfNo = string.IsNullOrWhiteSpace(request.ShelfNo) ? null : request.ShelfNo.Trim();

            Expression<Func<Location, bool>> filter = l =>
                (area == null || l.Area == area) &&
                (lineNo == null || l.LineNo == lineNo) &&
                (shelfNo == null || l.ShelfNo == shelfNo);

            int totalCount = await _repository.CountAsync(filter, cancellationToken);
            IEnumerable<Location> locations = await _repository.GetAllAsync(filter, pageSize: request.PageSize, pageNo: request.PageNo,
                orderBy: q => q.OrderBy(l => l.Area).ThenBy(l => l.LineNo).ThenBy(l => l.ShelfNo).ThenBy(l => l.RowNo).ThenBy(l => l.ColumnNo),
                tracked: false, cancellationToken: cancellationToken);

            return Result<PagedResponse<LocationResponse>>.Success(new PagedResponse<LocationResponse>
            {
                Items = _mapper.Map<List<LocationResponse>>(locations),
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<LocationResponse>> AddLocation(SaveLocationRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<LocationResponse>.From(validateResult.Errors);

            if (await PositionTaken(request, 0, cancellationToken)) return Result<LocationResponse>.Failure(LocationServiceErrors.PositionAlreadyExists);

            Result<Location> createResult = Location.Create(request.Area, request.LineNo, request.ShelfNo, request.RowNo, request.ColumnNo);
            if (createResult.IsFailure) return Result<LocationResponse>.Failure(createResult.Error);

            Location location = createResult.Value;

            await _repository.AddAsync(location, cancellationToken);
            await _repository.CommitAsync(cancellationToken);

            return Result<LocationResponse>.Success(_mapper.Map<LocationResponse>(location));
        }

        public async Task<Result<LocationResponse>> UpdateLocation(int id, SaveLocationRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<LocationResponse>.From(validateResult.Errors);

            Location location = await _repository.GetAsync(l => l.Id == id, tracked: true, cancellationToken: cancellationToken);
            if (location is null) return Result<LocationResponse>.Failure(RepositoryErrors.EntityNotFound);

            if (await PositionTaken(request, location.Id, cancellationToken)) return Result<LocationResponse>.Failure(LocationServiceErrors.PositionAlreadyExists);

            Result moveResult = location.MoveTo(request.Area, request.LineNo, request.ShelfNo, request.RowNo, request.ColumnNo);
            if (moveResult.IsFailure) return Result<LocationResponse>.Failure(moveResult.Error);

            _repository.Update(location);
            await _repository.CommitAsync(cancellationToken);

            return Result<LocationResponse>.Success(_mapper.Map<LocationResponse>(location));
        }

        public async Task<Result> DeleteLocation(int id, CancellationToken cancellationToken = default)
        {
            Location location = await _repository.GetAsync(l => l.Id == id, tracked: true, cancellationToken: cancellationToken);
            if (location is null) return Result.Failure(RepositoryErrors.EntityNotFound);

            bool isBeingReferenced = await _repository.IsUsing(location.Id, cancellationToken);
            if (isBeingReferenced) return Result.Failure(RepositoryErrors.EntityStillBeingReferenced);

            _repository.Remove(location);
            await _repository.CommitAsync(cancellationToken);

            return Result.Success();
        }

        private Task<bool> PositionTaken(SaveLocationRequest request, int locationId, CancellationToken cancellationToken)
        {
            return _repository.PositionExists(request.Area, request.LineNo, request.ShelfNo, request.RowNo, request.ColumnNo, locationId, cancellationToken);
        }
    }
}

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

namespace Application.Features.Publishers
{
    public class PublisherService
    {
        private readonly IPublisherRepository _repository;
        private readonly IMapper _mapper;
        private readonly PublisherValidator _validator;
        private readonly SearchPublishersValidator _searchValidator;
        public PublisherService(IPublisherRepository repository, IMapper mapper, PublisherValidator validator, SearchPublishersValidator searchValidator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
            _searchValidator = searchValidator;
        }

        public async Task<Result<PublisherResponse>> GetPublisher(int id, CancellationToken cancellationToken = default)
        {
            Publisher publisher = await _repository.GetAsync(p => p.Id == id, cancellationToken: cancellationToken);
            if (publisher is null) return Result<PublisherResponse>.Failure(RepositoryErrors.EntityNotFound);

            return Result<PublisherResponse>.Success(_mapper.Map<PublisherResponse>(publisher));
        }

        public async Task<Result<PagedResponse<PublisherResponse>>> SearchPublishers(SearchPublishersRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _searchValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PagedResponse<PublisherResponse>>.From(validateResult.Errors);

            string search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

            Expression<Func<Publisher, bool>> filter = p => search == null || p.Name.Contains(search);

            int totalCount = await _repository.CountAsync(filter, cancellationToken);
            IEnumerable<Publisher> publishers = await _repository.GetAllAsync(filter, pageSize: request.PageSize, pageNo: request.PageNo,
                orderBy: q => q.OrderBy(p => p.Name).ThenBy(p => p.Id), tracked: false, cancellationToken: cancellationToken);

            return Result<PagedResponse<PublisherResponse>>.Success(new PagedResponse<PublisherResponse>
            {
                Items = _mapper.Map<List<PublisherResponse>>(publishers),
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<PublisherResponse>> AddPublisher(SavePublisherRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PublisherResponse>.From(validateResult.Errors);

            Result<Publisher> createResult = Publisher.Create(request.Name, request.Address, request.Tel);
            if (createResult.IsFailure) return Result<PublisherResponse>.Failure(createResult.Error);

            Publisher publisher = createResult.Value;

            await _repository.AddAsync(publisher, cancellationToken);
            await _repository.CommitAsync(cancellationToken);

            return Result<PublisherResponse>.Success(_mapper.Map<PublisherResponse>(publisher));
        }

        public async Task<Result<PublisherResponse>> UpdatePublisher(int id, SavePublisherRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PublisherResponse>.From(validateResult.Errors);

            Publisher publisher = await _repository.GetAsync(id);
            if (publisher is null) return Result<PublisherResponse>.Failure(RepositoryErrors.EntityNotFound);

            publisher.Name = request.Name;
            publisher.Address = request.Address;
            publisher.Tel = request.Tel;

            _repository.Update(publisher);
            await _repository.CommitAsync(cancellationToken);

            return Result<PublisherResponse>.Success(_mapper.Map<PublisherResponse>(publisher));
        }

        public async Task<Result> DeletePublisher(int id, CancellationToken cancellationToken = default)
        {
            Publisher publisher = await _repository.GetAsync(id);
            if (publisher is null) return Result.Failure(RepositoryErrors.EntityNotFound);
            bool IsBeingReferenced = await _repository.IsUsing(publisher.Id);
            if (IsBeingReferenced) return Result.Failure(RepositoryErrors.EntityStillBeingReferenced);

            _repository.Remove(publisher);
            await _repository.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}

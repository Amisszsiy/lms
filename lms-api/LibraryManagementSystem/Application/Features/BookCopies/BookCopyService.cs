using Application.Common;
using Application.Errors;
using Application.Persistance;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.BookCopies
{
    public class BookCopyService
    {
        private readonly IBookCopyRepository _repository;
        private readonly IBookRepository _bookRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly CreateBookCopyValidator _createValidator;
        private readonly UpdateBookCopyValidator _updateValidator;
        public BookCopyService(IBookCopyRepository repository,
            IBookRepository bookRepository,
            ILocationRepository locationRepository,
            IMapper mapper,
            CreateBookCopyValidator createValidator,
            UpdateBookCopyValidator updateValidator)
        {
            _repository = repository;
            _bookRepository = bookRepository;
            _locationRepository = locationRepository;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<BookCopyResponse>> GetBookCopy(Guid id, CancellationToken cancellationToken = default)
        {
            BookCopy copy = await _repository.GetAsync(id);
            if (copy is null) return Result<BookCopyResponse>.Failure(RepositoryErrors.EntityNotFound);

            return Result<BookCopyResponse>.Success(_mapper.Map<BookCopyResponse>(copy));
        }

        public async Task<Result<List<BookCopyResponse>>> GetBookCopies(long bookId, bool includeWithdrawn = false, CancellationToken cancellationToken = default)
        {
            bool? isBookActive = await _bookRepository.IsActive(bookId, cancellationToken);
            if (isBookActive is null) return Result<List<BookCopyResponse>>.Failure(RepositoryErrors.EntityNotFound);

            var copies = await _repository.GetAllAsync(c => c.BookId == bookId && (includeWithdrawn || c.AvailableStatus != BookCopyStatus.Withdrawn),
                orderBy: q => q.OrderBy(c => c.CreatedDate).ThenBy(c => c.Id), tracked: false, cancellationToken: cancellationToken);

            return Result<List<BookCopyResponse>>.Success(_mapper.Map<List<BookCopyResponse>>(copies));
        }

        public async Task<Result<BookCopyResponse>> AddBookCopy(CreateBookCopyRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _createValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<BookCopyResponse>.From(validateResult.Errors);

            bool? isBookActive = await _bookRepository.IsActive(request.BookId, cancellationToken);
            if (isBookActive is null) return Result<BookCopyResponse>.Failure(BookCopyServiceErrors.BookNotFound);
            if (isBookActive is false) return Result<BookCopyResponse>.Failure(BookCopyServiceErrors.BookInactive);

            if (!await _locationRepository.Exists(request.LocationId, cancellationToken)) return Result<BookCopyResponse>.Failure(BookCopyServiceErrors.LocationNotFound);

            Result<BookCopy> createResult = BookCopy.Create(request.BookId, request.LocationId);
            if (createResult.IsFailure) return Result<BookCopyResponse>.Failure(createResult.Error);                                                                                                                                                                                                                                                   

            BookCopy copy = createResult.Value;

            await _repository.AddAsync(copy, cancellationToken);
            await _repository.CommitAsync(cancellationToken);

            return Result<BookCopyResponse>.Success(_mapper.Map<BookCopyResponse>(copy));
        }

        public async Task<Result<BookCopyResponse>> UpdateBookCopy(Guid id, UpdateBookCopyRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _updateValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<BookCopyResponse>.From(validateResult.Errors);

            BookCopy copy = await _repository.GetAsync(id);
            if (copy is null) return Result<BookCopyResponse>.Failure(RepositoryErrors.EntityNotFound);

            if (copy.LocationId != request.LocationId)
            {
                if (!await _locationRepository.Exists(request.LocationId, cancellationToken)) return Result<BookCopyResponse>.Failure(BookCopyServiceErrors.LocationNotFound);

                Result moveResult = copy.MoveTo(request.LocationId);
                if (moveResult.IsFailure) return Result<BookCopyResponse>.Failure(moveResult.Error);
            }

            if (copy.AvailableStatus != request.Status)
            {
                Result statusResult = ChangeStatus(copy, request.Status);
                if (statusResult.IsFailure) return Result<BookCopyResponse>.Failure(statusResult.Error);
            }

            _repository.Update(copy);
            await _repository.CommitAsync(cancellationToken);

            return Result<BookCopyResponse>.Success(_mapper.Map<BookCopyResponse>(copy));
        }

        // Soft delete: copy is set to withdrawn for remaining borrowing history.
        public async Task<Result> DeleteBookCopy(Guid id, CancellationToken cancellationToken = default)
        {
            BookCopy copy = await _repository.GetAsync(id);
            if (copy is null) return Result.Failure(RepositoryErrors.EntityNotFound);

            Result withdrawResult = ChangeStatus(copy, BookCopyStatus.Withdrawn);
            if (withdrawResult.IsFailure) return withdrawResult;

            _repository.Update(copy);
            await _repository.CommitAsync(cancellationToken);

            return Result.Success();
        }

        private static Result ChangeStatus(BookCopy copy, BookCopyStatus status)
        {
            switch (status)
            {
                case BookCopyStatus.Available:
                    return copy.Restore();
                case BookCopyStatus.Damaged:
                    return copy.MarkDamaged();
                case BookCopyStatus.Lost:
                    return copy.MarkLost();
                case BookCopyStatus.Withdrawn:
                    if (copy.IsOnLoan) return Result.Failure(BookCopyErrors.InvalidStatusTransition);
                    copy.Withdraw();
                    return Result.Success();
                default:
                    return Result.Failure(BookCopyErrors.InvalidStatusTransition);
            }
        }
    }
}

import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BookService } from '../services/book.service';
import { BookCopyService } from '../services/book-copy.service';
import { BorrowingService } from '../services/borrowing.service';
import { LookupService } from '../services/lookup.service';
import { getErrorMessage } from '../services/error-message';
import { BookResponse, SearchBooksRequest } from '../models/book';
import { BookCopyResponse, BookCopyStatus } from '../models/bookCopy';
import { AuthorResponse, GenreResponse, PublisherResponse } from '../models/lookups';

@Component({
  selector: 'app-book-search',
  imports: [
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './book-search.html',
  styleUrl: './book-search.css',
})
export class BookSearch implements OnInit {
  private readonly bookService = inject(BookService);
  private readonly bookCopyService = inject(BookCopyService);
  private readonly borrowingService = inject(BorrowingService);
  private readonly lookupService = inject(LookupService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly displayedColumns: string[] = [
    'isbn', 'title', 'publisher', 'authors', 'genres', 'publicationYear', 'availableCopyCount', 'actions'
  ];

  protected readonly books = signal<BookResponse[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly errorMessage = signal('');
  // Id of the book being borrowed, so its button can't be clicked twice.
  protected readonly borrowingBookId = signal<number | null>(null);

  protected search = '';
  protected pageIndex = 0;
  protected pageSize = 10;

  private publishers: PublisherResponse[] = [];
  private authors: AuthorResponse[] = [];
  private genres: GenreResponse[] = [];

  ngOnInit(): void {
    forkJoin({
      publishers: this.lookupService.getPublishers(),
      authors: this.lookupService.getAuthors(),
      genres: this.lookupService.getGenres()
    }).subscribe({
      next: (result) => {
        this.publishers = result.publishers;
        this.authors = result.authors;
        this.genres = result.genres;
        this.loadBooks();
      },
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  loadBooks(): void {
    // Normal users only see active books; the API ignores includeInactive for them anyway.
    const request: SearchBooksRequest = {
      search: this.search,
      includeInactive: false,
      pageNo: this.pageIndex + 1,
      pageSize: this.pageSize
    };

    this.errorMessage.set('');
    this.bookService.searchBooks(request).subscribe({
      next: (result) => {
        this.books.set(result.items);
        this.totalCount.set(result.totalCount);
      },
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  onSearch(): void {
    this.pageIndex = 0;
    this.loadBooks();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadBooks();
  }

  canBorrow(book: BookResponse): boolean {
    if (this.borrowingBookId() !== null) {
      return false;
    }
    return book.availableCopyCount > 0;
  }

  // A borrowing is made for one copy, so find the first available copy of the book and borrow it.
  borrow(book: BookResponse): void {
    this.borrowingBookId.set(book.id);

    this.bookCopyService.getBookCopies(book.id, false).subscribe({
      next: (copies) => {
        const copy = this.findAvailableCopy(copies);
        if (copy === null) {
          this.borrowingBookId.set(null);
          this.snackBar.open('No copy available', 'Close', { duration: 5000 });
          this.loadBooks();
          return;
        }
        this.borrowCopy(copy);
      },
      error: (err: HttpErrorResponse) => {
        this.borrowingBookId.set(null);
        this.snackBar.open(getErrorMessage(err), 'Close', { duration: 5000 });
      }
    });
  }

  getPublisherName(publisherId: number): string {
    for (const publisher of this.publishers) {
      if (publisher.id === publisherId) {
        return publisher.name;
      }
    }
    return '';
  }

  getAuthorNames(authorIds: number[]): string {
    const names: string[] = [];
    for (const author of this.authors) {
      if (authorIds.includes(author.id)) {
        names.push(`${author.firstName} ${author.lastName}`);
      }
    }
    return names.join(', ');
  }

  getGenreNames(genreIds: number[]): string {
    const names: string[] = [];
    for (const genre of this.genres) {
      if (genreIds.includes(genre.id)) {
        names.push(genre.genreName);
      }
    }
    return names.join(', ');
  }

  private borrowCopy(copy: BookCopyResponse): void {
    this.borrowingService.borrow({ bookCopyId: copy.id }).subscribe({
      next: () => {
        this.borrowingBookId.set(null);
        this.snackBar.open('Book borrowed', 'Close', { duration: 3000 });
        this.loadBooks();
      },
      error: (err: HttpErrorResponse) => {
        this.borrowingBookId.set(null);
        this.snackBar.open(getErrorMessage(err), 'Close', { duration: 5000 });
        this.loadBooks();
      }
    });
  }

  private findAvailableCopy(copies: BookCopyResponse[]): BookCopyResponse | null {
    for (const copy of copies) {
      if (copy.availableStatus === BookCopyStatus.Available) {
        return copy;
      }
    }
    return null;
  }
}

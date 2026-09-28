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
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BookService } from '../services/book.service';
import { LookupService } from '../services/lookup.service';
import { getErrorMessage } from '../services/error-message';
import { BookResponse, SearchBooksRequest } from '../models/book';
import { AuthorResponse, GenreResponse, PublisherResponse } from '../models/lookups';
import { BookFormDialog, BookFormDialogData } from './book-form-dialog/book-form-dialog';

@Component({
  selector: 'app-book-management',
  imports: [
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule
  ],
  templateUrl: './book-management.html',
  styleUrl: './book-management.css',
})
export class BookManagement implements OnInit {
  private readonly bookService = inject(BookService);
  private readonly lookupService = inject(LookupService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly displayedColumns: string[] = [
    'isbn', 'title', 'publisher', 'authors', 'genres', 'publicationYear', 'availableCopyCount', 'status', 'actions'
  ];

  protected readonly books = signal<BookResponse[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly errorMessage = signal('');

  // Search and paging state. pageIndex starts at 0 (paginator), API pageNo starts at 1.
  protected search = '';
  protected includeInactive = true;
  protected pageIndex = 0;
  protected pageSize = 10;

  // Used to show names instead of ids, and passed to the form dialog.
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
    const request: SearchBooksRequest = {
      search: this.search,
      includeInactive: this.includeInactive,
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

  openAddDialog(): void {
    this.openFormDialog(null);
  }

  openEditDialog(book: BookResponse): void {
    this.openFormDialog(book);
  }

  deactivate(book: BookResponse): void {
    const confirmed = confirm(`Deactivate "${book.title}"? All of its copies will be withdrawn.`);
    if (!confirmed) {
      return;
    }

    this.bookService.deactivateBook(book.id).subscribe({
      next: () => {
        this.snackBar.open('Book deactivated', 'Close', { duration: 3000 });
        this.loadBooks();
      },
      error: (err: HttpErrorResponse) => {
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

  private openFormDialog(book: BookResponse | null): void {
    const data: BookFormDialogData = {
      book: book,
      publishers: this.publishers,
      authors: this.authors,
      genres: this.genres
    };

    const dialogRef = this.dialog.open(BookFormDialog, { data: data, width: '600px' });

    dialogRef.afterClosed().subscribe((saved: boolean) => {
      if (saved) {
        this.snackBar.open('Book saved', 'Close', { duration: 3000 });
        this.loadBooks();
      }
    });
  }
}

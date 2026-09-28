import { AfterViewInit, Component, DestroyRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { debounceTime } from 'rxjs';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BookService } from '../services/book.service';
import { BookCopyService } from '../services/book-copy.service';
import { LookupService } from '../services/lookup.service';
import { getErrorMessage } from '../services/error-message';
import { BookResponse } from '../models/book';
import { BookCopyResponse, BookCopyStatus, getBookCopyStatusLabel, isBookCopyOnLoan } from '../models/bookCopy';
import { getLocationLabel, LocationResponse } from '../models/lookups';
import { BookCopyFormDialog, BookCopyFormDialogData } from './book-copy-form-dialog/book-copy-form-dialog';

@Component({
  selector: 'app-book-inventory',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatAutocompleteModule
  ],
  templateUrl: './book-inventory.html',
  styleUrl: './book-inventory.css',
})
export class BookInventory implements OnInit, AfterViewInit {
  private readonly bookService = inject(BookService);
  private readonly bookCopyService = inject(BookCopyService);
  private readonly lookupService = inject(LookupService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  protected readonly displayedColumns: string[] = ['id', 'location', 'status', 'actions'];

  // Book picker. Holds the typed text, or the BookResponse once an option is chosen.
  protected readonly bookSearchControl = new FormControl<string | BookResponse>('', { nonNullable: true });
  protected readonly bookOptions = signal<BookResponse[]>([]);
  protected readonly selectedBook = signal<BookResponse | null>(null);

  // The API returns every copy of the book at once, so search and paging are done here in the client.
  protected readonly dataSource = new MatTableDataSource<BookCopyResponse>([]);
  protected search = '';
  protected includeWithdrawn = false;
  protected readonly errorMessage = signal('');

  private locations: LocationResponse[] = [];
  private locationsById = new Map<number, LocationResponse>();

  ngOnInit(): void {
    this.lookupService.getLocations().subscribe({
      next: (locations) => {
        this.locations = locations;
        for (const location of locations) {
          this.locationsById.set(location.id, location);
        }
      },
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });

    // Search books while the user types in the picker.
    this.bookSearchControl.valueChanges
      .pipe(debounceTime(300), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') {
          this.searchBookOptions(value);
        }
      });
    this.searchBookOptions('');

    // Search box matches copy id, location or status.
    this.dataSource.filterPredicate = (copy: BookCopyResponse, filter: string) => {
      const text = `${copy.id} ${this.getLocationName(copy.locationId)} ${this.getStatusLabel(copy.availableStatus)}`;
      return text.toLowerCase().includes(filter);
    };
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  searchBookOptions(text: string): void {
    const request = { search: text, includeInactive: true, pageNo: 1, pageSize: 20 };
    this.bookService.searchBooks(request).subscribe({
      next: (result) => this.bookOptions.set(result.items),
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  displayBook(book: BookResponse | string | null): string {
    if (book === null || typeof book === 'string') {
      return book ?? '';
    }
    return `${book.title} (${book.isbn})`;
  }

  onBookSelected(event: MatAutocompleteSelectedEvent): void {
    const book: BookResponse = event.option.value;
    this.selectedBook.set(book);
    this.loadCopies();
  }

  loadCopies(): void {
    const book = this.selectedBook();
    if (book === null) {
      return;
    }

    this.errorMessage.set('');
    this.bookCopyService.getBookCopies(book.id, this.includeWithdrawn).subscribe({
      next: (copies) => {
        this.dataSource.data = copies;
      },
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  onSearch(): void {
    this.dataSource.filter = this.search.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  openAddDialog(): void {
    this.openFormDialog(null);
  }

  openEditDialog(copy: BookCopyResponse): void {
    this.openFormDialog(copy);
  }

  // Copy on loan must be returned first. Withdrawn copy is already withdrawn.
  canWithdraw(copy: BookCopyResponse): boolean {
    if (isBookCopyOnLoan(copy.availableStatus)) {
      return false;
    }
    if (copy.availableStatus === BookCopyStatus.Withdrawn) {
      return false;
    }
    return true;
  }

  withdraw(copy: BookCopyResponse): void {
    const confirmed = confirm('Withdraw this copy?');
    if (!confirmed) {
      return;
    }

    this.bookCopyService.withdrawBookCopy(copy.id).subscribe({
      next: () => {
        this.snackBar.open('Copy withdrawn', 'Close', { duration: 3000 });
        this.loadCopies();
      },
      error: (err: HttpErrorResponse) => {
        this.snackBar.open(getErrorMessage(err), 'Close', { duration: 5000 });
      }
    });
  }

  getLocationName(locationId: number): string {
    const location = this.locationsById.get(locationId);
    if (location === undefined) {
      return '';
    }
    return getLocationLabel(location);
  }

  getStatusLabel(status: BookCopyStatus): string {
    return getBookCopyStatusLabel(status);
  }

  private openFormDialog(copy: BookCopyResponse | null): void {
    const data: BookCopyFormDialogData = {
      copy: copy,
      book: this.selectedBook(),
      locations: this.locations
    };

    const dialogRef = this.dialog.open(BookCopyFormDialog, { data: data, width: '600px' });

    dialogRef.afterClosed().subscribe((saved: boolean) => {
      if (saved) {
        this.snackBar.open('Copy saved', 'Close', { duration: 3000 });
        this.loadCopies();
      }
    });
  }
}

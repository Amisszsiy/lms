import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { debounceTime } from 'rxjs';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BookService } from '../../services/book.service';
import { BookCopyService } from '../../services/book-copy.service';
import { getErrorMessage } from '../../services/error-message';
import { BookResponse } from '../../models/book';
import { BookCopyResponse, BookCopyStatus, CreateBookCopyRequest, getBookCopyStatusLabel, UpdateBookCopyRequest } from '../../models/bookCopy';
import { getLocationLabel, LocationResponse } from '../../models/lookups';

export interface BookCopyFormDialogData {
  // null when adding a new copy.
  copy: BookCopyResponse | null;
  // Book selected on the inventory screen. Default book for a new copy.
  book: BookResponse | null;
  locations: LocationResponse[];
}

// Autocomplete holds the typed text until an option is picked, then the picked object.
function optionSelectedValidator(control: AbstractControl): ValidationErrors | null {
  if (control.value !== null && typeof control.value === 'object') {
    return null;
  }
  return { notSelected: true };
}

@Component({
  selector: 'app-book-copy-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './book-copy-form-dialog.html',
  styleUrl: './book-copy-form-dialog.css',
})
export class BookCopyFormDialog implements OnInit {
  private readonly bookService = inject(BookService);
  private readonly bookCopyService = inject(BookCopyService);
  private readonly dialogRef = inject(MatDialogRef<BookCopyFormDialog>);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly data: BookCopyFormDialogData = inject(MAT_DIALOG_DATA);

  protected readonly isEdit = this.data.copy !== null;
  protected readonly errorMessage = signal('');
  protected readonly saving = signal(false);

  protected readonly bookOptions = signal<BookResponse[]>([]);
  protected readonly locationOptions = signal<LocationResponse[]>([]);
  protected readonly statusOptions: BookCopyStatus[] = [];

  protected readonly form = new FormGroup({
    book: new FormControl<BookResponse | string | null>(null, [optionSelectedValidator]),
    location: new FormControl<LocationResponse | string | null>(null, [optionSelectedValidator]),
    status: new FormControl<BookCopyStatus>(BookCopyStatus.Available, { nonNullable: true, validators: [Validators.required] })
  });

  ngOnInit(): void {
    this.form.controls.book.setValue(this.data.book);

    const copy = this.data.copy;
    if (copy !== null) {
      // BookId can't be changed after the copy is created.
      this.form.controls.book.disable();
      this.form.controls.location.setValue(this.findLocation(copy.locationId));
      this.form.controls.status.setValue(copy.availableStatus);
      this.statusOptions.push(...this.getAllowedStatuses(copy.availableStatus));
    }

    this.form.controls.book.valueChanges
      .pipe(debounceTime(300), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') {
          this.searchBookOptions(value);
        }
      });

    this.form.controls.location.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') {
          this.filterLocationOptions(value);
        }
      });

    this.searchBookOptions('');
    this.filterLocationOptions('');
  }

  searchBookOptions(text: string): void {
    // Only active books can get new copies.
    const request = { search: text, includeInactive: false, pageNo: 1, pageSize: 20 };
    this.bookService.searchBooks(request).subscribe({
      next: (result) => this.bookOptions.set(result.items),
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  // Type "A-1-2" to see every slot on Area A, Line 1, Shelf 2.
  filterLocationOptions(text: string): void {
    const filter = text.trim().toLowerCase();
    const matches: LocationResponse[] = [];
    for (const location of this.data.locations) {
      if (getLocationLabel(location).toLowerCase().includes(filter)) {
        matches.push(location);
      }
      // Keep the dropdown short.
      if (matches.length >= 50) {
        break;
      }
    }
    this.locationOptions.set(matches);
  }

  displayBook(book: BookResponse | string | null): string {
    if (book === null || typeof book === 'string') {
      return book ?? '';
    }
    return `${book.title} (${book.isbn})`;
  }

  displayLocation(location: LocationResponse | string | null): string {
    if (location === null || typeof location === 'string') {
      return location ?? '';
    }
    return getLocationLabel(location);
  }

  getStatusLabel(status: BookCopyStatus): string {
    return getBookCopyStatusLabel(status);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const location = this.form.controls.location.value as LocationResponse;

    this.errorMessage.set('');
    this.saving.set(true);

    const copy = this.data.copy;
    let save$;
    if (copy === null) {
      const book = this.form.controls.book.value as BookResponse;
      const request: CreateBookCopyRequest = {
        bookId: book.id,
        locationId: location.id
      };
      save$ = this.bookCopyService.addBookCopy(request);
    } else {
      const request: UpdateBookCopyRequest = {
        locationId: location.id,
        status: this.form.controls.status.value
      };
      save$ = this.bookCopyService.updateBookCopy(copy.id, request);
    }

    save$.subscribe({
      next: () => this.dialogRef.close(true),
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(getErrorMessage(err));
      }
    });
  }

  private findLocation(locationId: number): LocationResponse | null {
    for (const location of this.data.locations) {
      if (location.id === locationId) {
        return location;
      }
    }
    return null;
  }

  // Same transitions as BookCopyService.ChangeStatus in the API.
  // Borrowed / Return Requested are changed by the borrowing flow only.
  private getAllowedStatuses(current: BookCopyStatus): BookCopyStatus[] {
    switch (current) {
      case BookCopyStatus.Available:
        return [BookCopyStatus.Available, BookCopyStatus.Damaged, BookCopyStatus.Lost, BookCopyStatus.Withdrawn];
      case BookCopyStatus.Damaged:
        return [BookCopyStatus.Damaged, BookCopyStatus.Available, BookCopyStatus.Withdrawn];
      case BookCopyStatus.Lost:
        return [BookCopyStatus.Lost, BookCopyStatus.Available, BookCopyStatus.Withdrawn];
      default:
        return [current];
    }
  }
}

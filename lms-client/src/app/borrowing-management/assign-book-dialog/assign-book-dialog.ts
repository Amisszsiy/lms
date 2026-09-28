import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { debounceTime } from 'rxjs';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { UserService } from '../../services/user.service';
import { BookService } from '../../services/book.service';
import { BookCopyService } from '../../services/book-copy.service';
import { BorrowingService } from '../../services/borrowing.service';
import { getErrorMessage } from '../../services/error-message';
import { UserResponse } from '../../models/user';
import { BookResponse } from '../../models/book';
import { BookCopyResponse, BookCopyStatus } from '../../models/bookCopy';
import { AssignBorrowRequest } from '../../models/borrowing';

// Autocomplete holds the typed text until an option is picked, then the picked object.
function optionSelectedValidator(control: AbstractControl): ValidationErrors | null {
  if (control.value !== null && typeof control.value === 'object') {
    return null;
  }
  return { notSelected: true };
}

@Component({
  selector: 'app-assign-book-dialog',
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
  templateUrl: './assign-book-dialog.html',
  styleUrl: './assign-book-dialog.css',
})
export class AssignBookDialog implements OnInit {
  private readonly userService = inject(UserService);
  private readonly bookService = inject(BookService);
  private readonly bookCopyService = inject(BookCopyService);
  private readonly borrowingService = inject(BorrowingService);
  private readonly dialogRef = inject(MatDialogRef<AssignBookDialog>);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly errorMessage = signal('');
  protected readonly saving = signal(false);

  protected readonly userOptions = signal<UserResponse[]>([]);
  protected readonly bookOptions = signal<BookResponse[]>([]);
  // Available copies of the chosen book.
  protected readonly copyOptions = signal<BookCopyResponse[]>([]);

  // AssignBorrowRequest needs userId and bookCopyId. Book is only used to find its copies.
  protected readonly form = new FormGroup({
    user: new FormControl<UserResponse | string | null>(null, [optionSelectedValidator]),
    book: new FormControl<BookResponse | string | null>(null, [optionSelectedValidator]),
    bookCopyId: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] })
  });

  ngOnInit(): void {
    this.form.controls.user.valueChanges
      .pipe(debounceTime(300), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') {
          this.searchUserOptions(value);
        }
      });

    this.form.controls.book.valueChanges
      .pipe(debounceTime(300), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') {
          this.searchBookOptions(value);
        }
      });

    this.searchUserOptions('');
    this.searchBookOptions('');
  }

  searchUserOptions(text: string): void {
    // Inactive users can't borrow.
    this.userService.searchUsers(text, true, 1, 20).subscribe({
      next: (result) => this.userOptions.set(result.items),
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  searchBookOptions(text: string): void {
    const request = { search: text, includeInactive: false, pageNo: 1, pageSize: 20 };
    this.bookService.searchBooks(request).subscribe({
      next: (result) => this.bookOptions.set(result.items),
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  onBookSelected(event: MatAutocompleteSelectedEvent): void {
    const book: BookResponse = event.option.value;
    this.form.controls.bookCopyId.setValue('');
    this.copyOptions.set([]);

    this.bookCopyService.getBookCopies(book.id, false).subscribe({
      next: (copies) => {
        const availableCopies: BookCopyResponse[] = [];
        for (const copy of copies) {
          if (copy.availableStatus === BookCopyStatus.Available) {
            availableCopies.push(copy);
          }
        }
        this.copyOptions.set(availableCopies);

        // Most of the time any copy will do, so pick the first one.
        if (availableCopies.length > 0) {
          this.form.controls.bookCopyId.setValue(availableCopies[0].id);
        }
      },
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  displayUser(user: UserResponse | string | null): string {
    if (user === null || typeof user === 'string') {
      return user ?? '';
    }
    return `${user.firstName} ${user.lastName} (${user.email})`;
  }

  displayBook(book: BookResponse | string | null): string {
    if (book === null || typeof book === 'string') {
      return book ?? '';
    }
    return `${book.title} (${book.isbn})`;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const user = this.form.controls.user.value as UserResponse;
    const request: AssignBorrowRequest = {
      userId: user.id,
      bookCopyId: this.form.controls.bookCopyId.value
    };

    this.errorMessage.set('');
    this.saving.set(true);

    this.borrowingService.assign(request).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(getErrorMessage(err));
      }
    });
  }
}

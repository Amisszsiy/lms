import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BookService } from '../../services/book.service';
import { getErrorMessage } from '../../services/error-message';
import { BookResponse, SaveBookRequest } from '../../models/book';
import { AuthorResponse, GenreResponse, PublisherResponse } from '../../models/lookups';

export interface BookFormDialogData {
  // null when adding a new book.
  book: BookResponse | null;
  publishers: PublisherResponse[];
  authors: AuthorResponse[];
  genres: GenreResponse[];
}

@Component({
  selector: 'app-book-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './book-form-dialog.html',
  styleUrl: './book-form-dialog.css',
})
export class BookFormDialog {
  private readonly fb = inject(FormBuilder);
  private readonly bookService = inject(BookService);
  private readonly dialogRef = inject(MatDialogRef<BookFormDialog>);
  protected readonly data: BookFormDialogData = inject(MAT_DIALOG_DATA);

  protected readonly isEdit = this.data.book !== null;
  protected readonly currentYear = new Date().getFullYear();
  protected readonly errorMessage = signal('');
  protected readonly saving = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    isbn: ['', [Validators.required]],
    title: ['', [Validators.required, Validators.maxLength(200)]],
    publisherId: [0, [Validators.required, Validators.min(1)]],
    publicationYear: [this.currentYear, [Validators.required, Validators.min(1), Validators.max(this.currentYear)]],
    authorIds: [[] as number[], [Validators.required]],
    genreIds: [[] as number[], [Validators.required]]
  });

  constructor() {
    const book = this.data.book;
    if (book !== null) {
      this.form.setValue({
        isbn: book.isbn,
        title: book.title,
        publisherId: book.publisherId,
        publicationYear: book.publicationYear,
        authorIds: book.authorIds,
        genreIds: book.genreIds
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: SaveBookRequest = {
      isbn: value.isbn,
      title: value.title,
      publisherId: value.publisherId,
      publicationYear: value.publicationYear,
      authorIds: value.authorIds,
      genreIds: value.genreIds
    };

    this.errorMessage.set('');
    this.saving.set(true);

    const book = this.data.book;
    let save$;
    if (book === null) {
      save$ = this.bookService.addBook(request);
    } else {
      save$ = this.bookService.updateBook(book.id, request);
    }

    save$.subscribe({
      next: () => this.dialogRef.close(true),
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(getErrorMessage(err));
      }
    });
  }
}

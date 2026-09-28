import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BorrowingService } from '../services/borrowing.service';
import { getErrorMessage } from '../services/error-message';
import { BorrowingResponse, BorrowingStatus, getBorrowingStatusLabel, SearchBorrowingsRequest } from '../models/borrowing';
import { AssignBookDialog } from './assign-book-dialog/assign-book-dialog';

@Component({
  selector: 'app-borrowing-management',
  imports: [
    FormsModule,
    DatePipe,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './borrowing-management.html',
  styleUrl: './borrowing-management.css',
})
export class BorrowingManagement implements OnInit {
  private readonly borrowingService = inject(BorrowingService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly displayedColumns: string[] = [
    'book', 'borrower', 'borrowDate', 'dueDate', 'returnDate', 'status', 'actions'
  ];

  protected readonly statusOptions: BorrowingStatus[] = [
    BorrowingStatus.Borrowed,
    BorrowingStatus.Overdue,
    BorrowingStatus.ReturnRequested,
    BorrowingStatus.Returned
  ];

  protected readonly borrowings = signal<BorrowingResponse[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly errorMessage = signal('');

  protected search = '';
  // null = all statuses.
  protected status: BorrowingStatus | null = null;
  protected pageIndex = 0;
  protected pageSize = 10;

  ngOnInit(): void {
    this.loadBorrowings();
  }

  loadBorrowings(): void {
    const request: SearchBorrowingsRequest = {
      search: this.search,
      status: this.status,
      pageNo: this.pageIndex + 1,
      pageSize: this.pageSize
    };

    this.errorMessage.set('');
    this.borrowingService.searchBorrowings(request).subscribe({
      next: (result) => {
        this.borrowings.set(result.items);
        this.totalCount.set(result.totalCount);
      },
      error: (err: HttpErrorResponse) => this.errorMessage.set(getErrorMessage(err))
    });
  }

  onSearch(): void {
    this.pageIndex = 0;
    this.loadBorrowings();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadBorrowings();
  }

  openAssignDialog(): void {
    const dialogRef = this.dialog.open(AssignBookDialog, { width: '600px' });

    dialogRef.afterClosed().subscribe((saved: boolean) => {
      if (saved) {
        this.snackBar.open('Book assigned', 'Close', { duration: 3000 });
        this.loadBorrowings();
      }
    });
  }

  // Any open loan can be returned, with or without a return request (walk-in return).
  canProcessReturn(borrowing: BorrowingResponse): boolean {
    return borrowing.status !== BorrowingStatus.Returned;
  }

  processReturn(borrowing: BorrowingResponse): void {
    const confirmed = confirm(`Process return of "${borrowing.book.title}"?`);
    if (!confirmed) {
      return;
    }

    this.borrowingService.processReturn(borrowing.id).subscribe({
      next: () => {
        this.snackBar.open('Return processed', 'Close', { duration: 3000 });
        this.loadBorrowings();
      },
      error: (err: HttpErrorResponse) => {
        this.snackBar.open(getErrorMessage(err), 'Close', { duration: 5000 });
      }
    });
  }

  getStatusLabel(status: BorrowingStatus): string {
    return getBorrowingStatusLabel(status);
  }
}

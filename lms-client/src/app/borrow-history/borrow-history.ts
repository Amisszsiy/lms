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
import { MatSnackBar } from '@angular/material/snack-bar';
import { BorrowingService } from '../services/borrowing.service';
import { getErrorMessage } from '../services/error-message';
import { BorrowingResponse, BorrowingStatus, getBorrowingStatusLabel, SearchBorrowingsRequest } from '../models/borrowing';

@Component({
  selector: 'app-borrow-history',
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
  templateUrl: './borrow-history.html',
  styleUrl: './borrow-history.css',
})
export class BorrowHistory implements OnInit {
  private readonly borrowingService = inject(BorrowingService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly displayedColumns: string[] = [
    'book', 'borrowDate', 'dueDate', 'returnDate', 'status', 'actions'
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
    this.borrowingService.searchMyBorrowings(request).subscribe({
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

  // Only an open loan that hasn't been requested yet can be requested (Borrowed or Overdue).
  canRequestReturn(borrowing: BorrowingResponse): boolean {
    if (borrowing.status === BorrowingStatus.Borrowed) {
      return true;
    }
    if (borrowing.status === BorrowingStatus.Overdue) {
      return true;
    }
    return false;
  }

  getRequestReturnButtonText(borrowing: BorrowingResponse): string {
    if (borrowing.status === BorrowingStatus.Returned) {
      return 'Returned';
    }
    if (borrowing.status === BorrowingStatus.ReturnRequested) {
      return 'Return Requested';
    }
    return 'Request Return';
  }

  requestReturn(borrowing: BorrowingResponse): void {
    this.borrowingService.requestReturn(borrowing.id).subscribe({
      next: () => {
        this.snackBar.open('Return requested', 'Close', { duration: 3000 });
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

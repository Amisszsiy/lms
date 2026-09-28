import { Routes } from '@angular/router';
import { Register } from './register/register';
import { Login } from './login/login';
import { Dashboard } from './dashboard/dashboard';
import { Layout } from './layouts/layout/layout';
import { authGuard } from './guards/auth-guard';
import { noAuthGuard } from './guards/no-auth-guard';
import { adminGuard } from './guards/admin-guard';
import { BookManagement } from './book-management/book-management';
import { BookInventory } from './book-inventory/book-inventory';
import { BorrowingManagement } from './borrowing-management/borrowing-management';
import { BookSearch } from './book-search/book-search';
import { BorrowHistory } from './borrow-history/borrow-history';

export const routes: Routes = [
    {
        path: 'register',
        canActivate: [noAuthGuard],
        component: Register
    },
    {
        path: 'login',
        canActivate: [noAuthGuard],
        component: Login
    },
    {
        path: '',
        canActivate: [authGuard],
        component: Layout,
        children: [
            {
                path: '',
                pathMatch: 'full',
                redirectTo: 'dashboard'
            },
            {
                path: 'dashboard',
                component: Dashboard
            },
            // Admin menu
            {
                path: 'admin/books',
                canActivate: [adminGuard],
                component: BookManagement
            },
            {
                path: 'admin/inventory',
                canActivate: [adminGuard],
                component: BookInventory
            },
            {
                path: 'admin/borrowings',
                canActivate: [adminGuard],
                component: BorrowingManagement
            },
            // User menu
            {
                path: 'books',
                component: BookSearch
            },
            {
                path: 'my-borrowings',
                component: BorrowHistory
            }
        ]
    },
    {
        path: '**',
        redirectTo: 'dashboard'
    }
];

import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { RegisterRequest } from '../models/registerRequest';
import { HttpErrorResponse } from '@angular/common/http';
import { getErrorMessage } from '../services/error-message';

@Component({
  selector: 'app-register',
  imports: [
    MatCardModule, 
    MatButtonModule, 
    MatFormFieldModule, 
    MatInputModule, 
    MatIconModule, 
    ReactiveFormsModule
  ],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly form = this.fb.nonNullable.group(
    {
      // Same rules as RegisterValidator in the API.
      username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100), Validators.pattern(/^[a-zA-Z0-9._-]+$/)]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(100), Validators.pattern(/^(?=.*[a-zA-Z])(?=.*[0-9]).*$/)]],
      confirmPassword: ['', [Validators.required]],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
      tel: ['', [Validators.maxLength(15), Validators.pattern(/^\+?[0-9\- ]+$/)]]
    },
    { validators: this.passwordsMatchValidator() }
);

  protected readonly errorMessage = signal('');
  protected readonly hidePassword = signal(true);

  onSubmit() : void{
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');

    const { username, password, firstName, lastName, email, tel } = this.form.getRawValue();

    const registerRequest: RegisterRequest = { username, password, firstName, lastName, email, tel };

    this.authService.register(registerRequest).subscribe({
      next: () => this.router.navigate(['/login']),
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(getErrorMessage(err));
      },
    });
  }

  passwordsMatchValidator(): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const password = group.get('password');
      const confirmPassword = group.get('confirmPassword');

      if (password?.value === confirmPassword?.value) {
        if (confirmPassword?.hasError('passwordMismatch')) {
          const { passwordMismatch, ...rest } = confirmPassword.errors ?? {};
          confirmPassword.setErrors(Object.keys(rest).length ? rest : null);
        }
        return null;
      }

      confirmPassword?.setErrors({ ...confirmPassword.errors, passwordMismatch: true });
      return { passwordMismatch: true };
    };
  }
}

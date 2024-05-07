import { CommonModule } from '@angular/common';
import { Component, ElementRef, Input } from '@angular/core';
import {
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  FormGroup,
  Validators,
  ValidatorFn,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MESSAGE_CONSTANTS } from '../../../../constants/message.constants';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

export function passwordStrengthValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (!value) {
      return null;
    }

    const hasUpperCase = /[A-Z]+/.test(value);

    const hasLowerCase = /[a-z]+/.test(value);

    const hasNumeric = /[0-9]+/.test(value);

    const passwordValid = (hasUpperCase || hasLowerCase) && hasNumeric;

    return !passwordValid ? { weakPassword: true } : null;
  };
}

export function passwordMatchValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.parent?.get('password');
    const confirmPassword = control.parent?.get('confirmPassword');

    return password &&
      confirmPassword &&
      password.value !== confirmPassword.value
      ? { passwordMismatch: true }
      : null;
  };
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    RouterLink,
    MatProgressSpinnerModule,
  ],
})
export class ResetPasswordComponent {
  @Input() token: string = '';

  MESSAGE_CONSTANTS = MESSAGE_CONSTANTS;

  isLoading = false;

  resetPasswordForm = new FormGroup({
    password: new FormControl(null, [
      Validators.required,
      Validators.minLength(7),
      passwordStrengthValidator(),
    ]),
    confirmPassword: new FormControl(null, [
      Validators.required,
      passwordMatchValidator(),
    ]),
  });

  constructor(
    private authService: AuthService,
    private messageBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.IsLoggedIn()) {
      console.log(this.authService.IsLoggedIn());
      this.router.navigate(['/home']);
    }
  }

  onSubmit() {
    if (!this.resetPasswordForm.valid) return;
    if(this.resetPasswordForm.value.password!=this.resetPasswordForm.value.confirmPassword){
      this.resetPasswordForm.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return;
    }
    this.isLoading = true;
    this.authService
      .resetPassword(this.resetPasswordForm.value.password!, this.token)
      .subscribe({
        next: (response) => {
          if (response == 200) {
            this.messageBar.open('Password reset successfully', '', {
              duration: 3000,
              panelClass: ['messageBar', 'successMessage'],
              verticalPosition: 'top',
            });
          }
        },
        error: (error) => {
          this.messageBar.open(MESSAGE_CONSTANTS.ME005, '', {
            duration: 3000,
            panelClass: ['messageBar', 'failMessage'],
            verticalPosition: 'top',
          });
        },
      });
    setTimeout(() => {
      this.router.navigate(['/login']);
    }, 3000);
  }
}

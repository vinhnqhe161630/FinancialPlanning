import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormControl,
  Validators,
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
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
export class ForgotPasswordComponent {
  isLoading = false;

  emailFormControl = new FormControl('', [
    Validators.required,
    Validators.email,
  ]);

  constructor(
    private authService: AuthService,
    private messageBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    //Called after the constructor, initializing input properties, and the first call to ngOnChanges.
    //Add 'implements OnInit' to the class.
    if (this.authService.IsLoggedIn()) {
      console.log(this.authService.IsLoggedIn());
      this.router.navigate(['/home']);
    }
  }

  onSubmit() {
    if (this.emailFormControl.invalid) return;
    this.isLoading = true;
    this.authService.forgotPassword(this.emailFormControl.value!).subscribe({
      next: (response:any) => {
        if(response.statusCode ==200){
           this.messageBar.open(
          "We've sent an email with the link to reset your password.",
          undefined,
          {
            duration: 3000,
            panelClass: ['messageBar', 'successMessage'],
            verticalPosition: 'top',
          }
        );
        }else{
          this.messageBar.open(
            'The email address doesn’t exist. Please try again.',
            undefined,
            {
              duration: 3000,
              panelClass: ['messageBar', 'failMessage'],
              verticalPosition: 'top',
            }
          );
         
        }
       
        this.isLoading = false;
       
      },
    });
  }
}

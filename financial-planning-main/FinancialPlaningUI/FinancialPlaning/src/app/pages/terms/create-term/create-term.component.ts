import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  Validators,
} from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { TermService } from '../../../services/term.service';
import { CommonModule } from '@angular/common';
import { CreateTermModel } from '../../../models/term.model';
import { Router, RouterLink } from '@angular/router';
import { MessageBarComponent } from '../../../share/message-bar/message-bar.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatFormField, MatHint, MatLabel } from '@angular/material/form-field';
import {
  MatDatepickerInputEvent,
  MatDatepickerModule,
} from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MESSAGE_CONSTANTS } from '../../../../constants/message.constants';
import { MatInputModule } from '@angular/material/input';
@Component({
  selector: 'app-create-term',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    RouterLink,
    MatFormField,
    MatLabel,
    MatHint,
    MatDatepickerModule,
    MatInputModule,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './create-term.component.html',
  styleUrl: './create-term.component.css',
})
export class CreateTermComponent implements OnInit {
  termForm: FormGroup;
  termService: TermService;

  constructor(
    private fb: FormBuilder,
    termService: TermService,
    private _snackBar: MatSnackBar,
    private router: Router
  ) {
    this.termForm = this.fb.group({
      term: [''],
    });
    this.termService = termService;
  }
  ngOnInit() {
    this.termForm = this.fb.group({
      termName: ['', Validators.required],
      startDate: ['', Validators.required],
      duration: ['1_month', Validators.required],
      endDate: [{ value: '', disabled: true }],
      planDueDate: ['', [Validators.required, this.planDueDateValidator]],
      reportDueDate: ['', [Validators.required, this.reportDueDateValidator]],
    });

    this.termForm.get('startDate')?.valueChanges.subscribe(() => {
      this.updateEndDate();
      this.termForm.get('planDueDate')?.updateValueAndValidity();
      this.termForm.get('reportDueDate')?.updateValueAndValidity();
    });

    this.termForm.get('duration')?.valueChanges.subscribe(() => {
      this.updateEndDate();
      this.termForm.get('planDueDate')?.updateValueAndValidity();
      this.termForm.get('reportDueDate')?.updateValueAndValidity();
    });
  }

  updateEndDate(): void {
    const startDateControl = this.termForm.get('startDate');
    const durationControl = this.termForm.get('duration');
    const endDateControl = this.termForm.get('endDate');
    if (
      startDateControl &&
      startDateControl.valid &&
      durationControl &&
      durationControl.valid &&
      endDateControl
    ) {
      const startDate = new Date(startDateControl.value);
      const duration = durationControl.value;
      let monthsToAdd: number;

      switch (duration) {
        case '1_month':
          monthsToAdd = 1;
          break;
        case 'quarter':
          monthsToAdd = 3;
          break;
        case 'half_year':
          monthsToAdd = 6;
          break;
        default:
          monthsToAdd = 0; // Default to 0 if duration is not recognized
          break;
      }

      const endDate = new Date(startDate);
      endDate.setMonth(endDate.getMonth() + monthsToAdd);
      endDate.setHours(0, 0, 0, 0);
      endDateControl.setValue(this.formatDate(endDate));
    }
  }

  planDueDateValidator(control: any): { [key: string]: boolean } | null {
    // debugger;
    const planDueDate = new Date(control.value).setHours(0, 0, 0, 0);
    const startDate = new Date(
      control?.parent?.controls.startDate.value
    ).setHours(0, 0, 0, 0);
    const endDate = new Date(control?.parent?.controls.endDate.value).setHours(
      0,
      0,
      0,
      0
    );

    if (isNaN(planDueDate)) {
      return { invalidDate: true };
    } else if (planDueDate < startDate) {
      return { invalidRange1: true };
    } else if (planDueDate >= endDate) {
      return { invalidRange2: true };
    }
    return null;
  }

  getPlanDueDateErrorMessage(): string {
    const planDueDateControl = this.termForm.get('planDueDate');
    if (planDueDateControl?.errors) {
      if (planDueDateControl.errors['required']) {
        return 'Plan Due Date is required';
      }
      if (planDueDateControl.errors['invalidDate']) {
        return 'Invalid date format';
      }
      if (planDueDateControl.errors['invalidRange1']) {
        return 'Plan Due Date must be after the Start Date';
      }
      if (planDueDateControl.errors['invalidRange2']) {
        return 'Plan Due Date must be before the End Date';
      }
    }
    return '';
  }

  reportDueDateValidator(control: any): { [key: string]: boolean } | null {
    const reportDueDate = new Date(control.value).setHours(0, 0, 0, 0);
    const startDate = new Date(
      control?.parent?.controls.startDate.value
    ).setHours(0, 0, 0, 0);
    const endDate = new Date(control?.parent?.controls.endDate.value).setHours(
      0,
      0,
      0,
      0
    );

    if (isNaN(reportDueDate)) {
      return { invalidDate: true };
    } else if (reportDueDate < startDate) {
      return { invalidRange1: true };
    } else if (reportDueDate >= endDate) {
      return { invalidRange2: true };
    }
    return null;
  }

  getReportDueDateErrorMessage(): string {
    const reportDueDateControl = this.termForm.get('reportDueDate');
    if (reportDueDateControl?.errors) {
      if (reportDueDateControl.errors['required']) {
        return 'Report Due Date is required';
      }
      if (reportDueDateControl.errors['invalidDate']) {
        return 'Invalid date format';
      }
      if (reportDueDateControl.errors['invalidRange1']) {
        return 'Report Due Date must be after the Start Date';
      }
      if (reportDueDateControl.errors['invalidRange2']) {
        return 'Report Due Date must be before the End Date';
      }
    }
    return '';
  }

  formatDate(date: Date): string {
    return `${date.getFullYear()}-${(date.getMonth() + 1)
      .toString()
      .padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;
  }

  createTerm() {
    const durationMap: { [key: string]: number } = {
      '1_month': 1,
      quarter: 3,
      half_year: 6,
    };
    const durationValue = this.termForm.get('duration')?.value;
    const duration = durationMap[durationValue];
    const termData = new CreateTermModel({
      termName: this.termForm.get('termName')?.value,
      creatorId: '', // You need to set the creatorId
      duration: duration,
      startDate: this.termForm.get('startDate')?.value,
      planDueDate: this.termForm.get('planDueDate')?.value,
      reportDueDate: this.termForm.get('reportDueDate')?.value,
    });
    this.termService.createTerm(termData).subscribe((response) => {
      // console.log(response.status);
      if (response.errors != null) {
        // Redirect to the terms page
        // this.router.navigate(['/terms']);
        const data = { success: false, message: 'Operation failed!' };
        this._snackBar.openFromComponent(MessageBarComponent, {
          duration: 3000,
          data,
        });
      } else {
        // Redirect to the terms page
        this.router.navigate(['/terms']);
        const data = { success: true, message: MESSAGE_CONSTANTS.ME011 };
        this._snackBar.openFromComponent(MessageBarComponent, {
          duration: 3000,
          data,
        });
      }
    });
  }

  onSubmit(): void {
    if (this.termForm.valid) {
      // Submit form data
      console.log(this.termForm.value);
      // Call the service to create the term
      this.createTerm();
    } else {
      // Mark all fields as touched to trigger validation messages
      this.termForm.markAllAsTouched();
    }
  }

  onCancel(): void {
    // Reset form and navigate back to terms page
    // this.termForm.reset(); // Resetting the form
    // this.router.navigate(['/terms']); // Navigate back to terms page
  }
}

import { Component, Inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  Validators,
} from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { TermService } from '../../../services/term.service';
import { CreateTermModel } from '../../../models/term.model';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TermViewModel } from '../../../models/termview.model';
import { MAT_SNACK_BAR_DATA, MatSnackBar } from '@angular/material/snack-bar';
import {
  MatDialog,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { concatMap, of } from 'rxjs';
import { MessageBarComponent } from '../../../share/message-bar/message-bar.component';
import { DialogComponent } from '../../../share/dialog/dialog.component';
import { MESSAGE_CONSTANTS } from '../../../../constants/message.constants';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
@Component({
  selector: 'app-edit-term',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    RouterLink,
    MatDatepickerModule,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './edit-term.component.html',
  styleUrl: './edit-term.component.css',
})
export class EditTermComponent implements OnInit {
  termForm: FormGroup;
  // termService: TermService;
  termId: string = '';
  durationMap: { [key: string]: number } = {
    '1_month': 1,
    quarter: 3,
    half_year: 6,
  };
  durationReverseMap: { [key: number]: string } = {
    1: '1_month',
    3: 'quarter',
    6: 'half_year',
  };
  constructor(
    private fb: FormBuilder,
    private termService: TermService,
    private route: ActivatedRoute,
    private router: Router,
    private messageBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.termService = termService;
    this.termForm = this.fb.group({
      term: [''],
    });
  }
  ngOnInit() {
    this.termId = this.route.snapshot.params['id'];
    this.termForm = this.fb.group({
      termName: ['', Validators.required],
      startDate: ['', Validators.required],
      duration: ['', Validators.required],
      endDate: [{ value: '', disabled: true }],
      planDueDate: ['', [Validators.required, this.planDueDateValidator]],
      reportDueDate: ['', [Validators.required, this.reportDueDateValidator]],
    });
    this.termService.getTerm(this.termId).subscribe(
      (termData: TermViewModel) => {
        if (termData.status != 'New') {
          this.router.navigate(['/terms']);
          this.messageBar.openFromComponent(MessageBarComponent, {
            duration: 3000,
            data: {
              success: false,
              message: 'You can only edit a term with status New',
            },
          });
        }
        this.populateForm(termData);
        console.log(termData);
      },
      (error) => {
        console.error('Error fetching term details:', error);
      }
    );
    // debugger;
    // this.updateEndDate();
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

  populateForm(termData: TermViewModel): void {
    this.termForm.patchValue({
      termName: termData.termName,
      startDate: termData.startDate.slice(0, 10),
      duration: this.durationReverseMap[termData.duration],
      planDueDate: termData.planDueDate.slice(0, 10),
      reportDueDate: termData.reportDueDate.slice(0, 10),
      // Populate other form controls
    });
    this.updateEndDate();
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
      let monthsToAdd = this.durationMap[durationControl.value];
      const endDate = new Date(startDate);
      endDate.setMonth(endDate.getMonth() + monthsToAdd);
      endDateControl.setValue(this.formatDate(endDate));
    }
  }

  closeTerm() {
    const closeDialog = this.dialog.open(DialogComponent, {
      width: '400px',
      height: '250px',
      data: {
        title: 'Close term',
        content: 'Are you sure you want to close this term?',
        note: 'Please, rethink your decision because you will not be able to undo this action',
      },
    });
    closeDialog
      .afterClosed()
      .pipe(
        concatMap((result) => {
          if (result === 'confirm') {
            return this.termService.closeTerm(this.termId);
          } else {
            return of(null);
          }
        })
      )
      .subscribe((response) => {
        console.log(response);
        if (response == null) {
          return;
        }
        this.messageBar.openFromComponent(MessageBarComponent, {
          duration: 3000,
          data: {
            success: true,
            message: 'Term closed successfully',
          },
        });
      });
    // this.router.navigate(['/terms']);
  }

  planDueDateValidator(control: any): { [key: string]: boolean } | null {
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
    const reportDueDate = new Date(control.value);
    const startDate = new Date(control?.parent?.controls.startDate.value);
    const endDate = new Date(control?.parent?.controls.endDate.value);

    if (isNaN(reportDueDate.getTime())) {
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

  editTerm() {
    const durationValue = this.termForm.get('duration')?.value;
    const duration = this.durationMap[durationValue];
    const termData = new CreateTermModel({
      termName: this.termForm.get('termName')?.value,
      creatorId: '', // You need to set the creatorId
      duration: duration,
      startDate: this.termForm.get('startDate')?.value,
      planDueDate: this.termForm.get('planDueDate')?.value,
      reportDueDate: this.termForm.get('reportDueDate')?.value,
    });
    const termId = this.termId; // You need to set the termId
    this.termService.updateTerm(termId, termData).subscribe((response) => {
      this.messageBar.openFromComponent(MessageBarComponent, {
        duration: 3000,
        data: {
          success: true,
          message: MESSAGE_CONSTANTS.ME014,
        },
      });
      this.router.navigate(['/terms']);
    });
  }

  onSubmit(): void {
    if (this.termForm.valid) {
      // Submit form data
      const startDate = new Date(this.termForm.get('startDate')?.value);
      const endDate = new Date(this.termForm.get('endDate')?.value);
      const planDueDate = new Date(this.termForm.get('planDueDate')?.value);
      const reportDueDate = new Date(this.termForm.get('reportDueDate')?.value);
      var message = '';
      if (planDueDate < startDate || planDueDate > endDate) {
        message = 'Plan due date is not within the range.';
      } else if (reportDueDate < startDate || reportDueDate > endDate) {
        message = 'Report due date is not within the range.';
      }

      if (message != '') {
        this.messageBar.openFromComponent(MessageBarComponent, {
          duration: 3000,
          data: {
            success: false,
            message: message,
          },
        });
        return;
      }
      console.log(this.termForm.value);
      // Call the service to create the term
      this.termForm.disable();
      this.editTerm();
      this.termForm.enable();
    } else {
      // Mark all fields as touched to trigger validation messages
      this.termForm.markAllAsTouched();
    }
  }

  onCancel(): void {
    // Handle cancel action
    console.log('Cancel');
  }

  startTerm() {
    const startDialog = this.dialog.open(DialogComponent, {
      width: '400px',
      height: '250px',
      data: {
        title: 'Start term',
        content: 'Are you sure you want to start this term?',
        note: 'Please, rethink your decision because you will not be able to undo this action',
      },
    });
    startDialog
      .afterClosed()
      .pipe(
        concatMap((result) => {
          if (result === 'confirm') {
            return this.termService.startTerm(this.termId);
          } else {
            return of(null);
          }
        })
      )
      .subscribe((response) => {
        console.log(response);
        if (response == null) {
          return;
        }
        this.messageBar.openFromComponent(MessageBarComponent, {
          duration: 3000,
          data: {
            success: true,
            message: 'Start term successfully',
          },
        });
        this.router.navigate(['/terms']);
      });
  }
}

@Component({
  selector: 'start-term',
  standalone: true,
  templateUrl: '../start-term/start-term.component.html',
  styleUrls: ['../start-term/start-term.component.css'],
  imports: [MatDialogActions, MatDialogClose, MatDialogTitle, MatDialogContent],
})
export class StartTermDialog {
  constructor(public dialogRef: MatDialogRef<StartTermDialog>) {}
}

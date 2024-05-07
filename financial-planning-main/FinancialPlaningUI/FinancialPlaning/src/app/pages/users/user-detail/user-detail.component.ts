import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import {
  MatDialog,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { concatMap, of } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { provideNativeDateAdapter } from '@angular/material/core';
import { AuthService } from '../../../services/auth/auth.service';
import { ToastrService } from 'ngx-toastr';
import { MessageBarComponent } from '../../../share/message-bar/message-bar.component';

@Component({
  providers: [provideNativeDateAdapter()],
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FormsModule,
    CommonModule,
    RouterLink,
    MatDatepickerModule,
    MatInputModule,
  ],
  templateUrl: './user-detail.component.html',
  styleUrl: './user-detail.component.css',
})
export class UserDetailComponent implements OnInit {
  userForm: FormGroup;
  pageIndex = 0;
  isLoggedIn!: boolean;
  isCurrentUser!: boolean;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private dialog: MatDialog,
    private messageBar: MatSnackBar,
    private authService: AuthService,
    private toastr: ToastrService
  ) {
    this.userForm = this.fb.group({
      username: [{ value: '', disabled: true }],
      fullname: [{ value: '', disabled: true }],
      dob: [{ value: '', disabled: true }],
      email: [{ value: '', disabled: true }],
      address: [{ value: '', disabled: true }],
      phonenumber: [{ value: '', disabled: true }],
      department: [{ value: '', disabled: true }],
      position: [{ value: '', disabled: true }],
      role: [{ value: '', disabled: true }],
      status: [{ value: '', disabled: true }],
      note: [{ value: '', disabled: true }],
    });
    // Kích hoạt control
    this.userForm.get('status')?.enable();

    // Vô hiệu hóa control
    this.userForm.get('status')?.disable();
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const userId = params['id']; // Assuming 'id' is the parameter name
      this.loadUserDetail(userId);
    });
  }

  loadUserDetail(userId: string): void {
    // debugger;
    this.userService.getUserById(userId).subscribe({
      next: (userDetails: any) => {
        const dobDate = new Date(userDetails.dob);
        const day = dobDate.getDate().toString().padStart(2, '0'); // Thêm số 0 nếu chỉ có 1 số
        const month = (dobDate.getMonth() + 1).toString().padStart(2, '0'); // Thêm số 0 nếu chỉ có 1 số
        const year = dobDate.getFullYear();
        const formattedDob = `${day} / ${month} / ${year}`;

        // Assuming termDetails contains the required data
        this.userForm.patchValue({
          username: userDetails.username,
          fullname: userDetails.fullName,
          dob: formattedDob,
          email: userDetails.email,
          department: userDetails.departmentName,
          position: userDetails.positionName,
          role: userDetails.roleName,
          status: userDetails.status,
          note: userDetails.notes || 'N/A',
          phonenumber: userDetails.phoneNumber,
          address: userDetails.address,
        });
        console.log(userDetails);
        this.isCurrentUser = this.checkIfCurrentUser(userId);
      },
      error: (error: any) => {
        // Handle error
        console.error('Error fetching term details:', error);
      },
    });
  }
  // Check userId login is userId of user detail
  checkIfCurrentUser(userId: string): boolean {
    const loggedInUserId = this.authService.getUserId(); //get userId login from AuthService
    return userId === loggedInUserId;
  }
  onSubmit() {}
  cancel(): void {
    this.router.navigate(['/user-list']);
  }
  openUpdateDialog(): void {
    const userId = this.route.snapshot.paramMap.get('id');
    if (!userId) {
      console.error('User ID is null');
      return;
    }

    const currentStatus = this.userForm.get('status')?.value;
    console.log(currentStatus);
    const newStatus = currentStatus == 1 ? 0 : 1;
    console.log('new' + newStatus);

    const updateDialog = this.dialog.open(UpdateUserStatusDialog, {
      width: '400px',
      height: '250px',
    });

    updateDialog
      .afterClosed()
      .pipe(
        concatMap((result) => {
          if (result === 'update') {
            return this.userService.changeUserStatus(userId, newStatus);
          } else {
            return of(null);
          }
        })
      )
      .subscribe((response) => {
        // Check if response is null, if yes, it means user cancelled, so don't open any message bar
        if (response !== null && response === 200) {
          this.messageBar.openFromComponent(MessageBarComponent, {
            duration: 3000,
            data: {
              success: true,
              message: 'User status updated successfully',
            },
          });
          this.loadUserDetail(userId);
        }
      });
  }
  //Convert date to dd/mm/yyyy
  convertIsoDateToDdMmYyyy(isoDate: string): string {
    if (!isoDate) return '';
    const dateParts = isoDate.split('T')[0].split('-');
    if (dateParts.length !== 3) return isoDate; // Trả về nguyên bản nếu không phải định dạng ISO 8601
    return `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`;
  }
  convertDdMmYyyyToIsoDate(ddMmYyyyDate: string): string {
    if (!ddMmYyyyDate) return '';
    const dateParts = ddMmYyyyDate.split('/');
    if (dateParts.length !== 3) return ddMmYyyyDate; // Trả về nguyên bản nếu không phải định dạng dd/mm/yyyy

    const yyyy = dateParts[2];
    const mm = dateParts[1].padStart(2, '0'); // Đảm bảo mm luôn có 2 chữ số
    const dd = dateParts[0].padStart(2, '0'); // Đảm bảo dd luôn có 2 chữ số

    return `${yyyy}-${mm}-${dd}`;
  }
}
@Component({
  selector: 'app-update-user-status',
  standalone: true,
  imports: [MatDialogActions, MatDialogClose, MatDialogTitle, MatDialogContent],
  templateUrl: '../update-user-status/update-user-status.component.html',
  styleUrl: '../update-user-status/update-user-status.component.css',
})
export class UpdateUserStatusDialog {
  constructor(public dialogRef: MatDialogRef<UpdateUserStatusDialog>) {}
}

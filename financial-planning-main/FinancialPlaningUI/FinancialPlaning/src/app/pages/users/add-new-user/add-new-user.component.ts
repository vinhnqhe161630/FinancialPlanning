import {
  Component,
  ElementRef,
  OnInit,
  ViewChild,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { IDepartment } from '../../../models/department-list';
import { IRole } from '../../../models/role-list';
import { IPosition } from '../../../models/position-list';
import { AddUser } from '../../../models/adduser.model';
import { UserService } from '../../../services/user.service';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MessageBarComponent } from '../../../share/message-bar/message-bar.component';
import { MESSAGE_CONSTANTS } from '../../../../constants/message.constants';
import { DialogComponent } from '../../../share/dialog/dialog.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  providers: [provideNativeDateAdapter()],
  selector: 'app-add-new-user',
  standalone: true,
  imports: [
    RouterLink,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    MatDatepickerModule,
    MatInputModule,
  ],
  templateUrl: './add-new-user.component.html',
  styleUrl: './add-new-user.component.css',
})
export class AddNewUserComponent implements OnInit {
  departments: IDepartment[] = [];
  roles: IRole[] = [];
  positions: IPosition[] = [];
  userId!: string;
  isEdit = false;
  isSubmitting: boolean = false;

  @ViewChild('fullNameInput') fullNameInput!: ElementRef;

  ngAfterViewInit() {
    this.fullNameInput.nativeElement.focus();
  }

  constructor(
    private httpService: UserService,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
    private messageBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.getDepartments();
    this.getRoles();
    this.getPositions();

    this.route.params.subscribe((params) => {
      const userId = params['id'];
      this.getUserById(userId);
    });
  }
  // Set default status is 1
  getStatusLabel(status: number): string {
    return status === 1 ? 'Active' : 'Inactive';
  }

  getUserById(userId: string) {
    if (userId) {
      this.isEdit = true;
      this.userId = userId;

      this.httpService.getUserById(userId).subscribe({
        next: (userDetail: any) => {
          const departmentId = this.departments.find(
            (department) =>
              department.departmentName === userDetail.departmentName
          )?.id;
          const roleId = this.roles.find(
            (role) => role.roleName == userDetail.roleName
          )?.id;
          const positionId = this.positions.find(
            (position) => position.positionName == userDetail.positionName
          )?.id;
          const statusLabel = this.getStatusLabel(userDetail.status);
          this.addUserF.patchValue({
            username: userDetail.username,
            department: userDetail.departmentId,
            role: userDetail.roleId,
            position: userDetail.positionId,
            notes: userDetail.notes,
            email: userDetail.email,
            fullName: userDetail.fullName,
            phoneNumber: userDetail.phoneNumber,
            address: userDetail.address,
            dob: userDetail.dob,
            status: statusLabel,
          });
        },
        error: (error: any) => {
          console.log('Lỗi khi lấy thông tin người dùng:', error);
        },
      });
    } else {
      console.error('Invalid userId:', userId);
    }
  }
  //Get list departments
  getDepartments() {
    this.httpService.getAllDepartment().subscribe(
      (data: IDepartment[]) => {
        this.departments = data;
      },
      (error: any) => {
        console.log('Error fetching departments:', error);
      }
    );
  }

  //Get list roles
  getRoles() {
    this.httpService.getRole().subscribe(
      (data: IRole[]) => {
        this.roles = data;
      },
      (error: any) => {
        console.log('Error fetching departments:', error);
      }
    );
  }

  // Get list positions
  getPositions() {
    this.httpService.getPosition().subscribe(
      (data: IPosition[]) => {
        this.positions = data;
      },
      (error: any) => {
        console.log('Error fetching departments:', error);
      }
    );
  }

  // Validate data

  addUserF: FormGroup = new FormGroup({
    username: new FormControl(''),
    fullName: new FormControl('', [
      Validators.required,
      this.validateName,
      this.validateSpaces,
    ]),
    email: new FormControl('', [
      Validators.required,
      Validators.email,
      this.validateSpaces,
    ]),
    dob: new FormControl('', [Validators.required, this.validateDate]),
    department: new FormControl('', [Validators.required]),
    role: new FormControl('', [Validators.required]),
    notes: new FormControl(''),
    address: new FormControl(''),
    phoneNumber: new FormControl('', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(11),
      Validators.pattern('[0-9]*'),
    ]),
    position: new FormControl('', [Validators.required]),
    status: new FormControl('1'),
  });

  validateName(control: FormControl): { [key: string]: any } | null {
    const vietnameseCharactersRegex = /[^\x00-\x7F]+/; // Biểu thức chính quy để kiểm tra ký tự tiếng Việt
    const containsNumbers = /\d/.test(control.value); // Kiểm tra xem có chứa số không
    const specialCharactersRegex = /[^\w\s]/;

    if (vietnameseCharactersRegex.test(control.value)) {
      return { containsVietnamese: true }; // Có chứa ký tự tiếng Việt
    }
    if (containsNumbers) {
      return { containsNumber: true }; // Có chứa số
    }
    if (specialCharactersRegex.test(control.value)) {
      return { containsSpecialCharacters: true }; // Có chứa ký tự đặc biệt
    }

    return null;
  }
  validateSpaces(control: FormControl): { [key: string]: any } | null {
    const value = control.value;

    // Kiểm tra nếu có nhiều hơn một khoảng trắng giữa các từ hoặc có khoảng trắng ở đầu hoặc cuối
    if (/^\s|\s\s+/.test(value)) {
      return { excessSpaces: true };
    }

    return null;
  }

  // Compare date
  validateDate(control: FormControl): { [key: string]: any } | null {
    const selectedDate = new Date(control.value);
    const currentDate = new Date();

    if (selectedDate > currentDate) {
      return { futureDate: true };
    }

    return null;
  }

  //Submit form
  onSubmit() {
    this.isSubmitting = true;
    if (this.addUserF.invalid) {
      return;
    }
    console.log(this.addUserF.value);
    const user: AddUser = {
      username: this.addUserF.value.username,
      fullName: this.addUserF.value.fullName,
      email: this.addUserF.value.email,
      phoneNumber: this.addUserF.value.phoneNumber,
      dob: this.addUserF.value.dob,
      address: this.addUserF.value.address,
      departmentId: this.addUserF.value.department,
      positionId: this.addUserF.value.position,
      roleId: this.addUserF.value.role,
      status: -1,
      notes: this.addUserF.value.notes,
    };
    if (this.isEdit) {
      this.confirmUpdate(() => {
        user.status = -1;
        this.httpService.editUser(this.userId, user).subscribe(() => {
          console.log('success');
          this.messageBar.openFromComponent(MessageBarComponent, {
            duration: 4000,
            data: {
              success: true,
              message: MESSAGE_CONSTANTS.ME029,
            },
          });
          this.router.navigateByUrl('/user-list');
          this.isSubmitting = false;
        });
      });
    } else {
      user.status = 1;
      this.httpService.addNewUser(user).subscribe(
        () => {
          this.messageBar.openFromComponent(MessageBarComponent, {
            duration: 4000,
            data: {
              success: true,
              message: MESSAGE_CONSTANTS.ME027,
            },
          });
          this.router.navigateByUrl('/user-list');
          this.isSubmitting = false;
        },
        (error: any) => {
          if (error.status === 400) {
            // Handle bad request error
            console.log('Bad Request Error:', error);
            this.messageBar.openFromComponent(MessageBarComponent, {
              duration: 4000,
              data: {
                success: false,
                message: 'Email is exist: Please check your email',
              },
            });
          } else {
            // Handle other errors
            console.error('Error:', error);
            this.messageBar.openFromComponent(MessageBarComponent, {
              duration: 4000,
              data: {
                success: false,
                message: 'An error occurred while creating user',
              },
            });
          }
          this.isSubmitting = false;
        }
      );
    }
  }
  confirmUpdate(callback: () => void) {
    const dialogRef = this.dialog.open(DialogComponent, {
      width: '400px',
      height: '250px',
      data: {
        title: 'Update user',
        content: 'Are you sure you want to update this user?',
        note: 'Please, rethink your decision because this will affect to user',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        callback();
      } else {
        this.isSubmitting = false;
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

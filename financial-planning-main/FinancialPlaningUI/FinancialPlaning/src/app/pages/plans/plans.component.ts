import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { SidenavComponent } from '../../share/sidenav/sidenav.component';
import { RouterLink } from '@angular/router';
import {
  MatDialog,
  MatDialogRef,
  MatDialogActions,
  MatDialogClose,
  MatDialogTitle,
  MatDialogContent,
} from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import {
  MatPaginator,
  MatPaginatorModule,
  PageEvent,
} from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { PlanService } from '../../services/plan.service';
import { jwtDecode } from 'jwt-decode';
import { of } from 'rxjs';
import { concatMap } from 'rxjs/operators';
import { MatSelectModule } from '@angular/material/select';
import { Plan } from '../../models/planviewlist.model';
import { MessageBarComponent } from '../../share/message-bar/message-bar.component';

@Component({
  selector: 'app-terms',
  standalone: true,
  templateUrl: './plans.component.html',
  styleUrl: './plans.component.css',
  imports: [
    CommonModule,
    SidenavComponent,
    RouterLink,
    MatPaginatorModule,
    MatIconModule,
    MatTableModule,
    MatSnackBarModule,
    MatPaginatorModule,
    MatIconModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
  ],
})
export class PlansComponent implements OnInit {
  planList: any = [];

  role: string = '';
  // filter
  searchValue: string = '';

  terms: any = [];
  selectedTerm = 'All';

  departments: any = [];
  selectedDepartment = 'All';

  status: any = [];
  selectedStatus = 'All';

  quarters: any[] = [];
  selectedQuarter = 'All';

  listSize: number = 0;
  pageSize = 8;
  pageIndex = 0;
  dataSource: any = [];

  columnHeaders: string[] = [
    'no',
    'plan',
    'term',
    'department',
    'status',
    'version',
    'action',
  ];
  filterStatusEnable = false;

  showEditDeleteButton(plan: Plan): boolean {
    return (
      (this.role === 'Accountant' &&
        plan.department.toLowerCase() ===
          this.getUsersDepartment().toLowerCase()) ||
      this.role === 'FinancialStaff'
    );
  }

  constructor(
    private planService: PlanService,
    private elementRef: ElementRef,
    private dialog: MatDialog,
    private messageBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    //Get role
    if (typeof localStorage !== 'undefined') {
      const token = localStorage.getItem('token') ?? '';
      if (token) {
        const decodedToken: any = jwtDecode(token);
        this.role = decodedToken.role;
        this.fetchData();
      }
    }
  }

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  fetchData() {
    console.log(this.role);
    this.planService.getAllPlans().subscribe((data: any) => {
      this.planList = data;

      // Lọc dữ liệu dựa trên vai trò
      if (this.role === 'Accountant') {
        // Chỉ hiển thị các kế hoạch không phải ở trạng thái "New"
        this.planList = this.planList.filter(
          (plan: Plan) =>
            plan.department.toLowerCase() ===
              this.getUsersDepartment().toLowerCase() ||
            (plan.department.toLowerCase() !==
              this.getUsersDepartment().toLowerCase() &&
              plan.status !== 'New')
        );
      }

      // Lọc dữ liệu dựa trên vai trò
      if (this.role === 'FinancialStaff') {
        // Hiển thị chỉ các kế hoạch trong phòng ban của người dùng
        this.planList = this.planList.filter(
          (plan: Plan) =>
            plan.department.toLowerCase() ===
            this.getUsersDepartment().toLowerCase()
        );
      }
      this.terms = Array.from(
        new Set(this.planList.map((plan: Plan) => plan.term))
      );
      this.departments = Array.from(
        new Set(this.planList.map((plan: Plan) => plan.department))
      );
      this.status = Array.from(
        new Set(this.planList.map((plan: Plan) => plan.status))
      );

      this.dataSource = this.getPaginatedItems();
      console.log('Fetch data');
    });
  }
  getUsersDepartment(): string {
    let userDepartment = '';
    if (typeof localStorage !== 'undefined') {
      const token = localStorage.getItem('token') ?? '';
      if (token) {
        const decodedToken: any = jwtDecode(token);
        // Giả sử thông tin phòng ban được lưu trong trường 'department' của token
        userDepartment = decodedToken.departmentName ?? '';
      }
    }
    return userDepartment;
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.dataSource = this.getPaginatedItems();
  }

  getPaginatedItems() {
    const startIndex = this.pageIndex * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    let filteredList = this.planList;

    // Apply filtering based on search text
    if (this.searchValue.trim() !== '') {
      filteredList = filteredList.filter((plan: Plan) =>
        plan.plan.toLowerCase().includes(this.searchValue.toLowerCase())
      );
    }

    // Apply filtering based on selected department
    if (this.selectedDepartment !== 'All') {
      filteredList = filteredList.filter(
        (plan: Plan) =>
          plan.department.toLowerCase() ===
          this.selectedDepartment.toLowerCase()
      );
    }

    if (this.selectedTerm !== 'All') {
      filteredList = filteredList.filter(
        (plan: Plan) =>
          plan.term.toLowerCase() === this.selectedTerm.toLowerCase()
      );
    }

    // Apply filtering based on selected status
    if (this.selectedStatus !== 'All') {
      filteredList = filteredList.filter(
        (plan: Plan) =>
          plan.status.toLowerCase() === this.selectedStatus.toLowerCase()
      );
    }

    this.listSize = filteredList.length;
    return filteredList.slice(startIndex, endIndex);
  }

  changeSearchText(event: Event) {
    let target = event.target as HTMLInputElement;
    this.searchValue = target.value.trim();
    this.pageIndex = 0;
    this.dataSource = this.getPaginatedItems();
  }

  onDepartmentSelected(event: any): void {
    console.log(event.value);
    this.selectedDepartment = event.value;
    this.dataSource = this.getPaginatedItems();
  }

  onStatusSelected(event: any): void {
    console.log(event.value);
    this.selectedStatus = event.value;
    this.dataSource = this.getPaginatedItems();
  }
  onTermSelected(event: any): void {
    console.log(event.value);
    this.selectedTerm = event.value;
    this.dataSource = this.getPaginatedItems();
  }
  resetFilters() {
    this.searchValue = '';
    this.selectedDepartment = 'All';
    this.selectedStatus = 'All';
    this.selectedTerm = 'All';

    // Gọi lại fetchData() để cập nhật dữ liệu mới sau khi đặt lại bộ lọc
    this.fetchData();
  }
  openDeleteDialog(id: string) {
    const deleteDialog = this.dialog.open(DeletePlanDialog, {
      width: '400px',
      height: '250px',
    });

    deleteDialog
      .afterClosed()
      .pipe(
        concatMap((result) => {
          if (result === 'delete') {
            return this.planService.deletePlan(id);
          } else {
            return of(null);
          }
        })
      )
      .subscribe((response) => {
        if (response == null) {
          return;
        }
        this.messageBar.openFromComponent(MessageBarComponent, {
          duration: 3000,
          data: {
            httpStatusCode: response,
            success: response == 200,
            rmclose: true,
            message:
              response == 200
                ? 'Plan deleted successfully'
                : 'Failed to delete Plan',
          },
        });
        this.pageIndex = 0;
        this.fetchData();
      });
  }
}
@Component({
  selector: 'delete-plan',
  standalone: true,
  templateUrl: './delete-plan/delete-plan.component.html',
  styleUrls: ['./delete-plan/delete-plan.component.css'],
  imports: [MatDialogActions, MatDialogClose, MatDialogTitle, MatDialogContent],
})
export class DeletePlanDialog {
  constructor(public dialogRef: MatDialogRef<DeletePlanDialog>) {}
}

import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  ViewChild,
  ElementRef,
} from '@angular/core';
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
import {
  MatSnackBar,
  MatSnackBarModule,
} from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { TermService } from '../../services/term.service';
import { jwtDecode } from 'jwt-decode';
import { of } from 'rxjs';
import { concatMap } from 'rxjs/operators';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MessageBarComponent } from '../../share/message-bar/message-bar.component';

@Component({
  selector: 'app-terms',
  standalone: true,
  templateUrl: './terms.component.html',
  styleUrl: './terms.component.css',
  imports: [
    CommonModule,
    SidenavComponent,
    RouterLink,
    MatPaginatorModule,
    MatIconModule,
    MatTableModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatSelectModule,
  ],
})
export class TermsComponent implements OnInit {
  termList: any = [];

  role: string = '';

  statusOption: string = 'All';
  searchValue: string = '';
  filterStatusEnable = false;

  listSize: number = 0;
  pageSize = 8;
  pageIndex = 0;
  dataSource: any = [];

  columnHeaders: string[] = [
    'term',
    'startDate',
    'endDate',
    'status',
    'action',
  ];

  constructor(
    private termService: TermService,
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
    this.termService.getAllTerms().subscribe((data: any) => {
      this.termList = data;
      this.dataSource = this.getPaginatedItems();
      console.log(this.dataSource);
    });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.dataSource = this.getPaginatedItems();
  }

  getPaginatedItems() {
    const startIndex = this.pageIndex * this.pageSize;
    let filteredList = this.termList.filter(
      (data: any) =>
        data.termName.toLowerCase().includes(this.searchValue.toLowerCase()) &&
        (this.statusOption === 'All' || data.status === this.statusOption)
    );
    this.listSize = filteredList.length;
    return filteredList.slice(startIndex, startIndex + this.pageSize);
  }

  changeSearchText(event: Event) {
    let target = event.target as HTMLInputElement;
    this.searchValue = target.value.trim();
    this.pageIndex = 0;
    this.dataSource = this.getPaginatedItems();
  }

  changeStatusFilter(event: any) {
    this.statusOption = event.value;
    this.pageIndex = 0;
    this.dataSource = this.getPaginatedItems();
  }
  //Convert date to dd/mm/yyyy
  convertIsoDateToDdMmYyyy(isoDate: string): string {
    if (!isoDate) return '';
    const dateParts = isoDate.split('T')[0].split('-');
    if (dateParts.length !== 3) return isoDate; // Trả về nguyên bản nếu không phải định dạng ISO 8601
    return `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`;
  }
  openDeleteDialog(id: string) {
    const deleteDialog = this.dialog.open(DeleteTermDialog, {
      width: '400px',
      height: '250px',
    });

    deleteDialog
      .afterClosed()
      .pipe(
        concatMap((result) => {
          if (result === 'delete') {
            return this.termService.deleteTerm(id);
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
            message:
              response == 200
                ? 'Term deleted successfully'
                : 'Failed to delete term',
          },
        });
        this.pageIndex = 0;
        this.fetchData();
      });
  }
}

@Component({
  selector: 'delete-term',
  standalone: true,
  templateUrl: './delete-term/delete-term.component.html',
  styleUrls: ['./delete-term/delete-term.component.css'],
  imports: [MatDialogActions, MatDialogClose, MatDialogTitle, MatDialogContent],
})
export class DeleteTermDialog {
  constructor(public dialogRef: MatDialogRef<DeleteTermDialog>) {}
}

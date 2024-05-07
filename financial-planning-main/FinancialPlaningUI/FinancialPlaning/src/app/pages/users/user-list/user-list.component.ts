import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import {
  MatPaginator,
  MatPaginatorModule,
  PageEvent,
} from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { UserModel } from '../../../models/user.model';
import { UserService } from '../../../services/user.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css',
  imports: [
    CommonModule,
    RouterLink,
    MatPaginatorModule,
    FormsModule,
    HttpClientModule,
    RouterOutlet,
    MatTableModule,
    MatIconModule,
  ],
})
export class UserListComponent implements OnInit {
  userList: UserModel[] = [];
  originalUserList: UserModel[] = [];
  router = inject(Router);
  selectedRole: string = '';
  roles: string[] = [];
  searchValue: string = '';
  dataSource: any = [];
  nextId: number = 1;
  columnHeaders: string[] = [
    'index',
    'username',
    'email',
    'department',
    'position',
    'action',
  ];
  @ViewChild(MatPaginator) paginator?: MatPaginator;
  pageIndex = 0;
  pageSize = 10;
  listSize = 0;

  constructor(private httpService: UserService) {}

  ngOnInit(): void {
    this.getListUsers();
  }

  getListUsers(): void {
    this.httpService.getAllUser().subscribe((data: any) => {
      this.userList = data;
      this.originalUserList = data;
      this.roles = Array.from(
        new Set(data.map((user: UserModel) => user.roleName))
      );
      this.getPaginatedItems();
    });
  }

  filterUsers(): void {
    // Filter based on selected role and search input
    let filteredList = this.originalUserList;
    this.pageIndex = 0;

    // Apply role filter
    if (this.selectedRole) {
      filteredList = filteredList.filter(
        (user) => user.roleName === this.selectedRole
      );
    }

    // Apply search filter
    if (this.searchValue) {
      filteredList = filteredList.filter((user) =>
        user.username
          .toLowerCase()
          .includes(this.searchValue.toLowerCase().trim())
      );
    }

    this.userList = filteredList;
    this.getPaginatedItems();
  }
  search(): void {
    // Filter based on selected role and search input
    let filteredList = this.originalUserList;

    // Apply role filter
    if (this.selectedRole) {
      filteredList = filteredList.filter(
        (user) => user.roleName === this.selectedRole
      );
    }

    // Apply search filter
    if (this.searchValue) {
      filteredList = filteredList.filter((user) =>
        user.username.toLowerCase().includes(this.searchValue.toLowerCase())
      );
    }
    this.userList = filteredList;
    this.getPaginatedItems();
  }
  onChange(): void {
    this.filterUsers();
  }

  edit(id: string) {
    console.log(id);
    this.router.navigateByUrl('/edit-user/' + id);
  }

  getPaginatedItems(): void {
    const startIndex = this.pageIndex * this.pageSize;
    this.listSize = this.userList.length;
    this.dataSource = this.userList.slice(
      startIndex,
      startIndex + this.pageSize
    );
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.getPaginatedItems();
  }
}

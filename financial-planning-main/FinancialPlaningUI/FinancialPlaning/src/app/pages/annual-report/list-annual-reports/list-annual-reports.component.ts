import { CommonModule } from '@angular/common';
import { Component, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { AnnualReportService } from '../../../services/annual-report.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-list-annual-reports',
  standalone: true,
  imports: [
    RouterLink,
    MatPaginatorModule,
    CommonModule,
    MatTableModule,
    MatIconModule
  ],
  templateUrl: './list-annual-reports.component.html',
  styleUrl: './list-annual-reports.component.css'
})
export class ListAnnualReportsComponent {
  displayedColumns: string[] = ['Index','Year','TotalExpense',
  'TotalDepartment','Create-Date','Action'];
  dataSource: any = [];
  annualReports: any = [];
  searchValue: string = '';
   //paging
   listSize: number = 0;
   pageSize = 8;
   pageIndex = 0;

   constructor(
    private annualReportService:AnnualReportService
   ){
       this.dataSource = new MatTableDataSource<any>();
   }

   ngOnInit(): void {
      this.fetchData();
   }
   @ViewChild(MatPaginator) paginator!: MatPaginator;

   fetchData() {
    this.annualReportService.getListAnnualReport().subscribe((data: any) => {
      this.annualReports = data;
      this.dataSource = this.getPaginatedItems();
      console.log(data);
    });
   }
onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.dataSource = this.getPaginatedItems();
  }

 getPaginatedItems() {
    const startIndex = this.pageIndex * this.pageSize;
    let filteredList = this.annualReports.filter(
      (data: any) =>
      data.year.toString().includes(this.searchValue)
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
  //Convert date to dd/mm/yyyy
 convertIsoDateToDdMmYyyy(isoDate: string): string {
  if (!isoDate) return '';
  const dateParts = isoDate.split('T')[0].split('-');
  if (dateParts.length !== 3) return isoDate; // Trả về nguyên bản nếu không phải định dạng ISO 8601
  return `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`;
}
}

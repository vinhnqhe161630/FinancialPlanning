import { CommonModule } from '@angular/common';
import { Component, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AnnualReportService } from '../../../services/annual-report.service';

@Component({
  selector: 'app-annual-report-details',
  standalone: true,
  imports: [ 
    CommonModule,
    MatTableModule,
    MatSelectModule,
    MatPaginatorModule,
  RouterLink],
  templateUrl: './annual-report-details.component.html',
  styleUrl: './annual-report-details.component.css'
})
export class AnnualReportDetailsComponent {
  displayedColumns: string[] = ['No','Department','TotalExpense','BiggestExpenditure','CostType'];

  dataSource: any = [];
  annualReport: any = [];
  expenses: any = [];

  searchValue: string = '';
  //paging
  listSize: number = 0;
  pageSize = 8;
  pageIndex = 0;

  constructor(
    private annualReportService: AnnualReportService,
    private route: ActivatedRoute,
    private dialog: MatDialog,
  ) {
    this.dataSource = new MatTableDataSource<any>();
  }
ngOnInit(): void {
    this.route.params.subscribe(params => {
      const year = params['year']; // Assuming 'id' is the parameter name
      this.getAnnualReport(year);

    });
  }
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  getAnnualReport(year: number) {
    this.annualReportService.getAnnualReportDetails(year).subscribe((data: any) => {

      //List expenses
      this.expenses = data.expenses;
      
      //data of report
      this.annualReport = data.report;  
      //filter
      this.dataSource = this.getPaginatedItems();
      console.log(data);
    });
  }
  getPaginatedItems() {
    const startIndex = this.pageIndex * this.pageSize;
    let filteredList = this.expenses.filter(
      (data: any) =>
      data.department.toLowerCase().includes(this.searchValue.toLowerCase())
    );;
    this.listSize = filteredList.length;
    return filteredList.slice(startIndex, startIndex + this.pageSize);
  }
   //paging
   onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.dataSource = this.getPaginatedItems();
  }
 //Convert date to dd/mm/yyyy
 convertIsoDateToDdMmYyyy(isoDate: string): string {
  if (!isoDate) return '';
  const dateParts = isoDate.split('T')[0].split('-');
  if (dateParts.length !== 3) return isoDate; // Trả về nguyên bản nếu không phải định dạng ISO 8601
  return `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`;
}
changeSearchText(event: Event) {
  let target = event.target as HTMLInputElement;
  this.searchValue = target.value.trim();
  this.pageIndex = 0;
  this.dataSource = this.getPaginatedItems();
}

//export file
downloadFile(year: number) {
  this.annualReportService.exportAnnualReport(year).subscribe((data: any) => {
      const downloadUrl = data.result;
       // create hidden link to download
       const link = document.createElement('a');
       link.href = downloadUrl;
       link.setAttribute('download', '');

       // Add link into web and click it to download
       document.body.appendChild(link);
       link.click();

       // remove link after download 
       document.body.removeChild(link)
    }, 
  );
}
}

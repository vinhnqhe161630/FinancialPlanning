import { Component, ElementRef, OnInit } from '@angular/core';
import { UploadComponent } from '../../../share/upload/upload.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatOption, MatSelect } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { TermService } from '../../../services/term.service';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { ReportService } from '../../../services/report.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { jwtDecode } from 'jwt-decode';
import { Router, RouterLink } from '@angular/router';
import { MatCard } from '@angular/material/card';
import { start } from 'repl';
import { SelectTermModel } from '../../../models/select-term.model';
import { ActivatedRoute } from '@angular/router';
import { MessageBarComponent } from '../../../share/message-bar/message-bar.component';

@Component({
  selector: 'app-import-report',
  standalone: true,
  imports: [
    UploadComponent,
    MatFormFieldModule,
    MatSelect,
    MatOption, CommonModule,
    MatPaginatorModule,
    MatTableModule,
    ReactiveFormsModule,
    RouterLink,
    MatCard
  ],
  templateUrl: './reup-report.component.html',
  styleUrls: ['./reup-report.component.css']
})
export class ReupReportComponent implements OnInit {

  reportService: ReportService;
  term:any
  month:any
  // reportForm: FormGroup;
  reportId: string = '';
  dataSource: any = [];
  //paging
  listSize: number = 0;
  pageSize = 8;
  pageIndex = 0;
  filedata: any = [];
  file: any;
  dueDate: Date = new Date();
  columnHeaders: string[] = [
    'expense',
    'costType',
    'unitPrice',
    'amount',
    'total',
    'projectName',
    'supplierName',
    'pic',
    'notes'
  ];
  loading: boolean = false;
  validFileName: string = '';
  constructor(
    reportService: ReportService,
    private fb: FormBuilder,
    private elementRef: ElementRef,
    private messageBar: MatSnackBar,
    private route: ActivatedRoute,
    private router: Router) {
    this.reportService = reportService;
    // this.reportForm = this.fb.group({
    //   term: ['', Validators.required],
    //   month: ['', Validators.required],
    //   // fileInput: [null, Validators.required]
    // });
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      const reportId = params['id'];
      this.reportService.getReport(reportId).subscribe((data: any) => {
          this.reportId = reportId;
          this.term = data.report.termName;
          this.month = data.report.month;
          var department = data.report.departmentName;
          this.dueDate = new Date(data.report.reportDureDate);
          var currentDate = new Date();
          if (currentDate > this.dueDate) {
            this.messageBar.openFromComponent(MessageBarComponent, {
              duration: 3000,
              data: {
                rmclose: true,
                message: 'Report is overdue.',
              },
            });
            this.router.navigate(['/report-details/' + data.report.id]);
          }
          this.validFileName = `${department}_${ this.term}_${this.month.split(' ')[0]}_Report`;
          console.log('Report data:', data);
          console.log('validFileName:', this.validFileName);
          
      });
      // Use the reportId as needed
    });
  }

  //filter page
  getPaginatedItems() {
    const startIndex = this.pageIndex * this.pageSize;
    let filteredList = this.filedata;
    this.listSize = filteredList.length;
    return filteredList.slice(startIndex, startIndex + this.pageSize);
  }
  
  onImport(event: any) {
    this.file = event;
    if (this.file) {
      this.loading = true;
      console.log('Importing file:', this.file);
      this.reportService.importReport(this.file).subscribe(
        (data: any) => {
          this.filedata = data;
          this.dataSource = this.getPaginatedItems();
          console.log(data);
          this.loading = false;
        },
        error => {
          console.log(error);
          this.messageBar.open(
            error.error.message,
            undefined,
            {
              duration: 3000,
              panelClass: ['messageBar', 'successMessage'],
              verticalPosition: 'top',
              horizontalPosition: 'end',
            }
          );
          this.loading = false;
        }
      );
    } else {
      this.messageBar.open(
        "Please select a file to preview.",
        undefined,
        {
          duration: 3000,
          panelClass: ['messageBar', 'successMessage'],
          verticalPosition: 'top',
          horizontalPosition: 'end',
        }
      );
    }
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.dataSource = this.getPaginatedItems();
  }

  onSubmit() {
    // debugger;
    // var term = this.reportForm.value.term;
    if (this.file) {
      this.elementRef.nativeElement.querySelector('#submit-button').disabled = true;
      this.reportService.reupReport(this.filedata, this.reportId).subscribe(
        (data: any) => {
          console.log('report uploaded:', data);
          this.messageBar.openFromComponent(MessageBarComponent, {
            duration: 3000,
            data: {
            success: true,
            rmclose: true ,
            message: 'Uploaded successfully',
           },});
          this.router.navigate(['/reports']);
        },
        error => {
          this.elementRef.nativeElement.querySelector('.submit-button').disabled = false;
          console.log('Error uploading report:', error);
          this.messageBar.openFromComponent(MessageBarComponent, {
            duration: 3000,
           data: {
            success: false,
             rmclose: true ,
             message: error.error.message,
           },});
        }
      );
    } else {
      // console.log('Please select a file to upload.');
      // this.messageBar.open(
      //   "Please select a file to upload.",
      //   undefined,
      //   {
      //     duration: 3000,
      //     panelClass: ['messageBar', 'successMessage'],
      //     verticalPosition: 'top',
      //     horizontalPosition: 'end',
      //   }
      // );
      this.messageBar.openFromComponent(MessageBarComponent, {
        duration: 3000,
       data: {
        success: false,
         rmclose: true ,
         message: "Please select a file to upload.",
       },});
      this.elementRef.nativeElement.querySelector('.submit-button').disabled = false;
    }
  }

  onFileSelected(event: any) {
    // debugger;
    this.file = event;
    console.log('Selected file:', this.file);
  }
  exportReportTemplate(){
    this.reportService.exportTemplateReport().subscribe(
      (data: Blob) => {
        const downloadURL = window.URL.createObjectURL(data);
        const link = document.createElement('a');
        link.href = downloadURL;
        link.download = 'Template Report.xlsx';
        link.click();
      }

    );
  }
}




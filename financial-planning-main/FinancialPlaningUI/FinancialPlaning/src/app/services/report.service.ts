import {
  HttpClient,
  HttpErrorResponse,
  HttpResponse,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private apiUrl = environment.apiUrl + '/Report';
  constructor(private http: HttpClient) {}

  getListReport(): Observable<any> {
    return this.http.get<any>(this.apiUrl + '/reports');
  }

  deleteReport(reportId: string): Observable<number> {
    return this.http
      .delete(this.apiUrl + '/' + reportId, {
        observe: 'response',
        responseType: 'text',
      })
      .pipe(
        map((response: HttpResponse<any>) => response.status),
        catchError((error: HttpErrorResponse) => {
          console.error('Error occurred:', error);
          throw error;
        })
      );
  }

  getReport(reportId: string): Observable<any> {
    return this.http.get(this.apiUrl + '/details/' + reportId);
  }

  exportSinglereport(reportId: string, version: number): Observable<Blob> {
    return this.http.post<Blob>(
      `${this.apiUrl}/export/${reportId}/${version}`,
      null,
      { responseType: 'blob' as 'json' }
    );
  }

  exportMutilreport(reportIds: string[]): Observable<Blob> {
    return this.http.post<Blob>(`${this.apiUrl}/export`, reportIds, {
      responseType: 'blob' as 'json',
    });
  }
  
  exportTemplateReport(): Observable<Blob> {
    return this.http.get<Blob>(`${this.apiUrl}/exportTemplate`, {
      responseType: 'blob' as 'json',
    });
  }

  importReport(file: any): Observable<any> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post(this.apiUrl + '/import', formData);
  }

  uploadReport(expenses: [], termId: string, month: string): Observable<any> {
    const token = localStorage.getItem('token') ?? '';
    const decodedToken: any = jwtDecode(token);
    const uid = decodedToken.userId;
    const urlParams = new URLSearchParams();
    urlParams.append('uid', uid);
    urlParams.append('termId', termId);
    urlParams.append('month', month);
    return this.http.post(this.apiUrl + '/upload?' + urlParams, expenses);
  }

  reupReport(expenses: [], reportId: string): Observable<any> {
    const token = localStorage.getItem('token') ?? '';
    const decodedToken: any = jwtDecode(token);
    const uid = decodedToken.userId;
    const urlParams = new URLSearchParams();
    urlParams.append('reportId', reportId);
    urlParams.append('uid', uid);
    return this.http.post(this.apiUrl + '/reupload?' + urlParams, expenses);
  }
}

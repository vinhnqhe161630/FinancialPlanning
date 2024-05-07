import { Injectable } from '@angular/core';
import {
  HttpClient,
  HttpErrorResponse,
  HttpParams,
  HttpResponse,
} from '@angular/common/http';
import { Observable, catchError, map } from 'rxjs';
import { Plan } from '../models/planviewlist.model';
import { environment } from '../../environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
@Injectable({
  providedIn: 'root',
})
export class PlanService {
  private apiUrl = environment.apiUrl + '/Plan';

  constructor(private http: HttpClient) {}

  getFinancialPlans(
    keyword: string = '',
    department: string = '',
    status: string = ''
  ): Observable<Plan[]> {
    // Tạo các tham số query
    let params = new HttpParams();
    if (keyword) {
      params = params.set('keyword', keyword);
    }
    if (department) {
      params = params.set('department', department);
    }
    if (status) {
      params = params.set('status', status);
    }

    // Thực hiện gọi HTTP GET đến API endpoint
    return this.http.get<Plan[]>(`${this.apiUrl}`, { params });
  }

  getAllPlans(): Observable<any> {
    return this.http.get(this.apiUrl + '/Planlist');
  }

  exportPlanTemplate(): Observable<Blob> {
    return this.http.get<Blob>(`${this.apiUrl}/exportTemplate`, {
      responseType: 'blob' as 'json',
    });
  }

  getPlanById(planId: string): Observable<Plan> {
    return this.http.get<Plan>(`${this.apiUrl}/${planId}`);
  }

  importPlan(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + '/import', formData);
  }

  reupPlan(file: File, planId: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    const urlParams = new URLSearchParams();
    urlParams.append('planId', planId);
    return this.http.post(this.apiUrl + '/reup?' + urlParams, formData);
  }

  createPlan(termId: string, expenses: []): Observable<any> {
    const token = localStorage.getItem('token') ?? '';
    const decodedToken: any = jwtDecode(token);
    const uid = decodedToken.userId;
    const urlParams = new URLSearchParams();
    urlParams.append('termId', termId);
    urlParams.append('uid', uid);
    return this.http.post(this.apiUrl + '/create?' + urlParams, expenses);
  }

  editPlan(planId: string, expenses: []): Observable<any> {
    const token = localStorage.getItem('token') ?? '';
    const decodedToken: any = jwtDecode(token);
    const userId = decodedToken.userId;
    const urlParams = new URLSearchParams();
    urlParams.append('planId', planId);
    urlParams.append('userId', userId);
    return this.http.put(this.apiUrl + '/edit?' + urlParams, expenses);
  }

  deletePlan(PlanId: string): Observable<number> {
    return this.http
      .delete(this.apiUrl + '/' + PlanId, {
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

  getPlan(planId: string): Observable<any> {
    return this.http.get(this.apiUrl + '/details/' + planId);
  }

  exportPlan(planId: string, version: number): Observable<Blob> {
    return this.http.post<Blob>(
      `${this.apiUrl}/export/${planId}/${version}`,
      null,
      { responseType: 'blob' as 'json' }
    );
  }

  submitPlan(PlanId: string): Observable<number> {
    return this.http
      .put(
        this.apiUrl + '/' + PlanId + '/submit' ,
        {},
        {
          observe: 'response',
          responseType: 'text',
        }
      )
      .pipe(
        map((response: HttpResponse<any>) => response.status),
        catchError((error: HttpErrorResponse) => {
          console.error('Error occurred:', error);
          throw error;
        })
      );
  }

  approvePlan(PlanId: string): Observable<number> {
    return this.http
      .put(
        this.apiUrl + '/' + PlanId + '/approve',
        {},
        {
          observe: 'response',
          responseType: 'text',
        }
      )
      .pipe(
        map((response: HttpResponse<any>) => response.status),
        catchError((error: HttpErrorResponse) => {
          console.error('Error occurred:', error);
          throw error;
        })
      );
  }
  
  submitExpense(PlanId: string, approvedExpenses: any): any {
    return this.http
      .put(
        this.apiUrl + '/' + PlanId + '/' + approvedExpenses,
        {},
        {
          observe: 'response',
          responseType: 'text',
        }
      )
      .pipe(
        map((response: HttpResponse<any>) => response.status),
        catchError((error: HttpErrorResponse) => {
          console.error('Error occurred:', error);
          throw error;
        })
      );
  }
}

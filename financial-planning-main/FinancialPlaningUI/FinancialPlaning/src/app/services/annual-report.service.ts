import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AnnualReportService {
  private apiUrl = environment.apiUrl + '/AnnualReport';
  constructor(private http: HttpClient) {}

  getListAnnualReport(): Observable<any> {
    return this.http.get<any>(this.apiUrl + '/annualreports');
  }

  getAnnualReportDetails(year: number): Observable<any> {
    return this.http.get(this.apiUrl + '/details/' + year);
  }

  exportAnnualReport(year: number) {
    return this.http.get(this.apiUrl + '/export/' + year);
  }
}

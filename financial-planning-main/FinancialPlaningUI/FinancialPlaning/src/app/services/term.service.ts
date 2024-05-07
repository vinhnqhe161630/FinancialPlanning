import { Injectable } from '@angular/core';
import {
  HttpClient,
  HttpResponse,
  HttpErrorResponse,
} from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { CreateTermModel } from '../models/term.model';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class TermService {
  private apiUrl = environment.apiUrl + '/Term';
  constructor(private http: HttpClient) {} // Correct injection through

  createTerm(term: CreateTermModel): Observable<any> {
    const token = localStorage.getItem('token') ?? '';
    const decodedToken: any = jwtDecode(token);
    const uid = decodedToken.userId;
    term.creatorId = uid;
    console.log(term);
    return this.http.post(this.apiUrl, term);
  }

  getAllTerms(): Observable<any> {
    return this.http.get(this.apiUrl + '/all');
  }

  deleteTerm(termId: string): Observable<number> {
    return this.http
      .delete(this.apiUrl + '/' + termId, {
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

  updateTerm(termId: string, term: CreateTermModel): Observable<any> {
    const token = localStorage.getItem('token') ?? '';
    const decodedToken: any = jwtDecode(token);
    const uid = decodedToken.userId;
    term.creatorId = uid;
    return this.http.put(this.apiUrl + '/update/' + termId, term);
  }

  getTerm(termId: string): Observable<any> {
    return this.http.get(this.apiUrl + '/' + termId);
  }

  getStartedTerms(): Observable<any> {
    return this.http.get(this.apiUrl + '/started');
  }

  startTerm(termId: string): Observable<any> {
    return this.http.put(this.apiUrl + '/start/' + termId, {});
  }

  closeTerm(termId: string): Observable<any> {
    return this.http.put(this.apiUrl + '/close/' + termId, {});
  }

  getTermsToImportPlan(): Observable<any> {
    return this.http.get(this.apiUrl + '/noplan');
  }

  getTermsToImportReport(): Observable<any> {
    return this.http.get(this.apiUrl + '/noreport');
  }
}

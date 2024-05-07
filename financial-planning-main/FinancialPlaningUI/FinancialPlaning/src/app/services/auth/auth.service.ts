import {
  HttpClient,
  HttpErrorResponse,
  HttpResponse,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, map } from 'rxjs';
import { LoginModel } from '../../models/loginModel.model';
import { environment } from '../../../environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = environment.apiUrl + '/Auth';
  constructor(private http: HttpClient) {}

  login(model: LoginModel): Observable<any> {
    return this.http.post(this.apiUrl + '/Login', model);
  }

  IsLoggedIn() {
    if (typeof localStorage !== 'undefined') {
      const token = localStorage.getItem('token');
      if (token) {
        const decodedToken: any = jwtDecode(token);
        const expirationTime = decodedToken.exp;
        const currentTime = Math.floor(Date.now() / 1000); // Thời điểm hiện tại
        if (currentTime > expirationTime) {
          const token = localStorage.removeItem('token');
          window.location.href = '/login';
        }
        return currentTime <= expirationTime;
      }
    }
    return false;
  }

  logout(): void {
    localStorage.removeItem('token');
    return;
  }

  forgotPassword(email: string): Observable<any> {
    let urlParams = new URLSearchParams();
    urlParams.append('email', email);
    return this.http.post(this.apiUrl + '/ForgotPassword?' + urlParams, email);
  }

  resetPassword(password: string, token: string): Observable<number> {
    return this.http
      .post(
        this.apiUrl + '/ResetPassword',
        { password: password, token: token },
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
  // Check user isAdmin
  isAdmin(): boolean {
    if (typeof localStorage !== 'undefined') {
      const token = localStorage.getItem('token');
      if (token) {
        const decodedToken: any = jwtDecode(token);
        const userRole = decodedToken.role;
        return userRole === 'Admin';
      }
    }
    return false;
  }
  // Get user Id to check CurrentUser
  getUserId(): string | null {
    const token = localStorage.getItem('token');
    if (token) {
      const decodedToken: any = jwtDecode(token);
      return decodedToken.userId;
    }
    return null;
  }
}

import { Injectable } from '@angular/core';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard {
  constructor(private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {

    // Check if localStorage is defined
    if (typeof localStorage !== 'undefined') {
      const localData = localStorage.getItem('token');
      if (localData) {
        const decodedToken: any = jwtDecode(localData);
        const role = decodedToken.role;
        console.log('role')
        if (role == 'Admin') {
          return true;
        }
      }
    }
    // ROle isn't Admin
    this.router.navigateByUrl('/login');
    return false;
  }
}

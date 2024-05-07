import { Component } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { jwtDecode } from 'jwt-decode';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-sidenav',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './admin-sidenav.component.html',
  styleUrl: './admin-sidenav.component.css',
})
export class AdminSidenavComponent {
  hovering = false;

  constructor(
    private authService: AuthService,
    // private http: HttpClient,
    private router: Router
  ) {}
  logout() {
    if (confirm('Are you sure you want to logout?')) {
      this.authService.logout();
      this.router.navigateByUrl('/login').then(() => {
        window.location.reload();
      });
    }
  }
  username: string | undefined;
  role: string | undefined;

  ngOnInit(): void {
    //Get username
    if (typeof localStorage !== 'undefined') {
      const token = localStorage.getItem('token') ?? '';
      if (token) {
        const decodedToken: any = jwtDecode(token);
        this.username = decodedToken.username;
        this.role = decodedToken.role;
      }
    }
  }
}

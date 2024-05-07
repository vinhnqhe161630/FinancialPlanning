import { Component, OnInit } from '@angular/core';
import { SidenavComponent } from '../../share/sidenav/sidenav.component';
import { AdminSidenavComponent } from '../../share/admin-sidenav/admin-sidenav.component';


@Component({
    selector: 'app-layout',
    standalone: true,
    templateUrl: './layout.component.html',
    styleUrls: ['./layout.component.css'],
    imports: [SidenavComponent, AdminSidenavComponent]
})
export class LayoutComponent implements OnInit {
  
    constructor() { }
  
    ngOnInit(): void {
    }

}

// message-bar.component.ts
import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit, inject } from '@angular/core';
import { MatSnackBarRef,} from '@angular/material/snack-bar';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';

@Component({
  selector: 'app-message-bar',
  standalone: true,
  templateUrl: './message-bar.component.html',
  styleUrls: ['./message-bar.component.css'],
  imports: [CommonModule]
})
export class MessageBarComponent implements OnInit {
  constructor(@Inject(MAT_SNACK_BAR_DATA) public data: any) {
    if (!this.data.success) {
      this.snackBarRef.containerInstance.snackBarConfig.panelClass = ['error-snackbar'];
    } else {
      this.snackBarRef.containerInstance.snackBarConfig.panelClass = ['success-snackbar'];
    }
  }
  snackBarRef = inject(MatSnackBarRef);
  ngOnInit() {
  }

  // Helper method to determine icon class based on message type
  getIconClass() {
    if (this.data.success) {
      this.snackBarRef.instance.hostClass = 'success-snackbar';
      return 'fa-regular fa-circle-check';
    } else {
      this.snackBarRef.instance.hostClass = 'success-snackbar';
      return 'fa-regular fa-circle-xmark';
    }
  }
}

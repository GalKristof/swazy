import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router'; // Import router directives

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, // For *ngIf (though might not be needed directly in app.component anymore)
    RouterOutlet, // For displaying routed components
    RouterLink,   // For navigation links
    RouterLinkActive // For styling active links
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  // The activeTab logic is no longer needed as routing handles component display.
  // constructor() {} // Constructor can be removed if empty
}

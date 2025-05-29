import { CommonModule } from '@angular/common'; // Add if not present
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router'; // Add if not present
import { UiTestComponent } from './ui-test/ui-test.component'; // Add this

@Component({
  selector: 'app-root',
  standalone: true, // Ensure true
  imports: [CommonModule, RouterOutlet, UiTestComponent], // Add UiTestComponent here
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'swazy-ui';
}

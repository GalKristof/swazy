import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarModule } from '@syncfusion/ej2-angular-calendars'; // Syncfusion Calendar

@Component({
  selector: 'app-ui-test',
  standalone: true,
  imports: [CommonModule, CalendarModule], // Import CalendarModule here
  templateUrl: './ui-test.component.html',
  styleUrls: ['./ui-test.component.scss']
})
export class UiTestComponent {
  // Add any component logic if needed in the future
}

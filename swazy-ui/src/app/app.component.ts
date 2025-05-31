import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridModule, SortService, FilterService, PageService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ScheduleModule } from '@syncfusion/ej2-angular-schedule';
import { ChartModule, ColumnSeriesService, CategoryService, DataLabelService } from '@syncfusion/ej2-angular-charts';
import { CalendarModule, DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { environment } from '../environments/environment';
import { EventSettingsModel, DayService, WeekService, WorkWeekService, MonthService, AgendaService } from '@syncfusion/ej2-angular-schedule';

@Component({
  selector: 'app-root',
  imports: [CommonModule, GridModule, ButtonModule, ScheduleModule, ChartModule, CalendarModule, DatePickerModule],
  providers: [
    // Schedule services
    DayService, WeekService, WorkWeekService, MonthService, AgendaService,
    // Grid services
    SortService, FilterService, PageService,
    // Chart services
    ColumnSeriesService, CategoryService, DataLabelService
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'swazy-ui';
  environment = environment;

  // Date properties
  currentDate = new Date();
  scheduleDate = new Date(2025, 4, 30);

  // Grid Data
  gridData = [
    { id: 1, name: 'John Doe', email: 'john@example.com', role: 'Admin', salary: 75000, department: 'IT' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com', role: 'Manager', salary: 85000, department: 'HR' },
    { id: 3, name: 'Bob Johnson', email: 'bob@example.com', role: 'Developer', salary: 70000, department: 'IT' },
    { id: 4, name: 'Alice Brown', email: 'alice@example.com', role: 'Designer', salary: 65000, department: 'Design' },
    { id: 5, name: 'Mike Wilson', email: 'mike@example.com', role: 'Developer', salary: 72000, department: 'IT' },
    { id: 6, name: 'Sarah Davis', email: 'sarah@example.com', role: 'Analyst', salary: 68000, department: 'Finance' }
  ];

  pageSettings = { pageSize: 5 };

  // Schedule Data
  scheduleData = [
    {
      Id: 1,
      Subject: 'Team Meeting',
      StartTime: new Date(2025, 4, 30, 9, 0),
      EndTime: new Date(2025, 4, 30, 10, 30),
      CategoryColor: '#1aaa55'
    },
    {
      Id: 2,
      Subject: 'Project Review',
      StartTime: new Date(2025, 4, 31, 14, 0),
      EndTime: new Date(2025, 4, 31, 16, 0),
      CategoryColor: '#357cd2'
    },
    {
      Id: 3,
      Subject: 'Client Presentation',
      StartTime: new Date(2025, 5, 2, 10, 0),
      EndTime: new Date(2025, 5, 2, 12, 0),
      CategoryColor: '#7fa900'
    }
  ];

  eventSettings: EventSettingsModel = { dataSource: this.scheduleData };

  // Chart Data - Fixed structure
  chartData = [
    { department: 'IT', avgSalary: 72000 },
    { department: 'HR', avgSalary: 65000 },
    { department: 'Finance', avgSalary: 70000 },
    { department: 'Design', avgSalary: 68000 },
    { department: 'Marketing', avgSalary: 63000 }
  ];

  // Chart configuration
  primaryXAxis = { valueType: 'Category', title: 'Department' };
  primaryYAxis = { title: 'Average Salary ($)' };
  chartTitle = 'Department-wise Average Salary';
  tooltip = { enable: true };

  ngOnInit() {
    console.log('SwazyUI loaded with environment:', this.environment.production ? 'Production' : 'Development');
  }

  onSyncfusionClick() {
    console.log('Syncfusion button clicked!');
  }

  onEventAdd(args: any) {
    console.log('New event added:', args.data);
  }

  onDateChange(args: any) {
    console.log('Date selected:', args.value);
  }
}

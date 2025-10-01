import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Employee } from '../../models/employee';
import { EmployeeSchedule, DaySchedule } from '../../models/employee-schedule';

@Component({
  selector: 'app-schedule-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './schedule-management.html'
})
export class ScheduleManagementComponent {
  employees = input.required<Employee[]>();
  schedules = input.required<EmployeeSchedule[]>();

  employeeSelected = output<Employee>();
  scheduleSaved = output<{
    employee: Employee;
    currentSchedule: EmployeeSchedule | null;
    bufferTimeMinutes: number;
    isOnVacation: boolean;
    daySchedules: DaySchedule[];
  }>();
  scheduleCopied = output<{
    bufferTimeMinutes: number;
    isOnVacation: boolean;
    daySchedules: DaySchedule[];
  }>();

  selectedEmployee = signal<Employee | null>(null);
  currentSchedule = signal<EmployeeSchedule | null>(null);
  editScheduleForm = signal<DaySchedule[]>([]);
  bufferTimeMinutesValue = 15;
  isOnVacation = false;
  isSaving = false;
  isCopying = false;

  dayNames = ['Vasárnap', 'Hétfő', 'Kedd', 'Szerda', 'Csütörtök', 'Péntek', 'Szombat'];
  dayOrder = [1, 2, 3, 4, 5, 6, 0];

  selectEmployee(employee: Employee) {
    this.selectedEmployee.set(employee);
    this.employeeSelected.emit(employee);
  }

  setScheduleData(schedule: EmployeeSchedule | null) {
    this.currentSchedule.set(schedule);
    if (schedule) {
      this.bufferTimeMinutesValue = schedule.bufferTimeMinutes;
      this.isOnVacation = schedule.isOnVacation;
      this.editScheduleForm.set([...schedule.daySchedules]);
    } else {
      this.bufferTimeMinutesValue = 15;
      this.isOnVacation = false;
      this.editScheduleForm.set(this.createDefaultWeekSchedule());
    }
  }

  hasSchedule(userId: string): boolean {
    return this.schedules().some(s => s.userId === userId);
  }

  isEmployeeOnVacation(userId: string): boolean {
    return this.schedules().find(s => s.userId === userId)?.isOnVacation || false;
  }

  calculateAvailableMinutes(userId: string): number {
    const schedule = this.schedules().find(s => s.userId === userId);
    if (!schedule || schedule.isOnVacation) return 0;

    return schedule.daySchedules
      .filter(day => day.isWorkingDay)
      .reduce((total, day) => {
        const start = this.parseTime(day.startTime);
        const end = this.parseTime(day.endTime);
        return total + (end - start);
      }, 0);
  }

  private parseTime(timeStr: string | null): number {
    if (!timeStr) return 0;
    const [hours, minutes] = timeStr.split(':').map(Number);
    return hours * 60 + minutes;
  }

  getSortedEmployees(): Employee[] {
    const employees = this.employees();
    return [...employees].sort((a, b) => {
      const aOnVacation = this.isEmployeeOnVacation(a.userId);
      const bOnVacation = this.isEmployeeOnVacation(b.userId);

      if (aOnVacation && !bOnVacation) return 1;
      if (!aOnVacation && bOnVacation) return -1;

      const aMinutes = this.calculateAvailableMinutes(a.userId);
      const bMinutes = this.calculateAvailableMinutes(b.userId);
      return bMinutes - aMinutes;
    });
  }

  createDefaultWeekSchedule(): DaySchedule[] {
    return Array.from({ length: 7 }, (_, i) => ({
      dayOfWeek: i,
      isWorkingDay: i >= 1 && i <= 5,
      startTime: '09:00:00',
      endTime: '17:00:00'
    }));
  }

  getOrderedDaySchedules(): DaySchedule[] {
    const schedules = this.editScheduleForm();
    return this.dayOrder.map(dayIndex =>
      schedules.find(s => s.dayOfWeek === dayIndex)!
    ).filter(s => s !== undefined);
  }

  saveSchedule() {
    const employee = this.selectedEmployee();
    if (!employee || this.isSaving) return;

    this.isSaving = true;
    this.scheduleSaved.emit({
      employee,
      currentSchedule: this.currentSchedule(),
      bufferTimeMinutes: this.bufferTimeMinutesValue,
      isOnVacation: this.isOnVacation,
      daySchedules: this.editScheduleForm()
    });
  }

  onSaveComplete() {
    this.isSaving = false;
  }

  onSaveError() {
    this.isSaving = false;
  }

  copyToAllEmployees() {
    if (this.isCopying) return;

    this.isCopying = true;
    this.scheduleCopied.emit({
      bufferTimeMinutes: this.bufferTimeMinutesValue,
      isOnVacation: this.isOnVacation,
      daySchedules: this.editScheduleForm()
    });
  }

  onCopyComplete() {
    this.isCopying = false;
  }

  onCopyError() {
    this.isCopying = false;
  }
}

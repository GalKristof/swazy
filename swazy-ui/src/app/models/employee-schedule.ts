export interface DaySchedule {
  dayOfWeek: number; // 0 = Sunday, 1 = Monday, etc.
  isWorkingDay: boolean;
  startTime: string | null; // TimeSpan as string "HH:mm:ss"
  endTime: string | null; // TimeSpan as string "HH:mm:ss"
}

export interface EmployeeSchedule {
  id: string;
  userId: string;
  businessId: string;
  bufferTimeMinutes: number;
  isOnVacation: boolean;
  daySchedules: DaySchedule[];
  createdAt: string;
}

export interface CreateEmployeeSchedule {
  userId: string;
  businessId: string;
  bufferTimeMinutes: number;
  isOnVacation: boolean;
  daySchedules: DaySchedule[];
}

export interface UpdateEmployeeSchedule {
  id: string;
  bufferTimeMinutes: number;
  isOnVacation: boolean;
  daySchedules: DaySchedule[];
}

export interface AvailableTimeSlots {
  date: string;
  availableSlots: string[];
}

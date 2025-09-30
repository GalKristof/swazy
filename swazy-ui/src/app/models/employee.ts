﻿export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'Owner' | 'Manager' | 'Employee';
}

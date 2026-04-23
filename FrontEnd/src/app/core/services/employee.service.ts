import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EmployeeProfile {
  empId: number;
  empName: string;
  empEmail: string;
  role: string;
  empDOJ?: string;   // NEW FIELD
  EmpDOJ?: string;   // Accept both casing variants if needed
}

export interface VestingSchedule {
  vesting_Id: number;
  awards_AwardId: number;
  type: string;
  awardPrice: number;
  units_Vested: number;
  cumulative_Vested: number;
  remaining_Unvested: number;
  vesting_Date: string;
  final_Vesting_Date: string;
  current_Value: number;
  status: string;
}

export interface EmployeeDashboard {
  totalEquityGranted: number;
  shareValue: number;
}

export interface AdminDashboardEmployee {
  empId: number;
  empName: string;
  esops: number;
  vestingDate: string;
}

export interface AdminDashboard {
  totalEmployees: number;
  totalAwardsGranted: number;
  totalVestedUnits: number;
  pendingRequests: number;
  esopCount: number;
  rsuCount: number;
  esopPercentage: number;
  rsuPercentage: number;
  employees: AdminDashboardEmployee[];
}

export interface AuditLog {
  auditLogId: number;
  action_Type: string;
  entity_Type: string;
  entity_Id: number | null;
  employee_EmpId: number;
  createdDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  private baseUrl = 'https://localhost:7132/api/employee';
  private vestingUrl = 'https://localhost:7132/api/vestingSchedule';
  private auditUrl = 'https://localhost:7132/api/auditlog';

  constructor(private http: HttpClient) {}

  // Admin: Get all employees
  getAllEmployees(): Observable<EmployeeProfile[]> {
    return this.http.get<EmployeeProfile[]>(this.baseUrl);
  }

  // Logged-in user profile
  getMyProfile(): Observable<EmployeeProfile> {
    return this.http.get<EmployeeProfile>(`${this.baseUrl}/profile`);
  }

  // Get my vesting schedules
  getMyVestingSchedules(): Observable<VestingSchedule[]> {
    return this.http.get<VestingSchedule[]>(`${this.vestingUrl}/my-vesting`);
  }

  // Get my dashboard stats
  getMyDashboardStats(): Observable<EmployeeDashboard> {
    return this.http.get<EmployeeDashboard>(`${this.baseUrl}/dashboard`);
  }

  // Admin dashboard stats and employees
  getAdminDashboard(): Observable<AdminDashboard> {
    return this.http.get<AdminDashboard>(`${this.baseUrl}/admin-dashboard`);
  }

  // Get all audit logs
  getAuditLogs(): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(this.auditUrl);
  }

  // Admin: Create new employee
  createEmployee(employeeData: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, employeeData);
  }
}
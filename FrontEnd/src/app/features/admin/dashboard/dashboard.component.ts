import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeService, AdminDashboard, AuditLog } from '@core/services/employee.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  constructor(private employeeService: EmployeeService) {}

  // Stats Data
  stats = {
    totalEmployees: 120,
    totalAwardsGranted: '8500 Units',
    totalVestedUnits: '4200 Units',
    pendingRequests: 18
  };

  // Chart Data
  chartData = {
    esopCount: 0,
    rsuCount: 0,
    esopPercentage: 0,
    rsuPercentage: 0
  };

  // Audit Logs Data
  auditLogs: AuditLog[] = [];

  ngOnInit(): void {
    this.loadAdminDashboard();
    this.loadAuditLogs();
  }

  private loadAdminDashboard(): void {
    this.employeeService.getAdminDashboard().subscribe({
      next: (dashboard: AdminDashboard) => {
        this.stats = {
          totalEmployees: dashboard.totalEmployees,
          totalAwardsGranted: `${dashboard.totalAwardsGranted} Units`,
          totalVestedUnits: `${dashboard.totalVestedUnits} Units`,
          pendingRequests: dashboard.pendingRequests
        };

        this.chartData = {
          esopCount: dashboard.esopCount,
          rsuCount: dashboard.rsuCount,
          esopPercentage: dashboard.esopPercentage,
          rsuPercentage: dashboard.rsuPercentage
        };
      },
      error: err => {
        console.error('Failed to load admin dashboard data:', err);
      }
    });
  }

  private loadAuditLogs(): void {
    console.log('Loading audit logs...');
    this.employeeService.getAuditLogs().subscribe({
      next: (logs: AuditLog[]) => {
        console.log('Audit logs received:', logs);
        this.auditLogs = logs;
      },
      error: err => {
        console.error('Failed to load audit logs:', err);
        this.auditLogs = [];
      }
    });
  }
}


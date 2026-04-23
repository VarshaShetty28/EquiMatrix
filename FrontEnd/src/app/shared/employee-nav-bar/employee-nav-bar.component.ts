import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { EmployeeService, EmployeeProfile } from '../../core/services/employee.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-employee-nav-bar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule
  ],
  templateUrl: './employee-nav-bar.component.html'
})
export class EmployeeNavBarComponent implements OnInit {
  employeeName = 'Loading...';

  constructor(
    private employeeService: EmployeeService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadEmployeeProfile();
  }

  loadEmployeeProfile() {
    this.employeeService.getMyProfile().subscribe({
      next: (profile: EmployeeProfile) => {
        // Use the name from database
        this.employeeName = profile.empName || 'Employee';
      },
      error: (error) => {
        console.error('Failed to load employee profile from database:', error);
        // Fallback to JWT token name
        const tokenName = this.auth.getUserName();
        this.employeeName = tokenName || 'Employee';
      }
    });
  }

  onLogout() {
    this.auth.logout();
    this.router.navigate(['/'], { replaceUrl: true });
  }
}
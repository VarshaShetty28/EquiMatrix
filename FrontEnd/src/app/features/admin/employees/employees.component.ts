import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService, EmployeeProfile } from '../../../core/services/employee.service';

interface EmployeeViewModel {
  empId: number;
  empName: string;
  empEmail: string;
  empDOJ: string;
}

@Component({
  selector: 'app-employee',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employees.component.html'
})
export class EmployeeComponent implements OnInit {

  totalEmployees = 0;
  searchEmployeeName = '';

  employees: EmployeeViewModel[] = [];
  filteredEmployees: EmployeeViewModel[] = [];

  // Create Employee Form
  showCreateForm = false;
  isCreating = false;
  createError = '';
  newEmployee = {
    empName: '',
    empEmail: '',
    empDOJ: '',
    password: '',
    role: 'Employee'
  };

  constructor(private employeeService: EmployeeService) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.employeeService.getAllEmployees().subscribe({
      next: (data: EmployeeProfile[]) => {

        //  ONLY SHOW EMPLOYEES (HIDE ADMINS)
        const onlyEmployees = data.filter(emp => emp.role === 'Employee');

        this.employees = onlyEmployees.map(emp => {
          const rawDoj = emp.empDOJ ?? (emp as any).EmpDOJ;
          const parsedDoj = rawDoj ? new Date(rawDoj) : null;
          const formattedDoj = parsedDoj && !isNaN(parsedDoj.getTime())
            ? parsedDoj.toLocaleDateString('en-GB')
            : 'N/A';

          return {
            empId: emp.empId,
            empName: emp.empName,
            empEmail: emp.empEmail,
            empDOJ: formattedDoj
          };
        });

        this.filteredEmployees = [...this.employees];
        this.totalEmployees = this.employees.length;
      },
      error: () => {
        alert('Failed to load employees');
      }
    });
  }

  onSearchEmployee(): void {
    const value = this.searchEmployeeName.trim().toLowerCase();
    this.filteredEmployees = this.employees.filter(emp =>
      emp.empName.toLowerCase().includes(value)
    );
  }

  openCreateForm(): void {
    this.showCreateForm = true;
    this.createError = '';
    this.newEmployee = {
      empName: '',
      empEmail: '',
      empDOJ: '',
      password: '',
      role: 'Employee'
    };
  }

  closeCreateForm(): void {
    this.showCreateForm = false;
    this.createError = '';
  }

  isFormValid(): boolean {
    return (
      this.newEmployee.empName.trim().length > 0 &&
      this.newEmployee.empEmail.trim().length > 0 &&
      this.newEmployee.empDOJ.trim().length > 0 &&
      this.newEmployee.password.trim().length > 0 &&
      this.isValidEmail(this.newEmployee.empEmail)
    );
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  submitCreateEmployee(): void {
    if (!this.isFormValid()) {
      this.createError = 'Please fill in all fields with valid information';
      return;
    }

    this.isCreating = true;
    this.createError = '';

    const createDto = {
      empName: this.newEmployee.empName.trim(),
      empEmail: this.newEmployee.empEmail.trim(),
      empDOJ: this.newEmployee.empDOJ,
      password: this.newEmployee.password,
      role: 'Employee'
    };

    this.employeeService.createEmployee(createDto).subscribe({
      next: (response) => {
        this.isCreating = false;
        alert('Employee created successfully!');
        this.closeCreateForm();
        this.loadEmployees();
      },
      error: (err) => {
        this.isCreating = false;
        this.createError = err?.error?.message || 'Failed to create employee. Please try again.';
        console.error('Create employee error:', err);
      }
    });
  }
}

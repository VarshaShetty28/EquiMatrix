import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

export interface AwardReport {
  userId: string;
  awardType: 'ESOP' | 'RSU';
  units: number;
  grantDate: string;
  status: 'Active' | 'Exercised';
}

interface AwardResponseDto {
  awardId: number;
  award_Type: string;
  grant_Date: string;
  total_Units: number;
  exercise_Price: number;
  employee_EmpId: number;
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html'
})
export class ReportsComponent {

  userId = '';
  selectedFilter: 'ALL' | 'ESOP' | 'RSU' = 'ALL';

  allReports: AwardReport[] = [];
  filteredReports: AwardReport[] = [];

  hasSearched = false;
  isLoading = false;
  errorMessage = '';

  constructor(private http: HttpClient) {}

  generateReport() {
    this.hasSearched = true;
    this.errorMessage = '';
    this.isLoading = true;

    const searchId = this.userId.trim();
    if (!searchId) {
      this.errorMessage = 'Please enter a valid User ID';
      this.isLoading = false;
      this.allReports = [];
      this.filteredReports = [];
      return;
    }

    const empId = parseInt(searchId, 10);
    if (isNaN(empId)) {
      this.errorMessage = 'User ID must be a valid number';
      this.isLoading = false;
      this.allReports = [];
      this.filteredReports = [];
      return;
    }

    // Call backend API to fetch awards for the employee
    this.http.get<AwardResponseDto[]>(`https://localhost:7132/api/Award/employee/${empId}`)
      .subscribe({
        next: (awards) => {
          this.allReports = awards.map(award => ({
            userId: empId.toString(),
            awardType: this.normalizeAwardType(award.award_Type),
            units: award.total_Units,
            grantDate: new Date(award.grant_Date).toLocaleDateString('en-CA'),
            status: 'Active' as const
          }));
          this.applyFilter();
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Error fetching awards:', err);
          this.errorMessage = err?.error?.message || 'Failed to fetch employee awards. Please check the User ID and try again.';
          this.allReports = [];
          this.filteredReports = [];
          this.isLoading = false;
        }
      });
  }

  private normalizeAwardType(type: string): 'ESOP' | 'RSU' {
    const normalized = type?.toUpperCase() || 'ESOP';
    return normalized === 'RSU' ? 'RSU' : 'ESOP';
  }

  applyFilter() {
    this.filteredReports =
      this.selectedFilter === 'ALL'
        ? this.allReports
        : this.allReports.filter(r => r.awardType === this.selectedFilter);
  }

  downloadReport() {
    const headers = ['Award Type', 'Units', 'Grant Date', 'Status'];

    const rows = this.filteredReports.map(r => [
      r.awardType,
      r.units,
      r.grantDate,
      r.status
    ]);

    const csvContent =
      [headers, ...rows].map(row => row.join(',')).join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = `award-report-${this.userId}.csv`;
    link.click();

    URL.revokeObjectURL(url);
  }
}

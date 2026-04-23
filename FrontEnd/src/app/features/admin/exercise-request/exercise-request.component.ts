import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-exercise-request',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './exercise-request.component.html',
  styleUrl: './exercise-request.component.css'
})
export class ExerciseRequestComponent implements OnInit {
  constructor(private http: HttpClient) {}

  searchText = '';
  isLoading = false;
  errorMessage: string | null = null;

  exerciseRequests: any[] = [];
  filteredRequests: any[] = [];

  // Popup state
  showPopup = false;
  popupAction: 'accept' | 'reject' | null = null;
  selectedRequest: any = null;

  ngOnInit(): void {
    this.loadExerciseRequests();
  }

  loadExerciseRequests(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.http.get<any[]>('https://localhost:7132/api/ExerciseRequest')
      .subscribe({
        next: (data) => {
          console.log('Raw data from backend:', data);
          this.exerciseRequests = data.map(item => ({
            requestId: item.requestId ?? item.RequestId,
            employeeId: item.employee_EmpId ?? item.Employee_EmpId ?? item.employeeId,
            employeeName: item.employeeName ?? item.EmployeeName ?? '',
            shares: item.units_Requested ?? item.Units_Requested ?? item.shares,
            status: (item.status ?? item.Status ?? 'pending')?.toLowerCase(),
            awardId: item.awards_AwardId ?? item.Awards_AwardId ?? item.awardId,
            exerciseAmountUsd: item.exerciseAmountUsd ?? item.ExerciseAmountUsd,
            taxAmountUsd: item.taxAmountUsd ?? item.TaxAmountUsd,
            netAmountUsd: item.netAmountUsd ?? item.NetAmountUsd
          }));
          
          // Fetch missing employee names
          this.fetchMissingEmployeeNames();
          
          console.log('Mapped exercise requests:', this.exerciseRequests);
          this.filteredRequests = [...this.exerciseRequests];
          this.isLoading = false;
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Failed to load exercise requests';
          this.isLoading = false;
          console.error('Error loading exercise requests:', err);
        }
      });
  }

  private fetchMissingEmployeeNames(): void {
    const missingNames = this.exerciseRequests
      .filter(r => !r.employeeName && r.employeeId)
      .map(r => r.employeeId);

    const uniqueIds = [...new Set(missingNames)];

    uniqueIds.forEach(empId => {
      this.http.get<any>(`https://localhost:7132/api/Employee/${empId}`)
        .subscribe({
          next: (employee) => {
            const name = employee.empName ?? employee.EmpName ?? 'Unknown';
            this.exerciseRequests.forEach(r => {
              if (r.employeeId === empId) {
                r.employeeName = name;
              }
            });
            this.filteredRequests = [...this.exerciseRequests];
          },
          error: () => {
            // If fetch fails, leave as is
          }
        });
    });
  }

  // 🔍 Search by Employee ID
  onSearch(): void {
    const term = this.searchText.toLowerCase().trim();

    if (!term) {
      this.filteredRequests = [...this.exerciseRequests];
      return;
    }

    this.filteredRequests = this.exerciseRequests.filter(a =>
      String(a.employeeId).toLowerCase().includes(term)
    );
  }

  // ============================================
  // POPUP LOGIC
  // ============================================
  openPopup(request: any, action: 'accept' | 'reject'): void {
    this.selectedRequest = request;
    this.popupAction = action;
    this.showPopup = true;
  }

  confirmAction(): void {
    if (!this.selectedRequest) return;

    const status = this.popupAction === 'accept' ? 'Approved' : 'Rejected';
    const requestId = this.selectedRequest.requestId;
    const employeeName = this.selectedRequest.employeeName || 'Employee';
    const shares = this.selectedRequest.shares;

    this.http.put(
      `https://localhost:7132/api/ExerciseRequest/${requestId}/status?status=${status}`,
      {}
    )
      .subscribe({
        next: () => {
          // Update local state
          this.selectedRequest.status = status.toLowerCase();
          const actionText = this.popupAction === 'accept' ? 'Approved' : 'Rejected';
          alert(`✓ Exercise request ${actionText}\n\nEmployee: ${employeeName}\nShares: ${shares}\nStatus: ${actionText}`);
          this.closePopup();
          this.loadExerciseRequests(); // Refresh from backend
        },
        error: (err: HttpErrorResponse) => {
          alert('❌ Error: ' + this.getErrorMessage(err));
        }
      });
  }

  private getErrorMessage(err: HttpErrorResponse): string {
    if (err.error?.message) return err.error.message;
    if (typeof err.error === 'string') return err.error;
    if (err.message) return err.message;
    return 'An error occurred';
  }

  closePopup(): void {
    this.showPopup = false;
    this.popupAction = null;
    this.selectedRequest = null;
  }
}
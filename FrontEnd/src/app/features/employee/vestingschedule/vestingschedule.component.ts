import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

interface VestingRecord {
  vestingId: number;
  awardId: number;
  type: string;
  strikePrice: number;
  shares: number;
  cumulativeVested: number;
  remainingUnvested: number;
  vestingDate: string;
  finalVestingDate: string;
  currentValue: number;
  status: string;
}

@Component({
  selector: 'app-vesting-schedule',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vestingschedule.component.html'
})
export class VestingScheduleComponent implements OnInit {

  errorMessage: string | null = null;
  isLoading: boolean = false;
  selectedType: 'ALL' | 'ESOP' | 'RSU' = 'ALL';

  constructor(private http: HttpClient) {}

  vestingData: VestingRecord[] = [];

  ngOnInit(): void {
    this.loadVestingData();
  }

  private loadVestingData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.http.get<any[]>('https://localhost:7132/api/VestingSchedule/my-vesting')
      .subscribe({
        next: (data) => {
          this.vestingData = data.map(item => ({
            vestingId: item.Vesting_Id ?? item.vesting_Id ?? item.vestingId ?? item.vesting_Id ?? 0,
            awardId: item.Awards_AwardId ?? item.awards_AwardId ?? item.awardId ?? 0,
            type: item.Type ?? item.type ?? '',
            strikePrice: item.AwardPrice ?? item.awardPrice ?? item.Award_Price ?? item.strikePrice ?? 0,
            shares: item.Units_Vested ?? item.units_Vested ?? item.unitsVested ?? item.shares ?? 0,
            cumulativeVested: item.Cumulative_Vested ?? item.cumulative_Vested ?? item.cumulativeVested ?? 0,
            remainingUnvested: item.Remaining_Unvested ?? item.remaining_Unvested ?? item.remainingUnvested ?? 0,
            vestingDate: item.Vesting_Date ?? item.vesting_Date ?? item.vestingDate ?? item.VestingDate ?? '',
            finalVestingDate: item.Final_Vesting_Date ?? item.final_Vesting_Date ?? item.finalVestingDate ?? item.FinalVestingDate ?? '',
            currentValue: item.Current_Value ?? item.currentValue ?? 0,
            status: item.Status ?? item.status ?? ''
          }));
          this.isLoading = false;
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = error.error?.message || error.message || 'Could not load vesting data.';
          this.isLoading = false;
        }
      });
  }

  getFilteredData(): VestingRecord[] {
    if (this.selectedType === 'ALL') {
      return this.vestingData;
    }
    return this.vestingData.filter(record => record.type === this.selectedType);
  }
}
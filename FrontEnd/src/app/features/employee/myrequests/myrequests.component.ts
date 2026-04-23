import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { MarketPriceService } from '@core/services/market-price.service';

interface ExercisableShare {
  vestingId: number;
  awardId: number;
  type: 'ESOP' | 'RSU';
  vestedUnits: number;
  strikePrice: number;
  symbol: string;
  vestedDate: string;
}

interface ExerciseRequest {
  requestId: number;
  awards_AwardId: number;
  vestingScheduleId?: number;
  units_Requested: number;
  requested_Date: string;
  status: string;
  employee_EmpId: number;
  exerciseAmountUsd: number;
  taxableGainUsd: number;
  taxAmountUsd: number;
  netAmountUsd: number;
}

interface TaxCalculationResponseDto {
  country: string;
  financialYear: string;
  taxRegime: string;
  category: string;
  referenceFxRate: number;
  annualIncomeUsd: number;
  totalTaxUsd: number;
  totalCessUsd: number;
  rebateUsd: number;
  netTaxUsd: number;
  netIncomeUsd: number;
  effectiveTaxRate: number;
  slabBreakdown: any[];
}

@Component({
  selector: 'app-my-requests',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './myrequests.component.html'
})
export class MyRequestsComponent implements OnInit {

  // Tab control
  activeTab: 'exercise' | 'requests' = 'exercise';

  // Data
  exercisableShares: ExercisableShare[] = [];
  exerciseRequests: ExerciseRequest[] = [];

  // Exercise form
  showExerciseForm = false;
  selectedAward: ExercisableShare | null = null;
  unitsToExercise = 0;
  exerciseAmount = 0;
  taxableGain = 0;
  taxAmount = 0;
  payableAmount = 0;
  netAmount = 0;
  taxDetails: TaxCalculationResponseDto | null = null;
  showTaxPopup = false;
  vestingError: string | null = null;

  // New properties for enhanced form
  companySymbol = 'IBM';
  marketPrice: number | null = null;
  marketPriceLoading = false;
  marketPriceError = '';
  sharesToExercise: number | null = null;
  currentSharePrice: number | null = null;
  totalCost = 0;
  totalPayable = 0;
  marginalTaxRate = 0;

  constructor(private http: HttpClient, private marketPriceService: MarketPriceService) {}

  ngOnInit() {
    this.loadExercisableShares();
    this.loadExerciseRequests();
    this.seedTaxData();
  }

  loadExercisableShares() {
    this.vestingError = null;
    this.loadVestings('my-vested', true);
  }

  private loadVestings(endpoint: 'my-vested' | 'my-vesting', allowFallback = false) {
    this.http.get<any[]>(`https://localhost:7132/api/VestingSchedule/${endpoint}`)
      .subscribe({
        next: (data) => {
          console.log(`vesting payload (${endpoint})`, data);
          this.exercisableShares = (data || [])
            .map(item => ({
              vestingId: item.id ?? item.Id ?? item.vestingScheduleId ?? item.VestingScheduleId ?? item.vesting_Id ?? 0,
              awardId: item.awards_AwardId ?? item.Awards_AwardId ?? item.awardId ?? item.AwardId,
              type: item.type ?? item.Type ?? 'ESOP',
              vestedUnits: item.units_Vested ?? item.Units_Vested ?? item.unitsVested ?? item.UnitsVested ?? 0,
              strikePrice: item.awardPrice ?? item.AwardPrice ?? item.awardPrice ?? item.AwardPrice ?? 0,
              symbol: item.symbol ?? item.Symbol ?? `AWD-${item.awards_AwardId ?? item.Awards_AwardId ?? item.awardId ?? item.AwardId}`,
              vestedDate: item.vesting_Date ?? item.Vesting_Date ?? item.vestedDate ?? item.VestedDate
            }))
            .filter(item => item.awardId != null && item.vestedUnits > 0 && item.type === 'ESOP');
        },
        error: (err: HttpErrorResponse) => {
          console.error(`Error loading exercisable shares (${endpoint})`, err);
          if (allowFallback && endpoint === 'my-vested') {
            this.loadVestings('my-vesting', false);
          } else {
            this.vestingError = err.error?.title || err.error?.message || err.message || 'Unable to load vested shares. Please refresh or check your network connection.';
            this.exercisableShares = [];
          }
        }
      });
  }

  loadExerciseRequests() {
    this.http.get<ExerciseRequest[]>('https://localhost:7132/api/ExerciseRequest/my-requests')
      .subscribe({
        next: (data) => this.exerciseRequests = data,
        error: (err) => console.error('Error loading requests', err)
      });
  }

  seedTaxData() {
    this.http.post('https://localhost:7132/api/Tax/seed', {})
      .subscribe({
        next: () => console.log('Tax data seeded'),
        error: (err) => console.log('Tax data already exists or error:', err)
      });
  }

  // Actions
  switchTab(tab: 'exercise' | 'requests') {
    this.activeTab = tab;
  }

  getRemainingUnits(award: ExercisableShare): number {
    // Get total exercised ONLY for THIS specific vesting schedule
    const totalExercised = this.exerciseRequests
      .filter(req => 
        req.vestingScheduleId === award.vestingId && 
        req.status !== 'Rejected'
      )
      .reduce((sum, req) => sum + req.units_Requested, 0);
    
    return Math.max(0, award.vestedUnits - totalExercised);
  }

  exerciseShares(award: ExercisableShare) {
    this.selectedAward = award;
    this.showExerciseForm = true;
    this.unitsToExercise = 0;
    this.sharesToExercise = null;
    this.companySymbol = award.symbol && !award.symbol.startsWith('AWD-') ? award.symbol : 'IBM';
    this.marketPrice = null;
    this.marketPriceError = '';
    this.resetCalculations();
    this.onSymbolChange();
  }

  closeExerciseForm() {
    this.showExerciseForm = false;
    this.selectedAward = null;
    this.resetCalculations();
  }

  resetCalculations() {
    this.exerciseAmount = 0;
    this.taxableGain = 0;
    this.taxAmount = 0;
    this.payableAmount = 0;
    this.netAmount = 0;
    this.totalCost = 0;
    this.totalPayable = 0;
    this.marginalTaxRate = 0;
    this.taxDetails = null;
  }

  calculateTax() {
    if (!this.selectedAward || this.unitsToExercise <= 0 || !this.currentSharePrice || this.currentSharePrice <= 0) {
      this.resetCalculations();
      return;
    }

    this.exerciseAmount = this.currentSharePrice * this.unitsToExercise;
    this.taxableGain = Math.max(0, this.currentSharePrice - this.selectedAward.strikePrice) * this.unitsToExercise;

    // Call tax API
    const taxRequest = {
      country: 'US',
      financialYear: '2024',
      taxRegime: 'Federal',
      category: this.selectedAward.type,
      annualIncomeUsd: this.taxableGain
    };

    this.http.post<TaxCalculationResponseDto>('https://localhost:7132/api/Tax/calculate', taxRequest)
      .subscribe({
        next: (result) => {
          this.taxDetails = result;
          this.marginalTaxRate = result.effectiveTaxRate / 100;
          this.updateCosts();
          this.taxAmount = result.netTaxUsd;
          this.payableAmount = this.exerciseAmount + this.taxAmount;
          this.netAmount = this.exerciseAmount - this.taxAmount;
        },
        error: (err) => {
          console.error('Tax calculation error', err);
          this.resetCalculations();
        }
      });
  }

  submitExerciseRequest() {
    if (!this.selectedAward || this.unitsToExercise <= 0 || this.currentSharePrice == null || this.currentSharePrice <= 0) return;

    const request = {
      awards_AwardId: this.selectedAward.awardId,
      vestingScheduleId: this.selectedAward.vestingId,
      units_Requested: this.unitsToExercise,
      currentSharePrice: this.currentSharePrice
    };

    this.http.post('https://localhost:7132/api/ExerciseRequest', request)
      .subscribe({
        next: (response: any) => {
          alert('Exercise request submitted successfully!');
          this.closeExerciseForm();
          this.loadExerciseRequests();
          this.loadExercisableShares();
        },
        error: (err) => {
          console.error('Error submitting request', err);
          alert('Error submitting request');
        }
      });
  }

  // New methods for enhanced form
  onSymbolChange() {
    const symbol = this.companySymbol?.trim().toUpperCase();
    if (!symbol) {
      this.marketPriceError = 'Please enter a valid stock symbol.';
      return;
    }

    this.marketPriceLoading = true;
    this.marketPriceError = '';

    this.marketPriceService.getPrice(symbol).subscribe({
      next: (price) => {
        this.marketPrice = price.adjustedClosePrice;
        this.currentSharePrice = price.adjustedClosePrice;
        this.marketPriceLoading = false;
        this.calculateTax();
      },
      error: (err) => {
        console.error('Market price fetch failed:', err);
        this.marketPriceError = 'Unable to fetch market price. Please try again.';
        this.marketPriceLoading = false;
      }
    });
  }

  onSharesChange() {
    if (!this.sharesToExercise) {
      this.unitsToExercise = 0;
      this.resetCalculations();
      return;
    }
    this.unitsToExercise = this.sharesToExercise;
    this.calculateTax();
  }

  updateCosts() {
    if (this.selectedAward) {
      this.totalCost = (this.sharesToExercise || 0) * this.selectedAward.strikePrice;
      this.totalPayable = this.totalCost + this.taxAmount;
    }
  }

  openDetailDialog() {
    this.showTaxPopup = true;
  }

  onCancel() {
    this.closeExerciseForm();
  }

  onSubmit() {
    this.submitExerciseRequest();
  }

  isValid(): boolean {
    if (!this.sharesToExercise || this.sharesToExercise <= 0 || !this.selectedAward) {
      return false;
    }
    if (this.marketPrice === null || this.marketPrice <= 0) {
      return false;
    }
    return this.sharesToExercise <= this.getRemainingUnits(this.selectedAward);
  }

  openTaxPopup() {
    this.showTaxPopup = true;
  }

  closeTaxPopup() {
    this.showTaxPopup = false;
  }
}

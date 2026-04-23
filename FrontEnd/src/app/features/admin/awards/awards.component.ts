import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { MarketPriceService, MarketPrice } from '@core/services/market-price.service';

@Component({
  selector: 'app-awards',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './awards.component.html'
})
export class AwardsComponent implements OnInit {

  constructor(private http: HttpClient, private marketPriceService: MarketPriceService) {}

  searchText = '';
  isLoading = false;
  errorMessage: string | null = null;
  showCreatePopup = false;
  showEditPopup = false;

  awards: any[] = [];
  filteredAwards: any[] = [];

  createGrant = {
    employeeId: '',
    grantType: 'ESOP',
    numberOfShares: 0,
    strikePrice: 0,
    symbol: 'IBM',
    grantDate: ''
  };

  editGrant = {
    awardId: 0,
    employeeId: '',
    grantType: 'ESOP',
    numberOfShares: 0,
    strikePrice: 0,
    symbol: 'IBM',
    grantDate: ''
  };

  ngOnInit(): void {
    this.loadAwards();
  }

  loadAwards(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.http.get<any[]>('https://localhost:7132/api/Award')
      .subscribe({
        next: (data) => {
          this.awards = data.map(item => ({
            awardId: item.awardId ?? item.AwardId ?? 0,
            employeeId: item.employee_EmpId ?? item.Employee_EmpId ?? item.employeeId ?? 0,
            grantType: item.award_Type ?? item.Award_Type ?? item.grantType ?? '',
            numberOfShares: item.total_Units ?? item.Total_Units ?? item.numberOfShares ?? 0,
            strikePrice: item.exercise_Price ?? item.Exercise_Price ?? item.strikePrice ?? 0,
            symbol: item.symbol ?? 'IBM',
            grantDate: item.grant_Date ?? item.Grant_Date ?? item.grantDate ?? '',
            marketPrice: null,
            marketPriceLoading: false
          }));

          // Load market prices for awards with symbols
          this.loadMarketPrices();

          this.filteredAwards = [...this.awards];
          this.isLoading = false;
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Failed to load awards';
          this.isLoading = false;
        }
      });
  }

  loadMarketPrices(): void {
    const symbols = ['IBM']; // Use IBM for all awards
    if (symbols.length === 0) return;

    this.marketPriceService.getPricesBatch(symbols).then((priceMap: { [symbol: string]: number }) => {
      this.awards.forEach(award => {
        if (priceMap['IBM']) {
          award.marketPrice = priceMap['IBM'];
          award.marketPriceLoading = false;
        }
      });
      this.filteredAwards = [...this.awards];
    }).catch((err: any) => {
      console.error('Failed to load market prices:', err);
    });
  }

  onSearch(): void {
    const term = this.searchText.toLowerCase().trim();
    if (!term) {
      this.filteredAwards = [...this.awards];
      return;
    }
    this.filteredAwards = this.awards.filter(a =>
      String(a.employeeId).toLowerCase().includes(term)
    );
  }

  openCreatePopup(): void {
    this.createGrant = {
      employeeId: '',
      grantType: 'ESOP',
      numberOfShares: 0,
      strikePrice: 0,
      symbol: 'IBM',
      grantDate: ''
    };
    this.showCreatePopup = true;
    document.body.classList.add('overflow-hidden');
  }

  closeCreatePopup(): void {
    this.showCreatePopup = false;
    document.body.classList.remove('overflow-hidden');
  }

  submitCreateAward(): void {
    if (!this.createGrant.employeeId || !this.createGrant.grantDate || this.createGrant.numberOfShares <= 0) {
      alert('Please fill in all required fields');
      return;
    }

    const payload = {
      Employee_EmpId: parseInt(this.createGrant.employeeId),
      Award_Type: this.createGrant.grantType,
      Total_Units: this.createGrant.numberOfShares,
      Exercise_Price: this.createGrant.strikePrice,
      Symbol: this.createGrant.symbol,
      Grant_Date: new Date(this.createGrant.grantDate).toISOString().split('T')[0]
    };

    this.http.post('https://localhost:7132/api/Award', payload)
      .subscribe({
        next: () => {
          alert('Award created successfully');
          this.closeCreatePopup();
          this.loadAwards();
        },
        error: (err: HttpErrorResponse) => {
          console.error('Award creation error:', err);
          alert('Error: ' + this.getErrorMessage(err));
        }
      });
  }

  openEditPopup(award: any): void {
    this.editGrant = {
      awardId: award.awardId,
      employeeId: award.employeeId,
      grantType: award.grantType,
      numberOfShares: award.numberOfShares,
      strikePrice: award.strikePrice,
      symbol: award.symbol || 'IBM',
      grantDate: award.grantDate
    };
    this.showEditPopup = true;
    document.body.classList.add('overflow-hidden');
  }

  closeEditPopup(): void {
    this.showEditPopup = false;
    document.body.classList.remove('overflow-hidden');
  }

  submitEditAward(): void {
    if (!this.editGrant.employeeId || !this.editGrant.grantDate || this.editGrant.numberOfShares <= 0) {
      alert('Please fill in all required fields');
      return;
    }

    const payload: any = {
      Total_Units: this.editGrant.numberOfShares,
      Exercise_Price: this.editGrant.strikePrice,
      Symbol: this.editGrant.symbol
    };

    this.http.put(`https://localhost:7132/api/Award/${this.editGrant.awardId}`, payload)
      .subscribe({
        next: () => {
          alert('Award updated successfully');
          this.closeEditPopup();
          this.loadAwards();
        },
        error: (err: HttpErrorResponse) => {
          console.error('Award update error:', err);
          alert('Error: ' + this.getErrorMessage(err));
        }
      });
  }

  deleteAward(awardId: number): void {
    if (!confirm('Delete this award?')) return;
    this.http.delete(`https://localhost:7132/api/Award/${awardId}`)
      .subscribe({
        next: () => {
          alert('Award deleted successfully');
          this.loadAwards();
        },
        error: (err: HttpErrorResponse) => {
          console.error('Award delete error:', err);
          alert('Error: ' + this.getErrorMessage(err));
        }
      });
  }

  private getErrorMessage(err: HttpErrorResponse): string {
    if (err.error?.message) return err.error.message;
    if (err.error?.title) return err.error.title;
    if (typeof err.error === 'string') return err.error;
    if (err.message) return err.message;
    return 'An error occurred';
  }
}
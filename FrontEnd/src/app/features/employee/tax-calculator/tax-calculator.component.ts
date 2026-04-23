import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MarketPriceService } from '@core/services/market-price.service';

interface TaxCalculatorResult {
  totalTax: number;
  slabBreakdown: Array<{ rate: number; taxableIncomeUsd: number; taxAmountUsd: number }>;
}

@Component({
  selector: 'app-tax-calculator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tax-calculator.component.html'
})
export class TaxCalculatorComponent implements OnInit {
  calculation = {
    taxType: 'exercise' as 'exercise' | 'selling',
    awardType: 'esop' as 'esop' | 'rsu',
    stockSymbol: 'IBM',
    holdingPeriodMonths: 0,
    marketPrice: null as number | null,
    shares: 0,
    exercisePrice: 0,
    salePrice: 0
  };

  result: TaxCalculatorResult | null = null;
  loading = false;
  marketLoading = false;
  errorMessage = '';
  marketPriceError = '';

  constructor(private marketPriceService: MarketPriceService) {}

  ngOnInit(): void {
    this.fetchMarketPrice();
  }

  onTaxTypeChange(): void {
    this.resetCalculation();
  }

  onAwardTypeChange(): void {
    if (this.calculation.awardType === 'rsu') {
      this.calculation.exercisePrice = 0;
    }
    this.resetCalculation();
  }

  fetchMarketPrice(): void {
    const symbol = this.calculation.stockSymbol?.trim().toUpperCase();
    if (!symbol) {
      this.marketPriceError = 'Please enter a stock symbol.';
      return;
    }

    this.marketLoading = true;
    this.marketPriceError = '';
    this.result = null;

    this.marketPriceService.getPrice(symbol).subscribe({
      next: (price) => {
        this.calculation.marketPrice = price.adjustedClosePrice;
        this.marketLoading = false;
      },
      error: (err) => {
        console.error('Market price fetch failed:', err);
        this.marketPriceError = err?.error?.error || 'Unable to fetch market price. Please try again.';
        this.marketLoading = false;
      }
    });
  }

  calculateTax(): void {
    this.errorMessage = '';
    if (this.calculation.marketPrice == null || this.calculation.shares <= 0) {
      this.errorMessage = 'Please enter market price and number of shares.';
      return;
    }

    this.loading = true;
    const baseAmount = this.calculation.taxType === 'exercise'
      ? this.getPerquisiteValue()
      : this.getCapitalGainAmount();

    const totalTax = this.getTotalTax();

    this.result = {
      totalTax,
      slabBreakdown: [
        {
          rate: this.getTaxRate(),
          taxableIncomeUsd: baseAmount,
          taxAmountUsd: totalTax
        }
      ]
    };

    this.loading = false;
  }

  resetCalculation(): void {
    this.result = null;
    this.loading = false;
    this.errorMessage = '';
    this.marketPriceError = '';
  }

  getPerquisiteValue(): number {
    if (this.calculation.marketPrice == null) {
      return 0;
    }
    const price = this.calculation.marketPrice;
    const exercisePrice = this.calculation.awardType === 'rsu' ? 0 : this.calculation.exercisePrice;
    return this.calculation.shares * Math.max(0, price - exercisePrice);
  }

  getCapitalGainAmount(): number {
    if (this.calculation.marketPrice == null) {
      return 0;
    }
    return this.calculation.shares * Math.max(0, this.calculation.salePrice - this.calculation.marketPrice);
  }

  getTaxRate(): number {
    if (this.calculation.taxType === 'selling') {
      return this.calculation.holdingPeriodMonths >= 12 ? 0.10 : 0.20;
    }
    return 0.25;
  }

  getTotalTax(): number {
    const taxable = this.calculation.taxType === 'exercise'
      ? this.getPerquisiteValue()
      : this.getCapitalGainAmount();
    return taxable * this.getTaxRate();
  }

  getNetProceeds(): number {
    if (this.calculation.taxType === 'exercise') {
      return (this.calculation.marketPrice ?? 0) * this.calculation.shares - this.getTotalTax();
    }
    return this.calculation.salePrice * this.calculation.shares - this.getTotalTax();
  }

  getHoldingClassification(): string {
    if (this.calculation.taxType !== 'selling') {
      return '';
    }
    return this.calculation.holdingPeriodMonths >= 12 ? 'Long-Term Capital Gain' : 'Short-Term Capital Gain';
  }

  getTaxLabel(): string {
    return this.calculation.taxType === 'exercise'
      ? 'Exercise Tax Estimate'
      : 'Selling Tax Estimate';
  }
}

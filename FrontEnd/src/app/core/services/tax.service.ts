import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TaxSlabBreakdown {
  lowerBoundUsd: number;
  upperBoundUsd?: number | null;
  rate: number;
  taxableIncomeUsd: number;
  taxAmountUsd: number;
}

export interface TaxCalculationResponse {
  country: string;
  financialYear: string;
  taxRegime: string;
  category: string;
  annualIncomeUsd: number;
  totalTaxUsd: number;
  totalCessUsd: number;
  rebateUsd: number;
  netTaxUsd: number;
  netIncomeUsd: number;
  effectiveTaxRate: number;
  slabBreakdown: TaxSlabBreakdown[];
}

@Injectable({
  providedIn: 'root'
})
export class TaxService {
  private readonly baseUrl = 'https://localhost:7132/api/tax';

  constructor(private http: HttpClient) {}

  calculateTax(annualIncomeUsd: number): Observable<TaxCalculationResponse> {
    const payload = {
      country: 'US',
      financialYear: '2026',
      taxRegime: 'Federal',
      category: 'Single',
      annualIncomeUsd: annualIncomeUsd
    };

    return this.http.post<TaxCalculationResponse>(`${this.baseUrl}/calculate`, payload);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError, shareReplay } from 'rxjs/operators';

export interface MarketPrice {
  symbol: string;
  date: string;
  adjustedClosePrice: number;
}

export interface PriceHistoryEntry {
  date: string;
  price: number;
}

@Injectable({
  providedIn: 'root'
})
export class MarketPriceService {
  private readonly baseUrl = 'https://localhost:7132/api/tax';
  private cache = new Map<string, { price: MarketPrice; timestamp: number }>();
  private readonly CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

  constructor(private http: HttpClient) {}

  getPrice(symbol: string, date?: string): Observable<MarketPrice> {
    const cacheKey = `${symbol}_${date || 'latest'}`;
    const cached = this.cache.get(cacheKey);

    if (cached && (Date.now() - cached.timestamp) < this.CACHE_DURATION) {
      return of(cached.price);
    }

    const params: any = { symbol };
    if (date) {
      params.date = date;
    }

    return this.http.get<MarketPrice>(`${this.baseUrl}/market-price`, { params }).pipe(
      map(response => {
        this.cache.set(cacheKey, { price: response, timestamp: Date.now() });
        return response;
      }),
      catchError(error => {
        console.error('Error fetching market price:', error);
        return throwError(() => error);
      }),
      shareReplay(1)
    );
  }

  getPriceHistory(symbol: string, days: number = 30): Observable<PriceHistoryEntry[]> {
    const params = { symbol, days: days.toString() };

    return this.http.get<{ symbol: string; days: number; data: { [date: string]: number } }>(
      `${this.baseUrl}/market-price-history`,
      { params }
    ).pipe(
      map(response => {
        return Object.entries(response.data).map(([date, price]) => ({
          date,
          price
        }));
      }),
      catchError(error => {
        console.error('Error fetching price history:', error);
        return throwError(() => error);
      })
    );
  }

  getPricesBatch(symbols: string[]): Promise<{ [symbol: string]: number }> {
    const promises = symbols.map(symbol =>
      this.getPrice(symbol).toPromise().then(
        price => ({ symbol, price: price!.adjustedClosePrice }),
        () => ({ symbol, price: null })
      )
    );

    return Promise.all(promises).then(results => {
      const priceMap: { [symbol: string]: number } = {};
      results.forEach(result => {
        if (result.price !== null) {
          priceMap[result.symbol] = result.price;
        }
      });
      return priceMap;
    });
  }

  calculatePriceChange(oldPrice: number, newPrice: number): number {
    if (oldPrice === 0) return 0;
    return ((newPrice - oldPrice) / oldPrice) * 100;
  }

  clearCache(): void {
    this.cache.clear();
  }
}
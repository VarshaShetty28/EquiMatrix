import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { EmployeeService, VestingSchedule, EmployeeDashboard } from '@core/services/employee.service';
import { MarketPriceService } from '@core/services/market-price.service';

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './edashboard.component.html'
})
export class EmployeeDashboardComponent implements OnInit {

  overviewStats = {
    equityAwardsValue: 0,
    sharesValue: 0,
    currentShareholdings: 0,
    pendingExerciseRequests: 0
  };

  currentMarketPrice: number | null = null;
  marketPriceLoading = false;
  marketPriceError: string | null = null;

  // Trend chart variables
  priceHistory: number[] = [];
  sparklinePoints: string = '';
  sparklineData: Array<{ date: Date; price: number; x: number; y: number }> = [];
  priceChangePercent: number | null = null;

  tooltipVisible = false;
  tooltipDate = '';
  tooltipPrice: number | null = null;
  tooltipX = 0;
  tooltipY = 0;

  esopData = {
    unvested: 0,
    vested: 0,
    hasGrants: false
  };

  rsuData = {
    unvested: 0,
    vested: 0,
    hasGrants: false
  };

  constructor(
    private employeeService: EmployeeService,
    private marketPriceService: MarketPriceService
  ) {}

  ngOnInit(): void {
    this.loadDashboardStats();
    this.loadVestingData();
    this.loadCurrentMarketPrice();
    this.loadPriceHistory();  // trend graph loader
  }

  /* =========================================================================
     LOAD DASHBOARD STATS
  ========================================================================= */

  loadDashboardStats(): void {
    this.employeeService.getMyDashboardStats().subscribe({
      next: (stats: EmployeeDashboard) => {
        console.log('Dashboard stats received:', stats);
        this.overviewStats.equityAwardsValue = stats.totalEquityGranted;
        this.overviewStats.sharesValue = stats.shareValue;
      },
      error: (err: any) => {
        console.error('Failed to load dashboard stats:', err);
      }
    });
  }

  /* =========================================================================
     LOAD STOCK PRICE
  ========================================================================= */

  loadCurrentMarketPrice(): void {
    this.marketPriceLoading = true;
    this.marketPriceError = null;

    this.marketPriceService.getPrice('IBM').subscribe({
      next: (price: any) => {
        this.currentMarketPrice = price.adjustedClosePrice;
        this.marketPriceLoading = false;
      },
      error: (err: any) => {
        console.error('Failed to load market price:', err);
        this.marketPriceError = 'Failed to load price';
        this.marketPriceLoading = false;
      }
    });
  }

  /* =========================================================================
     LOAD REAL PRICE HISTORY FOR SPARKLINE
  ========================================================================= */

  loadPriceHistory(): void {
    this.marketPriceService.getPriceHistory('IBM', 15).subscribe({
      next: (history: any) => {

        const entries: Array<{ date: Date; price: number }> = history
          .map((entry: any) => ({ date: new Date(entry.date), price: entry.price }))
          .filter((entry: any) => !isNaN(entry.price));

        if (!entries || entries.length < 7) {
          console.warn('No usable price history for graph');
          this.sparklinePoints = '';
          return;
        }

        // Sort by date ascending (oldest to newest)
        entries.sort((a: any, b: any) => a.date.getTime() - b.date.getTime());

        const today = new Date();
        const todayMidnight = new Date(today.getFullYear(), today.getMonth(), today.getDate()).getTime();
        const filteredEntries = entries.filter(entry => entry.date.getTime() !== todayMidnight);
        // Always take exactly last 7 trading days (after filtering today's data)
        const latestEntries = filteredEntries.slice(-7);

        const prices = latestEntries.map(entry => entry.price);

        this.priceHistory = prices;

        const first = prices[0];
        const last = prices[prices.length - 1];

        this.priceChangePercent = ((last - first) / first) * 100;
        this.sparklinePoints = this.generateSparklinePoints(latestEntries);
      },

      error: (err) => {
        console.error('Failed to load price history:', err);
        this.sparklinePoints = '';
      }
    });
  }

  generateSparklinePoints(entries: Array<{ date: Date; price: number }>): string {
    const prices = entries.map(entry => entry.price);
    const max = Math.max(...prices);
    const min = Math.min(...prices);
    const range = max - min || 1;

    const points = entries.map((entry, i) => {
      const x = (i / (entries.length - 1)) * 200;
      const y = 40 - ((entry.price - min) / range) * 40;
      return { date: entry.date, price: entry.price, x, y };
    });

    this.sparklineData = points;
    return points.map(point => `${point.x},${point.y}`).join(' ');
  }

  showTooltip(point: { date: Date; price: number; x: number; y: number }, event: MouseEvent): void {
    const target = event.currentTarget as HTMLElement;
    const svg = target.closest('svg');

    if (svg) {
      const rect = svg.getBoundingClientRect();
      this.tooltipX = event.clientX - rect.left;
      this.tooltipY = event.clientY - rect.top;
    }

    this.tooltipPrice = point.price;
    this.tooltipDate = point.date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    this.tooltipVisible = true;
  }

  hideTooltip(): void {
    this.tooltipVisible = false;
  }

  /* =========================================================================
     VESTING CALCULATION
  ========================================================================= */

  loadVestingData(): void {
    this.employeeService.getMyVestingSchedules().subscribe({
      next: (data: VestingSchedule[]) => {
        this.calculateVestingData(data);
      },
      error: (err: any) => {
        console.error('Failed to load vesting data:', err);
      }
    });
  }

  calculateVestingData(vestings: VestingSchedule[]): void {
    const awardMap = new Map<number, { vested: number; total: number; type: string }>();

    vestings.forEach(vesting => {
      const awardId = vesting.awards_AwardId;
      const existing = awardMap.get(awardId);
      const vestedUnits = vesting.status === 'Vested' ? vesting.units_Vested : 0;

      if (!existing) {
        awardMap.set(awardId, {
          vested: vestedUnits,
          total: vesting.units_Vested,
          type: vesting.type
        });
      } else {
        existing.vested += vestedUnits;
        existing.total += vesting.units_Vested;
      }
    });

    let esopVested = 0, esopTotal = 0;
    let rsuVested = 0, rsuTotal = 0;

    awardMap.forEach(award => {
      if (award.type === 'ESOP') {
        esopVested += award.vested;
        esopTotal += award.total;
      } else if (award.type === 'RSU') {
        rsuVested += award.vested;
        rsuTotal += award.total;
      }
    });

    this.esopData.hasGrants = esopTotal > 0;
    this.esopData.vested = esopTotal ? Math.round((esopVested / esopTotal) * 100) : 0;
    this.esopData.unvested = esopTotal ? 100 - this.esopData.vested : 0;

    this.rsuData.hasGrants = rsuTotal > 0;
    this.rsuData.vested = rsuTotal ? Math.round((rsuVested / rsuTotal) * 100) : 0;
    this.rsuData.unvested = rsuTotal ? 100 - this.rsuData.vested : 0;
  }
}
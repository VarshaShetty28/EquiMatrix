# Market Price Feature - Implementation & Testing Guide

## Overview
The market price feature integrates real-time stock price data from the Alpha Vantage API to provide:
- Current market prices for equity awards
- Historical price data
- Price comparison calculations
- Tax calculations based on current market prices

## Architecture

### Backend Components

#### 1. **MarketPriceService** (`BackEnd/Services/MarketPriceService.cs`)
```csharp
public class MarketPriceService
{
    // Gets the adjusted close price for a specific date or latest
    public async Task<decimal?> GetAdjustedClosePriceAsync(string symbol, DateTime? date = null)
    
    // Gets historical price data (up to 365 days)
    public async Task<Dictionary<string, decimal>> GetPriceHistoryAsync(string symbol, int days = 30)
}
```

**Key Features:**
- Alpha Vantage API Integration
- Asynchronous HTTP requests
- Error handling and null checks
- Support for historical date queries

#### 2. **TaxController Endpoints** (`BackEnd/Controllers/TaxController.cs`)

**Get Current Market Price:**
```
GET /api/tax/market-price?symbol=IBM&date=2024-04-19
Response:
{
    "symbol": "IBM",
    "date": "latest",
    "adjustedClosePrice": 175.50
}
```

**Get Price History:**
```
GET /api/tax/market-price-history?symbol=IBM&days=30
Response:
{
    "symbol": "IBM",
    "days": 30,
    "data": {
        "2024-04-19": 175.50,
        "2024-04-18": 174.20,
        ...
    }
}
```

### Frontend Components

#### 1. **MarketPriceService** (`FrontEnd/core/services/market-price.service.ts`)
```typescript
@Injectable({ providedIn: 'root' })
export class MarketPriceService {
    // Get current price with 5-minute caching
    getPrice(symbol: string, date?: string): Observable<MarketPrice>
    
    // Get historical prices
    getPriceHistory(symbol: string, days?: number): Observable<PriceHistoryEntry[]>
    
    // Get multiple prices at once
    getPricesBatch(symbols: string[]): Observable<{ [symbol: string]: number }>
    
    // Calculate percentage change
    calculatePriceChange(oldPrice: number, newPrice: number): number
    
    // Clear price cache
    clearCache(): void
}
```

**Key Features:**
- Built-in caching (5 minutes)
- Batch operations
- Error handling with fallbacks
- Price change calculations

#### 2. **TaxService Enhancement** (`FrontEnd/core/services/tax.service.ts`)
- Added `getMarketPriceHistory()` method
- New `MarketPriceHistory` interface

#### 3. **Component Integrations**

**Exercise Modal** (`edashboard/exercise-modal/`)
- Symbol input/selection field
- Refresh button for price updates
- Real-time market price display
- Fallback to strike price if API fails

**Awards List** (`admin/awards/awards.component.ts`)
- Market price column in table
- Company symbol display
- Symbol input in edit modal
- Batch price loading

**Employee Dashboard** (`employee/edashboard/`)
- Current market price widget
- Loading and error states
- Automatic price refresh

## Testing Checklist

### 1. Backend API Tests

#### Test Current Market Price Endpoint
```bash
# Valid symbol
curl "https://localhost:7132/api/tax/market-price?symbol=IBM"

# With specific date
curl "https://localhost:7132/api/tax/market-price?symbol=IBM&date=2024-04-19"

# Invalid date format (should return 400)
curl "https://localhost:7132/api/tax/market-price?symbol=IBM&date=invalid"

# Invalid symbol (should return 404)
curl "https://localhost:7132/api/tax/market-price?symbol=INVALID123"
```

#### Test Price History Endpoint
```bash
# Get 30 days of history
curl "https://localhost:7132/api/tax/market-price-history?symbol=IBM&days=30"

# Get custom days
curl "https://localhost:7132/api/tax/market-price-history?symbol=IBM&days=60"

# Invalid days (should return 400)
curl "https://localhost:7132/api/tax/market-price-history?symbol=IBM&days=400"
```

### 2. Frontend Integration Tests

#### Exercise Modal
- [ ] Open exercise modal
- [ ] Verify default symbol (IBM) is loaded
- [ ] Change symbol and click Refresh
- [ ] Verify market price updates
- [ ] Test with invalid symbol
- [ ] Verify fallback to strike price on error
- [ ] Verify tax calculations use current market price

#### Awards List
- [ ] Load awards page
- [ ] Verify market prices load for all symbols
- [ ] Verify prices update periodically
- [ ] Edit an award and change symbol
- [ ] Verify updated symbol refreshes in list

#### Employee Dashboard
- [ ] Load dashboard
- [ ] Verify current share price displays
- [ ] Verify loading state appears briefly
- [ ] Test error state handling

### 3. Cache Testing

```typescript
// In browser console:
const service = inject(MarketPriceService);

// First call - should hit API
service.getPrice('IBM').subscribe(price => console.log(price));

// Second call (within 5 mins) - should use cache
service.getPrice('IBM').subscribe(price => console.log('Cached:', price));

// Clear cache
service.clearCache();

// Next call - should hit API again
```

### 4. Error Handling Tests

- Invalid API key (modify ApiKey in MarketPriceService)
- Network timeout (disable network and test)
- Malformed JSON response
- API rate limiting
- Symbol not found cases

### 5. Performance Tests

- Load times with caching vs without
- Batch operations with multiple symbols
- Memory usage for price cache over time
- HTTP request counts

## API Key Configuration

**Current API Key:** `3YTXQGTHPBDSWA4O`

**Alpha Vantage API Limits:**
- Free tier: 5 requests per minute, 100 per day
- Rate limiting: 500 requests per day

**To update API key:**
1. Edit `BackEnd/Services/MarketPriceService.cs`
2. Update the `ApiKey` constant
3. Rebuild and redeploy

## Supported Stock Symbols

Common symbols for testing:
- `IBM` - IBM Corporation
- `GOOGL` - Google/Alphabet
- `MSFT` - Microsoft
- `AAPL` - Apple
- `AMZN` - Amazon
- `TSLA` - Tesla

## Caching Strategy

**Frontend Cache (5 minutes):**
- Reduces API calls for frequently accessed symbols
- Automatically invalidates after 5 minutes
- Can be manually cleared with `clearCache()`

**Backend:** No caching (direct API calls each time)

## Future Enhancements

1. **Backend Caching** - Add Redis or in-memory cache for frequently requested symbols
2. **WebSocket Updates** - Real-time price updates without polling
3. **Price Alerts** - Notify users when prices reach certain thresholds
4. **Historical Charts** - Add charting library to visualize price trends
5. **Multiple Data Sources** - Integrate with Yahoo Finance, IEX Cloud for redundancy
6. **Local Caching** - Store historical data in database for improved performance

## Troubleshooting

### Issue: "Price not found" error
**Solution:** Verify symbol is valid on Alpha Vantage, check API key, ensure network connectivity

### Issue: Prices not updating
**Solution:** Clear cache with `marketPriceService.clearCache()`, check browser console for errors

### Issue: Slow API responses
**Solution:** Normal during market close, check for API rate limits, consider implementing backend caching

### Issue: Tax calculations incorrect
**Solution:** Verify market price is loaded (not using fallback), check tax service integration

## Code Examples

### Using MarketPriceService in Components

```typescript
import { MarketPriceService } from '@core/services/market-price.service';

export class MyComponent implements OnInit {
  constructor(private marketPriceService: MarketPriceService) {}

  ngOnInit() {
    // Get single price
    this.marketPriceService.getPrice('IBM').subscribe({
      next: (price) => console.log(`IBM Price: $${price.adjustedClosePrice}`),
      error: (err) => console.error('Failed to load price:', err)
    });

    // Get price history
    this.marketPriceService.getPriceHistory('IBM', 60).subscribe({
      next: (history) => {
        console.log('60-day history:', history);
        history.forEach(entry => console.log(`${entry.date}: $${entry.price}`));
      }
    });

    // Batch operation
    this.marketPriceService.getPricesBatch(['IBM', 'GOOGL', 'MSFT']).then(prices => {
      console.log('All prices:', prices);
    });
  }
}
```

## Summary

✅ **Completed:**
- Backend MarketPriceService with async API calls
- REST endpoints for current prices and history
- Angular MarketPriceService with caching
- Integration in Exercise Modal
- Integration in Awards List
- Integration in Employee Dashboard
- Comprehensive error handling
- Tax calculations using real market prices

🚀 **Ready for Production** with monitoring and additional optimizations as needed.

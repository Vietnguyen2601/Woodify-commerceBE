# 📊 ADMIN DASHBOARD RECOMMENDATIONS
## Woodify Commerce Backend

**Project Type**: Educational E-commerce Platform (Economic Focus)  
**Architecture**: Microservices (.NET 8)  
**Date**: April 8, 2026

---

## 1. EXECUTIVE SUMMARY

### 1.1 Dashboard Purpose & Scope

Vì dự án này là **dự án học tập hướng kinh tế**, dashboard admin phải:
- **Theo dõi KPIs kinh tế** (doanh số, lợi nhuận, hoa hồng, chi phí vận chuyển)
- **Hỗ trợ quyết định kinh doanh** (phân tích xu hướng, dự báo)
- **Quản lý thanh toán & tài chính** (chi tiết, minh bạch, có thể audit)
- **Làm rõ cơ chế hoa hồng** (commission tracking per order, per shop, trends)
- **Moderation & Compliance** (duyệt sản phẩm, kiểm tra vi phạm)

### 1.2 Multi-Role Architecture

```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  ADMIN SUPER    │  │  SHOP OWNER     │  │  CUSTOMER       │
│  (Platform)     │  │  (Business)     │  │  (Consumer)     │
├─────────────────┤  ├─────────────────┤  ├─────────────────┤
│ • All metrics   │  │ • Own shop data │  │ • Order status  │
│ • All shops     │  │ • Revenue       │  │ • Tracking      │
│ • Finance       │  │ • Analytics     │  │ • Wallet        │
│ • Moderation    │  │ • Settings      │  │ • Reviews       │
│ • System config │  │ • Marketing     │  │ • Cart          │
└─────────────────┘  └─────────────────┘  └─────────────────┘
         ↓                    ↓                     ↓
   (THIS DOCUMENT)      (Separate Panel)     (Separate UI)
```

---

## 2. CORE DASHBOARD FEATURES

### 2.1 SECTION 1: BUSINESS OVERVIEW (Landing/Home)

**Widget Sections**:

#### 2.1.1 Revenue Metrics (Real-Time)
```
┌─────────────────────────────────────────┐
│ TODAY'S METRICS                         │
├─────────┬─────────────────┬─────────────┤
│ Gross   │ Net Revenue     │ Commission  │
│ Revenue │ (After Commission) │ Earned  │
│ 50.5M   │ 47.5M VND        │ 3M VND  │
│ ↑ 12%   │ ↑ 8%              │ ↑ 15%   │
└─────────┴─────────────────┴─────────────┘

│ Total Users│ New Users   │ Active Today│
│ 15,234     │ 127 (+8%)   │ 3,456      │
└────────────┴─────────────┴────────────────┘
```

**Data to Display**:
- **Gross Revenue**: Sum of `SubtotalCents` + `ShippingFee` across all orders
- **Commission Revenue**: Sum of all `CommissionCents` (per commissioned order)
- **Net Revenue**: Gross - Commission - Refunds
- **Order Volume**: Count of orders by status
- **Payment Success Rate**: (Successful Payments) / (Total Attempts)

**Database Queries**:
- OrderService: Aggregate `TotalAmountCents`, `CommissionCents` by date
- PaymentService: Count by `Status`, `Provider`
- IdentityService: Count accounts by date

---

#### 2.1.2 Order Flow Overview
```
Orders Status Distribution (Real-Time)

PENDING    CONFIRMED  PROCESSING  READY    SHIPPED  DELIVERED  COMPLETED
   234        156        89       45       234      1,023      8,234
    ↓          ↓          ↓        ↓        ↓         ↓          ↓
 (12h avg) (8h avg)  (4h avg)  (2h avg) (1d avg)  (3d avg)   (avg 5d)
```

**Metrics to Show**:
- Count per status
- Average time in each status
- Conversion rate (e.g., Pending → Confirmed ratio)
- Stuck orders (stayed too long in one status)

**Sources**:
- OrderService DB: Count `Order` by `Status`
- ShipmentService DB: Aggregate time spent in each status

---

#### 2.1.3 Flash Cards / Quick Stats
```
┌──────────────────────────────────┐
│ Active Shops  │  Total Products  │
│ 234 (+5)      │  12,456 (+89)    │
└──────────────────────────────────┘

│ Pending Approval  │  Rejected Items  │
│ 45 products       │  12 products     │
└──────────────────────────────────┘

│ Customer Wallet Balance  │  Payment Methods  │
│ 123.5M VND total        │  PayOS, MoMo, ... │
└──────────────────────────────────┘
```

---

### 2.2 SECTION 2: FINANCIAL MANAGEMENT (Critical for Educational Context)

#### 2.2.1 Revenue Analytics Dashboard

**Time Series Charts with Flexible Time Range**:

**Primary Chart: Revenue Trend**
- **Default View**: Last 30 days (daily breakdown)
- **Time Range Filter Options**:
  ```
  ┌─────────────────────────────────────────────┐
  │ Filter by:                                  │
  ├─────────────────────────────────────────────┤
  │ ○ Last 7 days      (daily granularity)     │
  │ ○ Last 30 days     (daily granularity)     │ (DEFAULT)
  │ ○ Last 90 days     (daily granularity)     │
  │ ○ Last 12 months   (monthly granularity)   │
  │ ○ Custom range     (date picker)           │
  │ ○ By Month         (monthly comparison)    │
  │ ○ By Quarter       (quarterly comparison)  │
  │ ○ By Year          (yearly comparison)     │
  │ ○ Year-to-Date     (daily from Jan 1)      │
  └─────────────────────────────────────────────┘
  ```

- **Line Chart Displays** (user selects which metrics):
  - Gross Revenue (Total before commission/refunds)
  - Commission Revenue (Platform fee collected)
  - Net Revenue (Actual revenue = Gross - Commission - Refunds)

- **Daily Granularity Example** (Last 30 days):
  ```
  Date        | Gross Revenue | Commission | Net Revenue | Refunds
  ──────────────────────────────────────────────────────────────
  2026-04-08  | 5.2M          | 312K       | 4.88M       | 50K
  2026-04-07  | 4.8M          | 288K       | 4.51M       | 65K
  2026-04-06  | 6.1M          | 366K       | 5.75M       | 45K
  ...
  2026-03-10  | 3.5M          | 210K       | 3.29M       | 30K
  ```

- **Monthly Granularity** (Last 12 months):
  ```
  Month       | Gross Revenue | Commission | Net Revenue | Growth %
  ─────────────────────────────────────────────────────────────────
  2026-04     | 125.3M        | 7.5M       | 117.8M      | +8.3%
  2026-03     | 115.8M        | 6.95M      | 108.85M     | +6.2%
  2026-02     | 109.1M        | 6.55M      | 102.55M     | +4.1%
  ...
  2025-05     | 78.2M         | 4.69M      | 73.51M      | +2.1%
  ```

- **Quarterly Granularity**:
  ```
  Period      | Gross Revenue | Commission Rate | Net Revenue | QoQ Growth
  ──────────────────────────────────────────────────────────────────────
  Q1 2026     | 350.2M        | 6.0%            | 329.2M      | +12.5%
  Q4 2025     | 310.8M        | 6.1%            | 291.6M      | +8.3%
  Q3 2025     | 287.1M        | 6.05%           | 269.2M      | +5.2%
  Q2 2025     | 272.8M        | 5.95%           | 256.2M      | baseline
  ```

- **Yearly Granularity**:
  ```
  Year | Gross Revenue | Commission | Net Revenue | YoY Growth
  ───────────────────────────────────────────────────────────
  2026 | 350.2M        | 21.0M      | 329.2M      | +15.2%
  2025 | 303.6M        | 18.2M      | 285.4M      | +12.8%
  2024 | 269.2M        | 16.1M      | 253.1M      | +10.5%
  ```

**Other Revenue Analysis Charts**:
- **Revenue by Shop**: Top 10 shops by revenue (filterable by time range)
- **Revenue by Category**: Which product categories drive most revenue
- **Revenue by Payment Method**: Distribution across PayOS, MoMo, VNPay, Wallet

**Metrics to Calculate** (applies to all time ranges):
```
Gross Revenue = SUM(Order.TotalAmountCents) / 100
Commission Revenue = SUM(Order.CommissionCents) / 100
Net Revenue = Gross - Commission - Refunds

Average Order Value = Gross Revenue / Number of Orders
Commission Rate Analysis = Commission Revenue / Gross Revenue (should be ~6%)

Growth Rate = ((Current Period Revenue - Previous Period Revenue) / Previous Period Revenue) * 100
```

**Database Queries by Time Range**:
- **Daily**: `GROUP BY DATE(created_at)` with `INTERVAL '1 day'`
- **Monthly**: `GROUP BY DATE_TRUNC('month', created_at)` with `INTERVAL '1 month'`
- **Quarterly**: `GROUP BY DATE_TRUNC('quarter', created_at)` with `INTERVAL '1 quarter'`
- **Yearly**: `GROUP BY DATE_TRUNC('year', created_at)` with `INTERVAL '1 year'`
- **Custom Range**: User provides `startDate` and `endDate` parameters

**Export Options**:
- Download as CSV with selected time range
- PDF report with charts and summary statistics
- Excel with multiple sheets (daily, monthly, quarterly views)

**Visual Representation**:
- **Waterfall chart**: Gross → -Commission → -Refunds → Net
- **Pie chart**: Revenue distribution by shop/category
- **Bar chart**: Payment method adoption rates

#### 2.2.2 Commission Tracking (Key Feature)

**Why Important**: Understanding how commission system works is crucial for educational purpose.

**Tables to Display**:

| Order ID | Shop Name | Gross | Commission Rate | Commission $$ | Net (to Shop) | Status |
|----------|-----------|-------|-----------------|---------------|---------------|--------|
| ORD-001  | Wood Co   | 500k  | 6%              | 30k           | 470k          | COMPLETED |
| ORD-002  | Craft Inc | 250k  | 6%              | 15k           | 235k          | PROCESSING |

**Commission Analytics**:
```
Average Commission Rate: 6% (configurable)
Total Commission Collected: 1.23B VND
Commission Revenue Trend: ↑ 8% (week-over-week)

By Shop:
- Top Commission Earner: Shop A (300M VND)
- 10% of shops: 80% of commission (Pareto principle)
- Shops with 0 orders: 45 (opportunity for re-engagement)
```

**Filtering Options**:
- Date range
- Status (Pending, Completed, Refunded)
- Commission rate
- Shop name/category

---

#### 2.2.3 Payment Processing Dashboard

**Payment Status Metrics**:
```
Total Payment Attempts: 15,234
├─ Successful: 14,120 (92.7%)
├─ Pending: 456 (3.0%)
├─ Failed: 324 (2.1%)
└─ Cancelled: 334 (2.2%)

Payment Methods:
├─ PayOS: 6,234 (40.9%) ✅ Highest trust
├─ MoMo: 5,123 (33.6%)
├─ VNPay: 2,567 (16.9%)
└─ Wallet: 1,310 (8.6%)
```

**Real-Time Payment Monitoring**:
- Recent payment transactions (last 100 attempts)
- Failed payments with reason (timeout, declined, etc.)
- Average payment processing time
- Payment method success rate ranking

**Financial Reconciliation**:
- Expected revenue (from OrderService)
- Actual received (from PaymentService)
- Discrepancy alert if > 1%

#### 2.2.4 Wallet Ecosystem

**Wallet Statistics**:
```
Total Wallet Balances: 2.34B VND
├─ User Wallets: 1.89B VND
├─ Shop Settlement: 450M VND (pending payout)
└─ Platform Reserve: 0VND

Wallet Transactions (Daily):
├─ Top-ups: 120 transactions, +450M VND
├─ Payments: 8,234 transactions, -3.2B VND
├─ Refunds: 45 transactions, +125M VND
└─ Withdrawals: 12 transactions, -200M VND
```

**Transaction History Table**:
- Filter by user, transaction type, date range
- Export to CSV for accounting

---

### 2.3 SECTION 3: PRODUCT MANAGEMENT & MODERATION

#### 2.3.1 Product Lifecycle Tracking

**Product Status Distribution**:
```
DRAFT      PENDING     APPROVED    PUBLISHED   ARCHIVED    DELETED
 1,234      456         234         8,234       123         78
 
 (Not ready) (Awaiting)  (Approved)  (Active)    (Hidden)  (Removed)
             approval
```

**Key Metrics**:
- Approval time: Average time from PENDING → APPROVED
- Rejection rate: % of products rejected by moderators
- Time to publish: DRAFT → PUBLISHED conversion time
- Shelf life: Average time product stays PUBLISHED

#### 2.3.2 Moderation Dashboard (Critical!)

```
PENDING APPROVALS: 45 products
Recent Submissions (Last 24h):

| Product | Shop | Category | Status | Submitted | Age | Action |
|---------|------|----------|--------|-----------|-----|--------|
| Oak Chair | Shop A | Furniture | PENDING | 14:30 | 2h | Review |
| Bamboo Shelf | Shop B | Storage | PENDING | 12:15 | 4h | Review |

REJECTION QUEUE: 12 products (needs revision)

| Product | Reason | Submitted | Days Ago |
|---------|--------|-----------|----------|
| Table | Low quality image | 2026-04-02 | 6 days |
| Cabinet | Price too high | 2026-04-03 | 5 days |
```

**Moderation Logic**:
- Auto-check: Image quality, product description length, price range
- Manual queue: Flag to moderators for review
- Approval: Mark status APPROVED
- Rejection: Store reason for shop owner

**SLA Metrics**:
- Target: 80% of products approved within 24h
- Alert if queue > 50 items

---

#### 2.3.3 Product Catalog Analytics

**Best Performing Products**:
```
Rank | Product Name | Category | Revenue | Orders | Rating | Reviews |
-----|--------------|----------|---------|--------|--------|---------|
1    | Oak Desk     | Furniture| 12.5M   | 234    | 4.8    | 127     |
2    | Pine Chair   | Furniture| 11.2M   | 198    | 4.7    | 114     |
3    | Bamboo Shelf | Storage  | 9.8M    | 156    | 4.5    | 89      |
...
```

**Low Performers**:
```
Products with NO orders (past 30 days): 234
- Action: Auto-archive or notify shop to revise
```

**Category Analysis**:
- Revenue by category
- Growth rate by category
- Category trend (furniture up 12%, accessories down 5%)

---

### 2.4 SECTION 4: SHOP & SELLER MANAGEMENT

#### 2.4.1 Shop Performance Dashboard

```
TOTAL SHOPS: 234
├─ Active (published): 156 (66.7%)
├─ Inactive (draft/pending): 45 (19.2%)
├─ Suspended: 12 (5.1%)
└─ Deleted: 21 (9.0%)

TOP SHOPS (By Revenue):

Rank | Shop Name | Owner | Revenue | Orders | Rating | Followers |
-----|-----------|-------|---------|--------|--------|-----------|
1    | Wood Inc  | John  | 45.2M   | 1,234  | 4.9    | 5,234     |
2    | Fair Trade| Jane  | 38.5M   | 987    | 4.8    | 4,567     |
3    | Eco Wood  | Bob   | 32.1M   | 756    | 4.7    | 3,890     |
```

#### 2.4.2 Shop Metrics to Monitor

**For Each Shop**:
- **Total Products**: Count of products (PUBLISHED, DRAFT, ARCHIVED)
- **Revenue**: Sum of all orders from shop
- **Average Rating**: Mean of all reviews
- **Review Count**: Total number of reviews
- **Order Fulfillment Rate**: % of orders completed vs cancelled
- **Average Delivery Time**: From order to delivered
- **Customer Satisfaction**: % of positive reviews
- **Commission Owed**: Pending commission for next payout

**Shop Health Card**:
```
Shop ID: UUID
Owner: John Doe
Status: ACTIVE ✅
├─ Revenue Growth: +15% (month-over-month)
├─ Order Volume: 45 orders (this week)
├─ Average Rating: 4.8/5.0
├─ Commission Outstanding: 2.5M VND
├─ Followers: 5,234
└─ Products: 34 (27 published, 7 draft)

Risk Indicators:
⚠️ Low rating trend: 4.8 → 4.5 (last month)
📉 Orders declining: 56 → 45 (-19%)
```

---

#### 2.4.3 Seller KPIs Table

| KPI | Target | Current | Trend | Status |
|-----|--------|---------|-------|--------|
| Average Rating | 4.5+ | 4.8 | ↑ | ✅ GOOD |
| Order Fulfillment | 95%+ | 93% | ↓ | ⚠️ WARNING |
| Delivery On-Time | 90%+ | 85% | ↓ | ⚠️ WARNING |
| Response Time | <2h | 3.2h | ↑ | ❌ POOR |
| Return Rate | <3% | 5.2% | ↑ | ❌ POOR |

---

### 2.5 SECTION 5: ORDER & SHIPMENT TRACKING

#### 2.5.1 Real-Time Order Pipeline

```
ORDERS IN FLIGHT (Real-Time):

PENDING (234)          PROCESSING (89)        SHIPPED (234)         DELIVERED (8,234)
│                      │                       │                      │
├─ Oldest: 4h ago      ├─ Oldest: 2h ago      ├─ In transit: 1d     ├─ Completed: 8,234
├─ Action: Alert if >6h├─ Action: Urgent      ├─ Lost: 23 packages  ├─ Need Review: 1,234
└─ Re-engage necessary └─ Prepare shipment     └─ Insurance: 5       └─ Refund: 45
```

**Problematic Orders**:
- PENDING > 6 hours: Likely payment issue or system stuck
- SHIPPED > 7 days: Likely lost in transit or delivery issue
- CANCELLED orders: Reason analytics
- REFUNDING pending: Days since refund initiated

#### 2.5.2 Shipment Analytics

```
SHIPMENT STATISTICS:

Total Packages: 12,345
├─ Delivered On-Time: 11,234 (91.0%) ✅
├─ Late Delivery: 876 (7.1%) ⚠️
├─ Lost/Damaged: 45 (0.4%) ❌
│  ├─ With Insurance: 23
│  └─ Need Refund: 22
└─ Still In Transit: 190 (1.5%)

Delivery by Provider:
├─ GHN: 4,234 (91.2% on-time) → Best performer
├─ Viettel: 3,567 (88.5% on-time)
├─ J&T: 2,890 (85.3% on-time) → Needs improvement
└─ Others: 1,654

Average Delivery Time: 3.2 days
Last Mile Cost: 18,500 VND per package
```

#### 2.5.3 Shipment CMS (Content Management System)

**Provider Management**:
```
Shipping Providers:
├─ GHN (Giao Hàng Nhanh)
│  ├─ Status: ACTIVE ✅
│  ├─ Services: 3 (Standard, Fast, Express)
│  ├─ Performance: 91.2% on-time ✅
│  ├─ Fee: 15K - 45K VND
│  └─ Integration: Connected
│
├─ Viettel Post
│  ├─ Status: ACTIVE ✅
│  ├─ Services: 2
│  ├─ Performance: 88.5% on-time ⚠️
│  ├─ Fee: 18K - 50K VND
│  └─ Integration: Connected
│
└─ [+ADD NEW PROVIDER]
```

**ProviderService Mapping**:
- Show available services per provider
- Configure service codes
- Set shipping fee tiers
- Monitor performance metrics

---

### 2.6 SECTION 6: SYSTEM HEALTH & MONITORING

#### 2.6.1 Service Status

```
🟢 All Services Operational | Last Updated: 5 seconds ago

┌─────────────────────────────────────────────────┐
│ SERVICE         │ STATUS │ CPU  │ MEMORY │ UPTIME│
├─────────────────────────────────────────────────┤
│ Identity        │ ✅     │ 12%  │ 256MB  │ 15d  │
│ Product         │ ✅     │ 18%  │ 512MB  │ 15d  │
│ Order           │ ✅     │ 25%  │ 384MB  │ 14d  │
│ Payment         │ ✅     │ 8%   │ 128MB  │ 15d  │
│ Shipment        │ ✅     │ 15%  │ 256MB  │ 15d  │
│ Shop            │ ✅     │ 10%  │ 192MB  │ 15d  │
│ API Gateway     │ ✅     │ 22%  │ 512MB  │ 15d  │
└─────────────────────────────────────────────────┘

Database Status:
├─ PostgreSQL (Primary): Connected | 85% Disk Used ⚠️
└─ RabbitMQ: Connected | 234 queued messages
```

#### 2.6.2 Event Log Monitoring

```
RECENT SYSTEM EVENTS (Last 24h):

⚠️ [14:32] Order Service: High latency detected (p95: 2.3s)
✅ [12:15] Payment Service: 100% success rate
⚠️ [10:45] RabbitMQ: 234 messages in queue (normal: <50)
✅ [08:20] Database: Automatic backup completed (12.4GB)
⚠️ [06:30] Product Service: 5 rejected products need moderation
✅ [04:15] Nightly Report: Generated successfully
```

---

### 2.7 SECTION 7: REPORTING & EXPORT

#### 2.7.1 Report Templates

**Built-in Reports**:
1. **Daily Business Report**
   - Revenue, orders, new shops, transactions
   - Generated 6 AM daily
   - Email to management team

2. **Weekly Analytics**
   - Trend analysis
   - Top performers, bottom performers
   - Customer acquisition, retention

3. **Monthly P&L Statement**
   - Gross revenue by category/shop
   - Commissions, refunds
   - Operating costs (if tracked)
   - Net profit summary

4. **Compliance Report**
   - Moderation stats (approved, rejected, pending)
   - Product quality issues
   - Customer complaints

5. **Shipment Performance**
   - On-time delivery rate by provider
   - Lost/damaged packages
   - Insurance claims

#### 2.7.2 Export Options

- **CSV Export**: Data for Excel analysis
- **PDF Report**: Formatted for printing/sharing
- **Email Alert**: Automatic schedule
- **Custom Queries**: SQL for power users

---

## 3. TECHNICAL ARCHITECTURE RECOMMENDATIONS

### 3.1 Technology Stack

#### Frontend (Recommended)
```
┌─────────────────────────────────────────────────────────┐
│ Framework        │ React OR Vue.js 3                   │
├─────────────────────────────────────────────────────────┤
│ State Management │ Redux Toolkit OR Pinia              │
├─────────────────────────────────────────────────────────┤
│ UI Components    │ Ant Design OR Material-UI           │
├─────────────────────────────────────────────────────────┤
│ Charts           │ ECharts OR Chart.js                 │
├─────────────────────────────────────────────────────────┤
│ Tables           │ TanStack Table (React Table) v8     │
├─────────────────────────────────────────────────────────┤
│ Real-Time        │ Socket.IO for live updates          │
├─────────────────────────────────────────────────────────┤
│ HTTP Client      │ Axios                               │
├─────────────────────────────────────────────────────────┤
│ Build Tool       │ Vite                                │
└─────────────────────────────────────────────────────────┘
```

#### Backend (New Service)
```
┌─────────────────────────────────────────────────────────┐
│ Language         │ C# / .NET 8 (for consistency)       │
├─────────────────────────────────────────────────────────┤
│ Status           │ Optional - can query services       │
├─────────────────────────────────────────────────────────┤
│ Caching          │ Redis for aggregated metrics        │
├─────────────────────────────────────────────────────────┤
│ Real-Time        │ SignalR for live dashboard updates  │
├─────────────────────────────────────────────────────────┤
│ Aggregation      │ Scheduled background jobs           │
│                  │ (e.g., daily report generation)     │
├─────────────────────────────────────────────────────────┤
│ Logging          │ ELK Stack or Application Insights   │
└─────────────────────────────────────────────────────────┘
```

**Why separate AdminDashboard service?**
- Read-only access to other services
- Complex queries don't slow down core services
- Can aggregate data from multiple services
- Independent scaling
- Permission/authorization layer

### 3.2 Data Model for Dashboard Service

```
AdminDashboard.Database Schema:
├─ DailyMetrics
│  ├─ Date (PK)
│  ├─ GrossRevenue
│  ├─ CommissionRevenue
│  ├─ OrderCount
│  ├─ PaymentSuccessRate
│  └─ CreatedAt
│
├─ ShopMetrics (Daily snapshots)
│  ├─ ShopId
│  ├─ Date
│  ├─ Revenue
│  ├─ OrderCount
│  ├─ AverageRating
│  └─ FollowerCount
│
├─ PaymentMethodStats
│  ├─ Provider (PayOS, MoMo, VNPay)
│  ├─ Date
│  ├─ SuccessCount
│  ├─ FailureCount
│  └─ TotalAmount
│
├─ CategoryMetrics
│  ├─ CategoryId
│  ├─ Date
│  ├─ Revenue
│  ├─ Orders
│  └─ GrowthRate
│
├─ ShipmentMetrics
│  ├─ ProviderId
│  ├─ Date
│  ├─ OnTimeRate
│  ├─ LostCount
│  └─ AverageDeliveryDays
│
└─ ModerationQueue
   ├─ ProductId
   ├─ ShopId
   ├─ SubmittedAt
   ├─ Status (PENDING, APPROVED, REJECTED)
   └─ ReviewerNote
```

### 3.3 Data Aggregation Strategy

```
Real-Time Updates (Every 5 seconds via SignalR):
├─ Total Revenue (Today)
├─ Active Orders Count
├─ Payment Success Rate (Last hour)
└─ Recent Transactions (Last 20)

Background Jobs (Schedule):
├─ Hourly: Aggregate metrics by hour
├─ Daily (23:55 PM): Generate daily reports, snapshots
├─ Weekly (Every Monday 00:00): Week summary
├─ Monthly (1st of month): Month-end closing
└─ On-Demand: User-triggered report generation
```

### 3.4 Caching Strategy

**Use Redis for**:
- Hourly/daily aggregated metrics
- Top 10 shops (updated hourly)
- Payment method distribution
- Category rankings

**TTL (Time To Live)**:
- Real-time metrics: 5 seconds
- Hourly aggregates: 1 hour
- Daily reports: 24 hours

```
Key Pattern Examples:
├─ metrics:daily:{date} → Daily metrics
├─ metrics:hourly:{date}:{hour} → Hourly metrics
├─ shop:top10:revenue → Top 10 shops by revenue
├─ payment:methods:{date} → Payment distribution
└─ moderation:queue → Current pending items
```

---

## 4. API ENDPOINTS NEEDED

### 4.1 Dashboard Endpoints

```
GET /api/admin/dashboard/overview
├─ Returns: Revenue summaries, order counts, user stats
├─ Real-time: ✅
└─ Cache: 5 seconds

GET /api/admin/dashboard/revenue/daily
├─ Query: ?startDate=2026-04-01&endDate=2026-04-08
├─ Returns: Daily revenue breakdown, commission, refunds
└─ Cache: 1 hour

GET /api/admin/dashboard/orders/status-distribution
├─ Returns: Count per order status
└─ Real-time: ✅

GET /api/admin/dashboard/shops/top-10
├─ Returns: Top 10 shops by revenue, rating, orders
└─ Cache: 1 hour

GET /api/admin/dashboard/payment/methods
├─ Returns: Success rate by payment method
└─ Cache: 1 hour

POST /api/admin/reports/generate
├─ Body: { reportType: 'daily' | 'weekly' | 'monthly', format: 'pdf' | 'csv' }
├─ Returns: Report file download
└─ Async job

GET /api/admin/moderation/queue
├─ Returns: Pending products awaiting approval
├─ Real-time: ✅
└─ Pagination: Yes

POST /api/admin/moderation/approve/{productId}
├─ Body: { reason: 'text' }
├─ Returns: Success/error
└─ Authorization: Admin only

POST /api/admin/moderation/reject/{productId}
├─ Body: { reason: 'text' }
├─ Returns: Success/error
└─ Authorization: Admin only
```

### 4.2 WebSocket/SignalR Events

```
HubName: /admin-dashboard

Events:
├─ MetricsUpdated → Real-time metrics
│  ├─ { grossRevenue, netRevenue, total CommissionCents, orderCount }
│
├─ OrderStatusChanged → Order workflow update
│  ├─ { orderId, status, timestamp }
│
├─ PaymentProcessed → Payment event
│  ├─ { paymentId, amount, provider, status }
│
├─ ProductModerationNeeded → New product in queue
│  ├─ { productId, shopName, submittedAt }
│
└─ ShipmentUpdated → Tracking update
   ├─ { shipmentId, status, trackingNumber, eta }
```

---

## 5. UI/UX DESIGN PATTERNS

### 5.1 Dashboard Layout Structure

```
┌─────────────────────────────────────────────────────────────┐
│ HEADER: Woodify Admin Dashboard | Settings | Logout       │
├─────────────────────────────────────────────────────────────┤
├────────────────┬──────────────────────────────────────────┤
│                │                                          │
│ SIDEBAR        │ MAIN CONTENT AREA                        │
│ ─────────────  │ ────────────────────────────────────────  │
│ • Dashboard    │ [TAB CONTENT]                            │
│ • Orders       │ ┌─────────────────────────────────────┐ │
│ • Products     │ │ TAB 1: Overview                     │ │
│ • Shops        │ │ • Key metrics cards                 │ │
│ • Finance      │ │ • Revenue chart                     │ │
│ • Shipments    │ │ • Recent activities                 │ │
│ • Moderation   │ │ • System health                     │ │
│ • Settings     │ ├─────────────────────────────────────┤ │
│ • Reports      │ │ TAB 2: Analytics                    │ │
│ • System       │ │ • Detailed charts                   │ │
│                │ │ • Filters & date pickers            │ │
│                │ │ • Export options                    │ │
│                │ └─────────────────────────────────────┘ │
│                │                                          │
└────────────────┴──────────────────────────────────────────┘
```

### 5.2 Color Scheme & Branding

```
Primary Colors:
├─ Primary: #1890FF (Azure Blue) - Main theme
├─ Success: #52C41A (Green) - Positive metrics
├─ Warning: #FAAD14 (Orange) - Items needing attention
├─ Error: #F5222D (Red) - Errors/failed transactions
└─ Neutral: #8C8C8C (Gray) - Secondary information

Text Colors:
├─ Primary text: #262626
├─ Secondary: #8C8C8C
└─ Disabled: #BFBFBF

Background:
├─ Page background: #FAFAFA (Light gray)
├─ Card background: #FFFFFF
└─ Hover: #F5F5F5
```

### 5.3 Key UI Components

#### Cards / Metric Display
```
┌──────────────────────────┐
│ Today's Revenue          │
├──────────────────────────┤
│ 50.5M VND               │
│ ↑ 12% from yesterday    │
│ ─────────────────────── │
│ Compare with week ago ▼ │
└──────────────────────────┘
```

#### Data Tables
```
Features:
├─ Sorting: Click column header
├─ Filtering: Multi-column filter panel
├─ Pagination: 10, 50, 100 rows per page
├─ Selection: Bulk actions on selected rows
├─ Export: CSV, PDF, Excel
├─ Search: Global search bar
└─ Row Details: Click to expand (drawer/modal)
```

#### Charts
```
Time Series (Line/Area):
├─ X-axis: Dates
├─ Y-axis: Revenue, Orders, etc.
├─ Tooltip: Details on hover
├─ Legend: Click to show/hide
└─ Zoom: Drag to zoom in

Categorical (Bar/Pie):
├─ Categories: Shops, Payment methods, Status
├─ Values: Revenue, Count
├─ Legend: Clickable
└─ Drill-down: Click to see details
```

### 5.4 Responsive Design

**Breakpoints**:
- **Mobile** (< 768px): Sidebar becomes drawer, single-column layout
- **Tablet** (768px - 1024px): 2 columns
- **Desktop** (> 1024px): Full 3-column layout

---

## 6. SECURITY & PERMISSIONS

### 6.1 Role-Based Access Control (RBAC)

```
Admin Dashboard Roles:

┌─────────────────────────────────────────┐
│ Role: ADMIN_SUPER                       │
├─────────────────────────────────────────┤
│ • View all metrics                      │
│ • Manage shops (activate, suspend)     │
│ • Approve/reject products              │
│ • View financial reports                │
│ • Manage payment providers              │
│ • System configuration                  │
│ • User management                       │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Role: ADMIN_FINANCE                     │
├─────────────────────────────────────────┤
│ • View revenue reports                  │
│ • Commission tracking                   │
│ • Payment monitoring                    │
│ ✗ Cannot modify moderation              │
│ ✗ Cannot suspend shops                  │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Role: ADMIN_MODERATOR                   │
├─────────────────────────────────────────┤
│ • View moderation queue                 │
│ • Approve/reject products              │
│ • Leave feedback for sellers            │
│ ✗ Cannot view financial data            │
│ ✗ Cannot manage shops                   │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Role: ADMIN_SUPPORT                     │
├─────────────────────────────────────────┤
│ • View orders, shipments                │
│ • Track shipments                       │
│ • Escalate issues                       │
│ ✗ Cannot modify orders                  │
└─────────────────────────────────────────┘
```

### 6.2 Audit Logging

**Track Every Action**:
```
Audit Log Entry:
├─ AdminId: UUID
├─ Action: APPROVE_PRODUCT / REJECT_PRODUCT / SUSPEND_SHOP
├─ ResourceId: ProductId / ShopId
├─ Timestamp: 2026-04-08T14:32:15Z
├─ Status: SUCCESS / FAILURE
├─ IPAddress: 192.168.1.100
├─ UserAgent: Chrome 125
└─ Details: { reason: "Low quality images", ... }
```

**Audit Trail Table**:
- Sortable, filterable by action, user, date
- Export for compliance
- Immutable (cannot delete audit logs)

### 6.3 Data Privacy

**PII Handling**:
- Mask customer names/emails (show initials only)
- Mask payment card numbers (last 4 digits only)
- Mask personal phone numbers
- Bank account numbers: Hidden by default

```
Example:
├─ Full: john.doe@example.com → Display: j****@example.com
├─ Full: 0901234567 → Display: 090****67
├─ Full: 1234 5678 9012 3456 → Display: •••• •••• •••• 3456
```

---

## 7. PERFORMANCE OPTIMIZATION

### 7.1 Data Loading Strategy

**Lazy Loading**:
- Load only visible tabs
- Pagination for large tables
- Infinite scroll for activity feeds

**Progressive Enhancement**:
1. Load summary metrics immediately (cached)
2. Load charts while user views dashboard
3. Load detailed tables on demand (user clicks tab)

### 7.2 Query Optimization

**Avoid N+1 Queries**:
```csharp
// WRONG: N+1 query problem
var shops = await _context.Shops.ToListAsync();
foreach (var shop in shops) {
    var metrics = await _context.ShopMetrics
        .Where(m => m.ShopId == shop.ShopId)
        .ToListAsync();
}

// CORRECT: Join and load together
var shops = await _context.Shops
    .Include(s => s.Metrics)
    .ToListAsync();
```

**Caching Layer**:
- Cache entire endpoints (not individual queries)
- Invalidate on related data updates
- Use HTTP caching headers (ETag, Cache-Control)

### 7.3 Frontend Performance

**Code Splitting**:
- Load modules only when needed
- Lazy load chart library (heavyweight)
- Separate reports into async chunks

**Bundle Size**:
- Target: < 500 KB initial load
- Gzip compression
- Tree-shaking unused code
- Image optimization

---

## 8. TESTING STRATEGY

### 8.1 Unit Tests

**Dashboard Service Tests**:
```csharp
[Test]
public void CalculateCommissionRevenue_ReturnsCorrectAmount()
{
    // Arrange
    var orders = new[] {
        new Order { TotalAmountCents = 1_000_000, CommissionRate = 0.06m },
        new Order { TotalAmountCents = 500_000, CommissionRate = 0.06m },
    };
    
    // Act
    var commission = _service.CalculateCommission(orders);
    
    // Assert
    Assert.AreEqual(90_000, commission); // 6% of 1.5M
}
```

### 8.2 Integration Tests

- API endpoint tests (GET, POST, requires auth)
- Database aggregation tests
- Permission/RBAC tests

### 8.3 E2E Tests (Recommended)

**Scenarios to Test**:
1. Admin login → View dashboard → Export report
2. Moderation flow: Product arrives → Review → Approve
3. Financial reconciliation: Orders → Payments → Settlement
4. Shop suspension: Find shop → Suspend → Verify revenue excluded

**Tools**: Playwright, Cypress, Selenium

---

## 9. DEPLOYMENT & INFRASTRUCTURE

### 9.1 Hosting

**Backend (AdminDashboard Service)**:
- Container: Docker
- Orchestration: Docker Compose (dev), Kubernetes (prod)
- Database: PostgreSQL (separate from other services)
- Cache: Redis
- CDN: CloudFront (for static assets)

### 9.2 Scaling Considerations

```
Horizontal Scaling:
├─ Multiple AdminDashboard API instances (load balanced)
├─ Redis cluster for caching
├─ Read replicas of PostgreSQL for reporting
└─ Async job queue (e.g., Hangfire) for report generation

Monitoring:
├─ CPU, Memory, Disk usage
├─ API response time (p50, p95, p99)
├─ Database query performance
├─ Cache hit ratio
└─ WebSocket connection count
```

---

## 10. IMPLEMENTATION ROADMAP

### Phase 1: MVP (3-4 weeks)
```
Week 1:
├─ Setup AdminDashboard service (.NET 8)
├─ Design database schema
├─ Setup React frontend scaffold
└─ Create authentication middleware

Week 2:
├─ Implement core metrics endpoints
├─ Build dashboard overview page
├─ Real-time order/revenue updates via SignalR
└─ Basic charts (revenue, orders)

Week 3:
├─ Financial dashboard (commission tracking)
├─ Order management view
├─ Product moderation queue
└─ Basic authorization/RBAC

Week 4:
├─ Testing & bug fixes
├─ Performance optimization
├─ Documentation
└─ Deployment setup
```

### Phase 2: Enhanced Features (2-3 weeks)
```
├─ Advanced analytics & trending
├─ Shop management section
├─ Shipment tracking integration
├─ Report generation & export
├─ Audit logging
└─ Role-based access control
```

### Phase 3: Polish & Scale (1-2 weeks)
```
├─ UI/UX refinement
├─ Mobile responsiveness
├─ Performance tuning
├─ Security hardening
└─ A/B testing features
```

---

## 11. SUCCESS METRICS

### 11.1 Business Metrics
- Dashboard adoption: % of admins using it daily
- Time to insight: Can user find a specific metric in < 2 min?
- Report accuracy: Does exported data match actual revenue?

### 11.2 Technical Metrics
- Page load time: < 2 seconds
- API response time: < 500ms (p95)
- Chart rendering: < 1 second
- Cache hit rate: > 80%
- Availability: 99.5% SLA

### 11.3 User Experience
- System Usability Scale (SUS) score: > 75
- User satisfaction: > 4.0 / 5.0
- Feature adoption: Key features used by > 70% of admins

---

## 12. EDUCATIONAL VALUE

### 12.1 Learning Outcomes

Student should learn:

1. **Systems Design**:
   - Microservices architecture
   - Distributed data aggregation
   - Caching strategies
   - Real-time updates (WebSockets)

2. **Database**:
   - Complex queries and optimization
   - Time-series data
   - Aggregations and analytics
   - Denormalization for performance

3. **Frontend**:
   - Real-time dashboard UI
   - Data visualization
   - Responsiveness
   - State management

4. **Business**:
   - E-commerce metrics & KPIs
   - Commission structures
   - Financial reporting
   - Supply chain (shipment tracking)

### 12.2 Project Documentation

**Must Include**:
- Architecture decisions & trade-offs
- Database schema with rationale
- Performance benchmarks
- Cost analysis (cloud resources)
- Scalability projections

---

## 13. FREQUENTLY ASKED QUESTIONS

### Q1: Should I create a separate service or integrate into existing services?

**Answer**: Create separate **AdminDashboardService** because:
- It's read-heavy (won't impact transactional services)
- Complex aggregations (better isolated)
- Independent scaling
- Security isolation (only admins access)
- Easier to test separately

### Q2: How to keep dashboard data fresh without impacting main services?

**Answer**: Use **denormalization** pattern:
```
Real-time layer:
├─ Direct queries for live metrics (< 5s cache)
├─ WebSocket for instant updates
└─ Redis for hot data

Batch layer:
├─ Scheduled aggregation jobs (hourly/daily)
├─ Write to denormalized tables
├─ Build historical trends
└─ Generate reports overnight
```

### Q3: What about sensitive financial data in reports?

**Answer**: Implement **audit trail**:
- Every report access is logged
- Who accessed, when, what data, how long viewed
- Data masking for export (PII)
- Encryption at rest (database column encryption)

### Q4: How to handle millions of orders for reporting?

**Answer**: **Time-bucketing strategy**:
- Store hourly/daily aggregates
```sql
SELECT 
  DATE(created_at) as date,
  SUM(total_amount) as daily_revenue,
  COUNT(*) as order_count
FROM Orders
GROUP BY DATE(created_at)
```
- Materialized views for common queries
- Archive old data (keep last 2 years hot, archive older)

### Q5: Performance concern: Real-time vs. Accuracy?

**Answer**: **Hybrid approach**:
```
Real-time display (5s cache):
├─ Estimated based on last hour trend
├─ Fast response
└─ Good enough for monitoring

Accurate reporting (batch jobs):
├─ Runs nightly (00:00)
├─ Queries all data (no cache)
├─ Used for financial statements
└─ Shows [Last updated: yesterday 00:15]
```

---

## 14. CONCLUSION

### Key Takeaways

1. **Focus on Economics**: Dashboard must clearly show profit/loss, commission flow, and financial trends
2. **Real-Time Monitoring**: Alert admins to issues immediately
3. **Modularity**: Design for future expansion (reports, notifications, etc.)
4. **Learning First**: Prioritize code clarity and documentation over optimizations
5. **User-Centric**: Test with actual admins to understand workflows

### Recommended Tech Stack (MERN Alternative)

If you prefer MERN instead of .NET:

```
Frontend: React + Redux Toolkit + Ant Design + ECharts
Backend: Node.js + Express + TypeScript
Database: PostgreSQL + Redis
Real-Time: Socket.IO
Deployment: Docker + Docker Compose
```

### Final Recommendation

Start with **Phase 1 MVP** focusing on:
- Revenue/commission tracking ✅
- Order pipeline visualization ✅
- Product moderation queue ✅
- Basic admin authentication ✅

Then expand based on feedback and learning outcomes.

---

**Document Version**: 1.0  
**Created**: April 8, 2026  
**For**: Woodify Commerce Backend Project (Educational Purpose)  
**Contact**: [Your Team]

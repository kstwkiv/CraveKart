# 🌿 CraveKart — Food Delivery Platform

A full-stack food delivery platform built with **.NET 10 Microservices** and **Angular 21**, featuring real-time order tracking, multi-role dashboards, and a complete restaurant management system.

---

## Architecture

CraveKart follows a **microservices architecture** with an API Gateway routing all requests from the Angular frontend to the appropriate backend service.

```
Angular Frontend (port 4200)
        │
        ▼
  API Gateway / Ocelot (port 5000)
        │
   ┌────┴────────────────────────────────┐
   │                                     │
Identity.API   Restaurant.API   Order.API   Delivery.API   Payment.API   Notification.API
 (port 5001)    (port 5002)    (port 5003)   (port 5005)   (port 5004)    (port 5006)
        │                                     │
        └──────────── RabbitMQ ───────────────┘
                   (Message Bus)
        │
      SQL Server
   (separate DB per service)
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 21, TypeScript, SCSS |
| Backend | .NET 10, ASP.NET Core, EF Core |
| API Gateway | Ocelot |
| Messaging | RabbitMQ + MassTransit |
| Real-time | SignalR (delivery tracking) |
| Database | SQL Server (one DB per service) |
| Auth | JWT Bearer tokens |
| Email | SMTP (Gmail) |

---

## Services

### Identity.API — Authentication & Users
- Register, login, logout
- JWT token generation & refresh
- Password reset via OTP email
- Roles: `Customer`, `RestaurantOwner`, `DeliveryAgent`, `Admin`

### Restaurant.API — Restaurant & Menu Management
- Restaurant CRUD with image upload
- Admin approval workflow
- Menu categories and items
- Customer reviews & owner replies
- Rating aggregation

### Order.API — Order Lifecycle
- Place orders with cart items
- Order status progression: Placed → Confirmed → Preparing → Ready → PickedUp → Delivered
- Order history per customer
- Admin order management

### Delivery.API — Delivery Agent System
- Agent registration & profile
- Self-assign ready orders
- Real-time GPS location sharing via SignalR
- Earnings tracking (₹100 per delivery)
- Vehicle type management

### Payment.API — Payment Processing
- Process payments (Card / Cash on Delivery)
- Payment status tracking

### Notification.API — Email Notifications
- Beautiful HTML email templates for all events
- Order placed, status updates, delivery completed
- Restaurant approved/rejected
- Welcome email, login alerts, password reset OTP

---

## User Roles

| Role | Access |
|---|---|
| **Customer** | Browse restaurants, place orders, track delivery, leave reviews |
| **Restaurant Owner** | Manage restaurants, menu items, incoming orders |
| **Delivery Agent** | View ready orders, pick up & deliver, track earnings |
| **Admin** | Approve/reject restaurants, manage all orders & users |

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- SQL Server (local or Docker)
- RabbitMQ (local or Docker)

### 1. Start Infrastructure

```bash
docker-compose up -d
```

This starts SQL Server and RabbitMQ.

### 2. Run Database Migrations

Run in each service folder:

```bash
dotnet ef database update --project Services/Identity.API
dotnet ef database update --project Services/Restaurant.API
dotnet ef database update --project Services/Order.API
dotnet ef database update --project Services/Payment.API
dotnet ef database update --project Services/Delivery.API
```

### 3. Start Backend Services

Open 7 terminals and run each:

```bash
dotnet run --project ApiGateway
dotnet run --project Services/Identity.API
dotnet run --project Services/Restaurant.API
dotnet run --project Services/Order.API
dotnet run --project Services/Payment.API
dotnet run --project Services/Delivery.API
dotnet run --project Services/Notification.API
```

### 4. Start Frontend

```bash
cd foodfleet-web
npm install
ng serve
```

Open **http://localhost:4200**

---

## API Ports

| Service | Port |
|---|---|
| API Gateway | 5000 |
| Identity.API | 5001 |
| Restaurant.API | 5002 |
| Order.API | 5003 |
| Payment.API | 5004 |
| Delivery.API | 5005 |
| Notification.API | 5006 |

---

## Key Features

- 🔐 JWT authentication with role-based access control
- 🍽️ Restaurant browsing with cuisine filters, search, and sorting
- 🛒 Cart with minimum order validation and dish suggestions
- 📦 Real-time order tracking with SignalR
- 🛵 Delivery agent dashboard with live GPS sharing
- 💰 Agent earnings tracking (₹100/delivery)
- 📧 Beautiful HTML email notifications for all events
- ⭐ Customer reviews with owner replies
- 🎯 Personalized restaurant recommendations based on order history
- 📱 Fully responsive design

---

## Project Structure

```
FoodFleetMicroservices/
├── ApiGateway/              # Ocelot API Gateway
├── Services/
│   ├── Identity.API/        # Auth & user management
│   ├── Restaurant.API/      # Restaurants & menus
│   ├── Order.API/           # Orders
│   ├── Payment.API/         # Payments
│   ├── Delivery.API/        # Delivery agents
│   └── Notification.API/    # Email notifications
├── Shared/                  # Shared events & messaging
├── foodfleet-web/           # Angular frontend
└── docker-compose.yml       # Infrastructure (SQL Server + RabbitMQ)
```

---

## Testing

CraveKart includes a dedicated NUnit test project that covers the core business logic of the backend services using **mocked dependencies** (no database or message broker required).

### Test Project Location

```
Tests/
└── FoodFleet.Tests/
    ├── FoodFleet.Tests.csproj
    ├── Identity/
    │   └── AuthServiceTests.cs          # 7 tests
    ├── Orders/
    │   ├── PlaceOrderHandlerTests.cs    # 7 tests
    │   └── CancelOrderHandlerTests.cs   # 9 tests
    ├── Payments/
    │   └── PaymentServiceTests.cs       # 13 tests
    └── Delivery/
        └── AssignDeliveryHandlerTests.cs # 6 tests
```

### Tools & Libraries

| Tool | Purpose |
|---|---|
| **NUnit 4** | Test framework — `[TestFixture]`, `[Test]`, `[TestCase]`, `Assert.That` |
| **Moq 4** | Mocking framework — replaces DB, JWT, password, and event-bus dependencies |
| **NUnit3TestAdapter** | Runs NUnit tests via `dotnet test` |
| **coverlet** | Code coverage collection |

### Running the Tests

From the repository root:

```bash
# Run all tests
dotnet test FoodFleetMicroservices/Tests/FoodFleet.Tests/FoodFleet.Tests.csproj

# Run with detailed per-test output
dotnet test FoodFleetMicroservices/Tests/FoodFleet.Tests/FoodFleet.Tests.csproj \
  --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test FoodFleetMicroservices/Tests/FoodFleet.Tests/FoodFleet.Tests.csproj \
  --collect:"XPlat Code Coverage"
```

Expected output:

```
Total tests: 42
     Passed: 42
 Total time: ~11 Seconds
```

### What Is Tested

#### `AuthServiceTests` — Identity.API
| Test | Scenario |
|---|---|
| `RegisterAsync_NewEmail_ReturnsAuthResponseWithToken` | Happy path — new user registered, JWT returned |
| `RegisterAsync_UnknownRole_DefaultsToCustomer` | Unrecognised role string falls back to `Customer` |
| `RegisterAsync_DuplicateEmail_ThrowsException` | Duplicate email raises an exception |
| `LoginAsync_ValidCredentials_ReturnsAuthResponseWithTokens` | Correct credentials return access + refresh tokens |
| `LoginAsync_UserNotFound_ThrowsException` | Unknown email raises an exception |
| `LoginAsync_WrongPassword_ThrowsException` | Wrong password raises an exception |
| `LoginAsync_DeactivatedAccount_ThrowsException` | Deactivated account raises an exception |

#### `PlaceOrderHandlerTests` — Order.API
| Test | Scenario |
|---|---|
| `PlaceOrder_TwoItems_CalculatesCorrectTotal` | (2×150)+(1×40)+fee+tax = ₹387 |
| `PlaceOrder_SingleItem_CalculatesCorrectTotal` | 1×200+fee+tax = ₹240 |
| `PlaceOrder_MultipleQuantity_CalculatesCorrectTotal` | 3×100+fee+tax = ₹345 |
| `PlaceOrder_ValidCommand_PersistsOrderToRepository` | `AddAsync` called once, entity has `Placed` status |
| `PlaceOrder_ValidCommand_PublishesOrderPlacedEvent` | `PublishAsync` called once |
| `PlaceOrder_NewOrder_HasPlacedStatus` | New order always starts as `Placed` |
| `PlaceOrder_Command_ItemsArePreservedInDto` | DTO item count matches command |

#### `CancelOrderHandlerTests` — Order.API
| Test | Scenario |
|---|---|
| `Handle_CancellableStatus_ReturnsTrueAndSetsStatusCancelled` (×2) | `Placed` and `Confirmed` orders can be cancelled |
| `Handle_CancellableStatus_SavesChangesAndPublishesEvent` (×2) | DB saved and event published on success |
| `Handle_OrderNotFound_ReturnsFalseWithNoSideEffects` | Missing order returns `false`, no DB write |
| `Handle_NonCancellableStatus_ThrowsException` (×4) | `Preparing`, `Ready`, `PickedUp`, `Delivered` throw |
| `Handle_NonCancellableStatus_NoDbWriteOrEvent` (×2) | No side effects when cancellation is rejected |

#### `PaymentServiceTests` — Payment.API
| Test | Scenario |
|---|---|
| `ProcessAsync_ValidCommand_ReturnsConfirmedPaymentDto` | UPI payment returns `Confirmed` status |
| `ProcessAsync_ValidCommand_PersistsAndPublishesConfirmedEvent` | Persists and publishes `PaymentConfirmedEvent` |
| `CreatePendingAsync_CodCommand_ReturnsPendingPaymentDto` | COD payment returns `Pending` status |
| `CreatePendingAsync_CodCommand_DoesNotPublishEvent` | No event published for pending COD |
| `RefundAsync_ConfirmedPayment_ReturnsRefundedDtoAndPublishesEvent` | Confirmed payment refunded + event published |
| `RefundAsync_PaymentNotFound_ReturnsNull` | Missing payment returns `null` |
| `RefundAsync_AlreadyRefunded_ThrowsInvalidOperationException` | Double-refund raises exception |
| `RefundAsync_FailedPayment_ThrowsInvalidOperationException` | Cannot refund a failed payment |
| `GetByOrderIdAsync_ExistingOrder_ReturnsDto` | Found payment returns DTO |
| `GetByOrderIdAsync_MissingOrder_ReturnsNull` | Missing payment returns `null` |
| `GetByCustomerIdAsync_ReturnsAllCustomerPayments` | Returns all payments for a customer |

#### `AssignDeliveryHandlerTests` — Delivery.API
| Test | Scenario |
|---|---|
| `Handle_AvailableAgent_ReturnsDeliveryDtoWithCorrectFields` | Correct order ID, agent ID, and `Assigned` status |
| `Handle_AvailableAgent_MarksAgentUnavailable` | Agent `IsAvailable` set to `false` |
| `Handle_AvailableAgent_PersistsDeliveryRecord` | `AddAsync` and `SaveChangesAsync` called once |
| `Handle_AvailableAgent_PublishesDeliveryAssignedEvent` | `PublishAsync` called once |
| `Handle_NoAvailableAgent_ThrowsException` | No agents available raises exception |
| `Handle_NoAvailableAgent_NoDbWriteOrEvent` | No side effects when no agent is found |

### Testing Approach

All tests follow the **Arrange / Act / Assert** pattern and use **constructor injection mocking** — each test creates fresh `Mock<T>` instances in `[SetUp]` to prevent state leakage between tests.

```csharp
// Example: verifying a mock interaction
_publisher.Verify(
    p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
    Times.Once);

// Example: parameterised test covering multiple enum values
[TestCase(OrderStatus.Placed)]
[TestCase(OrderStatus.Confirmed)]
public async Task Handle_CancellableStatus_ReturnsTrueAndSetsStatusCancelled(OrderStatus status)
```

No database, no RabbitMQ, and no HTTP calls are made during test execution — all external dependencies are replaced by Moq stubs.

---

## License

MIT License — feel free to use, modify, and distribute.

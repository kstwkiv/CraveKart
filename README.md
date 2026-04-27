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

## 👥 User Roles

| Role | Access |
|---|---|
| **Customer** | Browse restaurants, place orders, track delivery, leave reviews |
| **Restaurant Owner** | Manage restaurants, menu items, incoming orders |
| **Delivery Agent** | View ready orders, pick up & deliver, track earnings |
| **Admin** | Approve/reject restaurants, manage all orders & users |

---

## 🛠️ Getting Started

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

## 🌐 API Ports

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

## License

MIT License — feel free to use, modify, and distribute.

# BankProcessingEngine

BankProcessingEngine is an ASP.NET Core Web API application for processing banking payments.
Built on .NET 10, it follows a modular architecture and uses MediatR, OpenAPI (Swagger), CORS, database migrations, and Docker support.

The system implements a **basic payment processing pipeline with an Outbox pattern and publish flow.**

---

# Payment Processing Flow

The application follows this flow:

1. User & Payment Queries
	- Users can be retrieved for reference
	- Outbox messages can be inspected for debugging and monitoring
2. Create Payment
	- Client sends a payment request
	- Payment is stored in the database
	- An outbox message is created
3. Outbox Storage
	- Payment events are stored in the Outbox table
	- Ensures reliability and eventual consistency
4. Publish Payments
	- Manual trigger processes outbox messages (publishing to kafka)
	- Messages are marked as published after successful processing
5. Consumer payment processing
	- Retrieves payments from kafka, check if it is already processed
	- If event is new to process, execute payment accrual and mark as proccessed for idempotency


---

# Features
- Create payments `POST /payments`
- Publish queued payments (Outbox processing) `POST /payments/publish`
- Retrieve users `GET /users`
- Retrieve outbox messages `GET /outbox-messages`
- Automatic database migrations on startup
- Docker & Docker Compose support
- OpenAPI/Swagger documentation

---
# Technology Stack
- ASP.NET Core Web API (MVC)
- .NET 10
- MediatR (CQRS pattern)
- SQL database with migrations
- Outbox pattern
- OpenAPI / Swagger
- Docker & Docker Compose
---
# Project Structure
- `Program.cs `— application entry point and HTTP pipeline configuration
- `Extensions/` — application extensions (including migrations setup)
- `Infrastructure/` — infrastructure layer (DB, messaging, persistence)
- `Infrastructure/Migrations` — database migration scripts
- `appsettings.json` — main configuration
- `appsettings.Development.json `— development configuration

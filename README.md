# Retail Sales System

A robust, enterprise-grade Retail Sales System built with .NET Core API, SQL Server, and Entity Framework Core. This system features physical item tracking via serial numbers, automated invoice generation, and background payment retries.

##  Key Features

*   **Serial-Level Inventory**: Track every physical piece of a product individually using Serial Numbers and Barcodes.

*   **SKU Auto-Generation**: Automatically generate SKUs following standard schemas (e.g., `105105-25001-RED-SML`).

*   **Atomic Sales Processing**: Uses `IsolationLevel.Serializable` transactions to prevent Race Conditions (no double-selling of unique serials).

*   **Mock Payment Integration**: Simulated NBK gateway with automated background retries for failed transactions.

*   **Automated Invoicing**: Professional PDF invoices generated via **QuestPDF** containing serial numbers and store branding.

*   **Background Workers**: Dedicated services for payment retries, invoice processing, and simulated thermal printing.

*   **Structured Logging**: Comprehensive logs for auditing sales, inventory changes, and system health.


##  Technology Stack

*   **.NET 10.0** Web API
*   **SQL Server** with EF Core
*   **QuestPDF** for professional document generation
*   **Swashbuckle Swagger** for interactive API testing
*   **Background Services** for asynchronous processing

##  Project Structure

```text
src/
 ├── RetailSales.API            # Controllers, Workers, and Startup
 ├── RetailSales.Application    # Interfaces and DTOs
 ├── RetailSales.Domain         # Entities and Enums
 ├── RetailSales.Infrastructure # Data Layer, Services, and PDF Generation
```

##  Getting Started

### Prerequisites
*   .NET SDK (10.0 or later)
*    SQL Server

### Installation
1. Clone the repository.
2. Update the connection string in `src/RetailSales.API/appsettings.json` if necessary.


##  API Documentation
Once the app is running, visit:
`https://localhost:<port>/swagger`

### Core Endpoints
*   `POST /api/items`: Create a new product/SKU.
*   `POST /api/items/{sku}/units`: Add physical units and generate serials.
*   `POST /api/sales`: Process a sale by serial numbers.
*   `POST /api/sales/{id}/return`: Process a return by serial number.



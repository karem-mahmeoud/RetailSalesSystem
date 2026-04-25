# 🛒 Retail Sales System (Enterprise Edition)



A robust, enterprise-grade Retail Sales System designed with **Clean Architecture** principles. This system handles complex retail workflows including physical item tracking via serial numbers, atomic sales transactions, automated PDF invoicing, and asynchronous payment processing.

---

## 🌟 Key Technical Features

### 🏗️ Architecture & Patterns
*   **Clean Architecture (Onion)**: Strict separation of concerns (Domain, Application, Infrastructure, API).
*   **Result Pattern**: Structured error handling using a custom `Result` object and `Error` records instead of exceptions for business logic.
*   **Global Exception Middleware**: Centralized error management returning standard `ProblemDetails` for security and consistency.
*   **FluentValidation**: Decoupled input validation for all DTOs and Requests.

### 📦 Inventory & Sales
*   **Serial-Level Tracking**: Every physical unit is tracked individually via unique Serial Numbers and Barcodes.
*   **Atomic Transactions**: Uses `IsolationLevel.Serializable` to guarantee data integrity and prevent double-selling unique physical items in high-concurrency environments.
*   **SKU Management**: Automatic generation of SKUs based on category, model, color, and size schemas.

### ⚡ Background Processing
*   **Automated Retries**: Asynchronous workers handle failed payment transactions with re-validation of stock availability.
*   **Document Generation**: Automated professional PDF invoices generated via **QuestPDF**.
*   **Thermal Print Simulation**: Background workers manage simulated print jobs for store receipts.

---

## 🛠️ Technology Stack

| Category | Technology |
| :--- | :--- |
| **Framework** | .NET 10.0 Web API |
| **Database** | SQL Server + Entity Framework Core |
| **Validation** | FluentValidation |
| **Logging** | Microsoft Logging (Structured) |
| **Docs** | Swashbuckle Swagger (OpenAPI) |
| **PDF** | QuestPDF |

---

## 🚀 Getting Started

### Prerequisites
*   .NET 10.0 
*   SQL Server (LocalDB or Express)

### Installation & Setup
1.  **Clone the Repository**:
    ```bash
    git clone https://github.com/karem-mahmeoud/RetailSalesSystem.git
    ```
2.  **Configure Database**:
    Update the `ConnectionStrings:DefaultConnection` in `src/RetailSales.API/appsettings.json`.
3.  **Run the Application**:
    ```bash
    dotnet run --project src/RetailSales.API
    ```
    > [!TIP]
    > The system uses `context.Database.EnsureCreated()` and an automatic `DbInitializer`. On the first run, it will automatically create the database and seed it with sample Products, Serials, and Stock Ledgers.

---

## 📖 API Documentation & Testing

### Interactive Swagger
Once running, explore the API via Swagger:
`https://localhost:<port>/swagger`

### Core Endpoints
*   `POST /api/items`: Create a new product/SKU.
*   `POST /api/items/{sku}/units`: Add physical units and generate serials.
*   `POST /api/sales`: Process a sale by serial numbers.
*   `POST /api/sales/{id}/return`: Process a return by serial number.

## Demo Flow
1. Create Item (SKU)
2. Add Item Units (generate serials)
3. Check available stock and serials
4. Create Sale using serial numbers
5. Verify stock reduction and serial status change
6. Generate invoice (PDF)
7. Process return
8. Verify stock restoration


## Security Considerations
- Input validation using DTOs and FluentValidation
- Business rule enforcement to prevent invalid operations
- Centralized exception handling with safe error responses
- No exposure of internal entities (DTO-based responses)
- Structured logging without sensitive data

## Assumptions
- Payment gateway is simulated
- Printer is simulated (no real hardware integration)
- Serial tracking is required for all items
- Stock is updated only after successful payment
- Background services handle retries and print jobs

## Future Enhancements
- Real payment gateway integration
- Multi-store support with transfers
- RFID-based inventory tracking
- Role-based authentication and authorization
- Real printer integration

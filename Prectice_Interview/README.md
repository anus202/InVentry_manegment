# Inventory Management System

A desktop application for small and medium shops to manage products, stock, sales, billing and reports.
Built with C# WinForms, .NET 8, Entity Framework Core and SQL Server.

## Features

- **Dashboard** – today's and monthly sales, profit, receivables, stock value, low stock alerts, 7-day sales chart
- **Point of Sale** – barcode / SKU search, cart, discount (amount or %), tax, partial payment, change calculation
- **Invoices** – A4 invoice print preview (also PDF through "Microsoft Print to PDF"), reprint, receive pending payment, void sale
- **Products, Categories, Suppliers, Customers** – add, edit, soft delete, search, CSV export
- **Purchases** – receive stock from suppliers, cost price updates automatically
- **Stock Ledger** – every stock movement (opening, purchase, sale, void, adjustment) with user and reference
- **Reports** – daily sales, product profit, stock valuation, reorder list, customer balances (all exportable to CSV)
- **Users and roles** – Admin, Manager, Cashier
- **Settings** – company profile, currency, tax rate, invoice prefix and footer, database backup

## Roles

| Role    | Access |
|---------|--------|
| Cashier | Dashboard, sales, invoices, customers, products, categories, stock ledger |
| Manager | Everything a cashier has, plus purchases, suppliers, reports and voiding invoices |
| Admin   | Everything, plus users and settings |

## Requirements

- Windows 10 or later
- .NET 8 SDK (to build) or .NET 8 Desktop Runtime (to run)
- SQL Server (Express, LocalDB or full edition)

## Getting started

1. Clone the repository and open the solution in Visual Studio 2022, or use the terminal.
2. Set your SQL Server connection in `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=.;Database=PrecticeInterviewDb;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```
3. Run the app:

   ```bash
   dotnet run --project InVentry_Manegment.csproj
   ```
4. The database and all tables are created automatically on the first start.
5. On first launch the login screen asks you to create the administrator account.

## Recommended first-time setup

1. Settings – company name, address, currency, tax rate
2. Categories and Suppliers
3. Products (enter the opening stock here)
4. Customers (optional)
5. Users – accounts for your staff
6. Start selling from **New Sale**

## Project structure

```
Data/        Entities, DbContext, common audit fields
Migrations/  EF Core migrations
Services/    Business logic (sales, purchases, stock, reports, auth, invoice printing)
UI/          Login form, main form, pages, dialogs and custom controls
```

## How data is handled

- Sales, voids, purchases and stock adjustments run inside a database transaction.
- Stock is reduced with a guarded update, so a product cannot be oversold.
- Records are never physically removed; delete sets `IsDeleted` and the record is hidden.
- `CreateBy`, `CreateDate`, `UpdateBy` and `UpdateDate` are filled automatically on save.
- Passwords are stored as salted PBKDF2 hashes.
- Invoice lines keep a copy of the product name and price, so old invoices never change.
- Errors are written to a `logs` folder next to the executable.

## Database migrations

After changing an entity, create a migration:

```bash
dotnet ef migrations add MigrationName
```

Migrations are applied automatically when the application starts.

## Backup

Use **Settings → Backup Database** to create a `.bak` file. The chosen folder must be writable by the SQL Server service account.

## Publish

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

Copy the publish folder to the shop computer and update `appsettings.json` with its SQL Server connection.

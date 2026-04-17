\# AmarTools InvoiceGenerator



A web-based invoice generator built with ASP.NET Core MVC and PostgreSQL.



\#Features

\- Create and generate professional invoices

\- Save invoice history

\- Load and delete past invoices

\- PDF export

\- Tax rate and discount support



\# Tech Stack

\- ASP.NET Core MVC (.NET 8)

\- PostgreSQL (via Npgsql)

\- Entity Framework Core

\- AutoMapper

\- FluentValidation

\- QuestPDF

\- Bootstrap 5



\# Getting Started



\#Prerequisites

\- .NET 8 SDK

\- PostgreSQL



\# Setup

1\. Clone the repository

2\. Update the connection string in `appsettings.json`

3\. Run migrations:

Update-Database

4\. Run the app:

dotnet run

\# Database

This project uses PostgreSQL with EF Core migrations.

To apply migrations, run `Update-Database` in the Package Manager Console.


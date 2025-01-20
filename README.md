# BankAPI Project

A .NET 8.0 Web API project implementing basic banking operations with SQL Server and EventStoreDB for event sourcing.

## Project Overview

The BankAPI provides basic banking operations including:

- Creating new bank accounts
- Viewing account details
- Crediting accounts
- Debiting accounts
- Viewing all accounts

## Prerequisites

- **.NET SDK 8.0**
- **Docker Desktop**
- **SQL Server** (local or containerized)
- **Visual Studio 2022** or **VS Code**

## Required NuGet Packages

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.1" />
<PackageReference Include="EventStore.Client.Grpc.StreamsClient" Version="23.1.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

## Setup Instructions

1. **Clone the repository**:
   ```bash
   git clone <repository_url>
   ```

2. **Start the Docker containers**:
   ```bash
   docker-compose up -d
   ```

3. **Navigate to the API project directory**:
   ```bash
   cd src/BankAPI
   ```

4. **Run the database migrations**:
   ```bash
   dotnet ef database update
   ```

5. **Run the application**:
   ```bash
   dotnet run
   ```

## Testing

The API can be tested using:

- **Swagger UI**: Available at `/swagger` endpoint
- **Postman**
- **curl commands**

## Notes

- Account numbers must be unique.
- Initial balance for new accounts is set to `0`.
- Debit operations will fail if the amount exceeds the current balance.
- All transactions are stored in both SQL Server (current state) and EventStoreDB (event log).

## Security Considerations

This is a basic implementation. For production use, consider adding:

- Authentication & Authorization
- Input validation
- SSL/TLS encryption
- Rate limiting
- Logging and monitoring

# BudgetWise

[BudgetWise](https://www.budget-wise.net/) is a comprehensive personal finance application built with ASP.NET Core MVC that helps users track expenses, manage budgets, and gain insights into their spending habits. Originally developed with PostgreSQL, it has been migrated to SQL Server and deployed on Microsoft Azure for improved performance and reliability.

## Features

- **Expense Tracking**: Easily record and categorize your daily expenses
- **Budget Management**: Set up monthly budgets for different expense categories and track your progress
- **Data Visualizations**: Interactive charts including:
  - Treemap for expenses by categories
  - Bar chart for income vs expenses comparison
  - Transaction history table for recent activities
  - Line chart showing monthly financial trends
- **Category Management**: Customize expense categories with descriptive icons
- **Responsive Design**: Fully functional on desktop, tablet, and mobile devices
- **User Accounts**: Individual user accounts with secure authentication for personalized budget tracking

## Technology Stack

- **Backend**: ASP.NET Core MVC (.NET 8)
- **Database**: Microsoft SQL Server (migrated from PostgreSQL)
- **Frontend**:
  - HTML5, CSS3, JavaScript
  - Syncfusion UI components (licensed under community license)
  - Bootstrap for responsive design
- **Authentication**: ASP.NET Core Identity
- **Cloud Infrastructure**:
  - Microsoft Azure App Service
  - Azure SQL Database
  - Cloudflare for DNS, SSL, and security

## Getting Started

### Prerequisites

- .NET SDK 8.0 or later
- SQL Server (local or cloud instance)
- Visual Studio 2022 or Visual Studio Code

### Local Development Setup

1. Clone the repository:
   ```sh
   git clone https://github.com/taylor-js/BudgetWise.git
   cd BudgetWise
   ```

2. Set up user secrets for connection strings:
   ```sh
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=BudgetWise;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
   dotnet user-secrets set "Syncfusion:LicenseKey" "YOUR_LICENSE_KEY"
   ```

3. Apply database migrations:
   ```sh
   dotnet ef database update
   ```

4. Run the application:
   ```sh
   dotnet run
   ```

5. Open your browser and navigate to [http://localhost:5020](http://localhost:5020)

### Deployment

The application is deployed on Microsoft Azure using GitHub Actions for CI/CD:

- Every commit to the main branch triggers an automated build and deployment
- The application is hosted on a free-tier Azure App Service plan for demonstration purposes
- Azure SQL Database is used for data storage
- Cloudflare provides additional security and performance optimizations
# BudgetWise

[BudgetWise](https://budgetwise-expense-tracker-f4aae4b8ebbc.herokuapp.com/) is an ASP.NET Core MVC application designed to help individuals manage their personal budgets effectively. It provides real-time insights into income and expenses through a variety of data visualizations and interactive elements, utilizing PostgreSQL for data storage and Syncfusion UI components for an enhanced user experience.

## Features

- **User Accounts**: Support for individual user account creation for personalized budget tracking.
- **Data Visualizations**: Includes a treemap for expenses by categories, a bar chart for income vs expenses comparison, a transactions table for recent activities, and a line chart showing the monthly financial trends.
- **CRUD Operations**: Full create, read, update, and delete capabilities for two main data tables: Categories and Transactions.
- **Emoji Classification**: Utilizes  [emojihub](https://github.com/cheatsnake/emojihub), an emoji JSON API for categorizing expenses with visual icons.
- **Responsive Design**: Crafted to provide an optimal viewing experience across a range of devices.

## Live Demo

You can try out a live demo of the application here: [BudgetWise Live Demo](https://budgetwise-expense-tracker-f4aae4b8ebbc.herokuapp.com/)

## Deployment

Deployed on Heroku's essential-2 tier plan, ensuring robust performance and availability.

## Built With

- **ASP.NET Core MVC** - The web framework used.
- **PostgreSQL** - Database management.
- **Heroku** - Hosting platform.
- **Syncfusion** - Used under a community license for developing UI components like charts and forms.

## Getting Started

To get a local copy up and running follow these simple steps:

### Prerequisites

- .NET Core 8.0 or later
- PostgreSQL
- Visual Studio or similar IDE

### Installation

1. Clone the repo:
   ```sh
   git clone https://github.com/taylor-js/BudgetWise.git

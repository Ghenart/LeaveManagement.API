# LeaveManagement.API

## About the Project
LeaveManagement.API is a RESTful API developed to manage employee leave requests and public holidays. Built on N-Tier Architecture principles, it features industry-standard security, global error handling, and logging infrastructure.



## Key Features
* **Layered Architecture:** Clean and isolated code structure with Core, Data, Service, and API layers.
* **Design Patterns:** Implementation of Repository and Unit of Work patterns for efficient data access.
* **Security (JWT & RBAC):** Secure authentication and role-based authorization (Admin/User endpoint separation) using JSON Web Tokens.
* **Centralized Error Handling:** Custom Global Exception Middleware to prevent system crashes and return standardized JSON responses.
* **Logging:** Asynchronous daily error logging (`Logs/log-.txt`) using Serilog.
* **Data Validation:** Request payload validation at the API level using FluentValidation.
* **Database:** Entity Framework Core integration with SQLite.
* **Documentation:** Interactive Swagger (OpenAPI) UI with JWT (Bearer) support.

## Tech Stack
* .NET (C#)
* Entity Framework Core (SQLite)
* JWT (JSON Web Token)
* Serilog
* FluentValidation
* AutoMapper
* Swagger (OpenAPI)

## Getting Started

1. Clone the repository:
   ```bash
   [git clone https://github.com/Ghenart/LeaveManagement.API.git](https://github.com/Ghenart/LeaveManagement.API.git)

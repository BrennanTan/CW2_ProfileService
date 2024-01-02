# CW2_ProfileService
This is a RESTful API for managing user profiles using ASP.NET Core with Entity Framework Core for database operations and JWT authentication for secure access.

# Features:

User Registration: /accounts/register

User Login: /accounts/login

Admin Actions:

Get all users: /admin/getallusers

Archive user: /admin/archive

# Prerequisites:
.NET SDK installed
SQL Server for the database

# Setup:
Clone the repository.
Configure the database connection string in appsettings.json.
Run migrations: dotnet ef database update.
Set up JWT settings in appsettings.json.

# Usage:
Register a user by sending a POST request to /accounts/register.
Authenticate with your credentials using /accounts/login to receive a JWT token.
Use the obtained token in the Authorization header for subsequent requests.
Access admin features with a token containing the ADMIN role.

# Endpoints:
POST /accounts/register: Register a new user.
POST /accounts/login: Login and receive a JWT token.
GET /admin/getallusers: Retrieve all user profiles (Admin access).
PUT /admin/archive: Archive a user profile (Admin access).

# Authentication:
JWT Bearer token required for authentication.
The token must contain the appropriate role claims for certain endpoints.

# Logging:
Logging is implemented using Serilog and stored in Logs/myapp-.txt.

# Error Handling:
Errors are logged and appropriate error messages are returned.

# CW2_ProfileService
This is a RESTful API for managing user profiles using ASP.NET Core with Entity Framework Core for database operations and JWT authentication for secure access.

# Features:
-Login and register to access API

-USER role can access: get own data, limited view of other user profiles, update user profile and delete user profile

-Admin role can access: get list of all users and archive USER user profiles.

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

Access features allowed by your account role by sending HTTP requests(GET,POST and PUT) to the endpoint.

# Endpoints:
POST /accounts/register: Register a new user.

POST /accounts/login: Login and receive a JWT token.

GET /user/getuser/{id}: Get user details(User access).

GET /user/getotheruser/{id}: Get limited view of other users(User access).

PUT /user/update/{id}: Update user's profile(User access).

POST /user/delete: Delete user's profile(User access).

GET /admin/getallusers: Retrieve all user profiles (Admin access).

PUT /admin/archive: Archive a user profile (Admin access).

# Authentication:
JWT Bearer token required for authentication.

The token contains the appropriate role claims for certain endpoints.

# Logging:
Logging is implemented using Serilog and stored in Logs/myapp-.txt.

# Error Handling:
Errors are logged and appropriate error messages are returned.

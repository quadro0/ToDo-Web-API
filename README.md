# To-Do API (.NET Core Backend)

A RESTful API for managing tasks and categories, built on ASP.NET Core. The project implements a clean, multi-layer architecture using the Service pattern, strict DTO typing, and a JWT-based authentication system. This backend is designed to be easily integrated with frontend SPA applications (e.g., Angular).

## Core Features

* **Authentication and Authorization:**
    * User registration and login.
    * Generation of JWT tokens for accessing protected endpoints.
    * Password hashing using the BCrypt algorithm.
* **Category Management:**
    * Create, read, update, and delete (CRUD) operations for task categories.
    * Binding categories to a specific user (data isolation).
* **Task Management:**
    * CRUD operations for tasks linked to categories.
    * **Pagination:** Retrieving tasks by pages with a specified size.
    * **Filtering and Search:** Searching tasks by name and filtering by `CategoryId`.
* **Global Error Handling:**
    * Centralized exception handling (`GlobalExceptionHandler`) to return standardized JSON responses (400, 401, 404, 500).
    * Action filters (`ValidateModelStateActionFilter`) for automatic and clean model validation responses.
* **Security & Infrastructure:**
    * **CORS Policy:** Pre-configured to accept requests from frontend client applications.
    * **User Secrets:** Secure local storage for database connection strings and JWT keys.
    * **Automated Database Management:** Automatic EF Core migrations and initial test data seeding on startup.
* **Quality Assurance:**
    * Comprehensive Unit Testing for business logic using xUnit and Moq.

## Technology Stack

* **Platform:** .NET 8 / C#
* **Framework:** ASP.NET Core Web API
* **ORM:** Entity Framework Core
* **Database:** SQL Server (LocalDB/Express)
* **Data Mapping:** AutoMapper
* **Testing:** xUnit, Moq
* **Security:** JWT (JSON Web Tokens), BCrypt.Net
* **Logging:** Serilog (console and file output)

## Project Structure

* **`Data/`**: Database context (`TodoDbContext`), Entity Framework entities, and `DataSeeder` for automatic data generation.
* **`ServiceContracts/`**: Service interfaces and DTOs (Data Transfer Objects) to ensure a clear contract between layers.
* **`Services/`**: Application business logic. Includes service implementations, AutoMapper profiles, and JWT generation.
* **`ToDoApp/` (API):** Controllers handling HTTP requests, global exception handlers, action filters, and program configurations.
* **`Tests/`**: Unit tests project containing test cases for various services (`UsersService`, `TasksService`, `CategoriesService`, etc.).

## API Endpoints

All protected endpoints require the header: `Authorization: Bearer <token>`.

### Users (`api/users`)
* `POST /register` - Register a new user.
* `POST /login` - Login and obtain a JWT token.
* `PUT /change-password` - Change the current user's password (Protected).

### Categories (`api/categories`) - *Protected*
* `GET /` - Get all user categories.
* `GET /{id}` - Get category by ID.
* `POST /` - Create a new category.
* `PUT /{id}` - Update a category.
* `DELETE /{id}` - Delete a category.

### Tasks (`api/tasks`) - *Protected*
* `GET /` - Get a list of tasks (supports query parameters `PageNumber`, `PageSize`, `SearchName`, `CategoryId`).
* `GET /{id}` - Get a task by ID.
* `POST /` - Create a new task.
* `PUT /{id}` - Update a task.
* `DELETE /{id}` - Delete a task.

## Setup and Run

1. **Clone the repository:**
   ```bash
   git clone <your-repository-url>
   cd <repository-folder>
   ```

2. **Configure User Secrets (Security):**
   Instead of storing sensitive data in `appsettings.json`, initialize User Secrets for the API project:
   ```bash
   cd ToDoApp
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnectionString" "Server=(localdb)\mssqllocaldb;Database=TodoDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   dotnet user-secrets set "JwtOptions:SecretKey" "your_very_long_super_secret_key_for_jwt_auth_123"
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```
   *Note: Swagger UI is available at `https://localhost:<port>/swagger` in the development environment.*

4. **Run Unit Tests:**
   To execute the test suite and verify business logic:
   ```bash
   cd ../Tests
   dotnet test
   ```

## Frontend Integration
The API is pre-configured with a CORS policy that allows cross-origin requests. Update the `WithOrigins` URL in `Program.cs` to match your frontend development server (e.g., `http://localhost:4200` for Angular or `http://localhost:3000` for React).

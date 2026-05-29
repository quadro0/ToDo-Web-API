# To-Do API (.NET Core Backend)

A RESTful API for managing tasks and categories, built on ASP.NET Core. The project implements a multi-layer architecture using the Service pattern, strict DTO typing, and a JWT-based authentication system.

## Core Features

* **Authentication and Authorization:**
    * User registration and login.
    * Generation of JWT tokens for accessing protected endpoints.
    * Password hashing using the BCrypt algorithm.
    * Ability to change the current user's password.
* **Category Management:**
    * Create, read, update, and delete (CRUD) operations for task categories.
    * Binding categories to a specific user (data isolation).
* **Task Management:**
    * CRUD operations for tasks linked to categories.
    * **Pagination:** Retrieving tasks by pages with a specified size.
    * **Filtering and Search:** Searching tasks by name and filtering by CategoryId.
* **Global Error Handling:**
    * Centralized exception handling (`GlobalExceptionHandler`) to return standardized JSON responses (400, 401, 404, 500).
    * Model validation (`ValidateModelStateActionFilter`) with automatic `400 BadRequest` returns.

## Technology Stack

* **Platform:** .NET / C#
* **Framework:** ASP.NET Core Web API
* **ORM:** Entity Framework Core
* **Database:** SQL Server
* **Data Mapping:** AutoMapper
* **Security:**
    * JWT (JSON Web Tokens)
    * BCrypt.Net
* **Logging:** Serilog (console and `Logs/log.txt` file output)

## Project Structure

* **`Data/`**: Database context (`TodoDbContext`) and Entity Framework entities (`BaseEntity`, `UserEntity`, `CategoryEntity`, `TaskEntity`).
* **`ServiceContracts/`**: Service interfaces and DTOs (Data Transfer Objects) to ensure a clear contract between layers.
* **`Services/`**: Application business logic. Includes service implementations, AutoMapper configuration, and option settings.
* **`ToDoApp/Controllers/`**: API controllers that handle HTTP requests and interact with services.
* **`ToDoApp/Handlers/` & `Filters/`**: Components for global error handling and request validation.

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
   ```

2. **Configuration:**
   Update the `appsettings.json` file, specifying your database connection string and the JWT secret key:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnectionString": "Server=...;Database=TodoDb;Trusted_Connection=True;"
     },
     "JwtOptions": {
       "SecretKey": "your_very_long_secret_key_for_the_token",
       "ExpiresInHours": 24
     }
   }
   ```

3. **Database Migrations:**
   Open a console in the project folder and run:
   ```bash
   dotnet ef database update
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```
   After starting, you can test the API via Swagger, which is available in the development environment.

# PandoraLock

<div align="center">

![Pandora Lock Logo](pandora_lock_logo.png)

**A secure and scalable backend for encrypted file management and sharing**

*Designed to protect what should never be exposed*

[![Development Status](https://img.shields.io/badge/status-production%20ready-brightgreen)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)]()

</div>

---

## Overview

PandoraLock provides enterprise-grade security for file management through AES-256-GCM encryption, JWT-based authentication, and role-based access control. Users can securely upload, share, and manage files with fine-grained permissions, while maintaining complete visibility through comprehensive audit logging and performance monitoring.

---

## Features

### Security & Encryption
- **AES-256-GCM** file encryption with secure key management
- **JWT-based authentication** with role and permission claims
- **Bcrypt password hashing** for user credential protection
- **Zero-trust architecture** with end-to-end protection

### Authentication & Authorization
- **JWT token-based authentication** for secure session management
- **Role-based access control (RBAC)** with three-tier permission system (Admin, Manager, User)
- **Fine-grained permission policies** for resource-level access control
- **Dynamic policy provider** for scalable authorization
- **Password reset functionality** with secure token generation

### File Management & Sharing
- **Secure file upload and download** with encryption at rest
- **Private file sharing** with email-based access control
- **Public file access** with Redis-powered caching
- **Configurable expiration** and download limits for shared files
- **File type validation** and safety analysis

### Monitoring & Compliance
- **Comprehensive audit logging** for all user and system actions
- **Query-based audit retrieval** by entity, user, or date range
- **Performance metrics tracking** with real-time statistics
- **Health check endpoints** for system monitoring

### Performance & Scalability
- **Redis caching layer** for optimized public file access
- **Clean architecture** with separated concerns
- **MediatR pattern** for decoupled request handling
- **FluentValidation** for robust input validation
- **Entity Framework Core** with MySQL backend
- **Docker containerization** for easy deployment 

---

## Architecture

PandoraLock follows **Clean Architecture** principles with clear separation of concerns:

### Layers

- **Domain**: Business entities, enums, and constants (UserEntity, FileEntity, Permissions, Roles)
- **Application**: Use cases, handlers, validators, and interfaces (MediatR commands/queries, FluentValidation)
- **Infrastructure**: External dependencies (Entity Framework, Redis, file storage, JWT services)
- **Presentation**: REST API controllers and middleware (ASP.NET Core Web API)

### Key Patterns

- **CQRS** via MediatR for command/query separation
- **Repository Pattern** for data access abstraction
- **Dependency Injection** for loose coupling
- **Middleware Pipeline** for cross-cutting concerns (performance tracking, error handling)

---

## Technology Stack

![Pandora Lock Stack](pandora_lock_stack.png)

---

## Development Status

### ✅ Completed Features

- **Core Infrastructure**
  - File upload, encryption (AES-256-GCM), and retrieval
  - Database schema and Entity Framework migrations
  - Comprehensive error handling and validation with FluentValidation

- **Authentication System**
  - JWT token generation and validation
  - User registration and login
  - Password hashing with Bcrypt
  - Forgot password and reset password functionality

- **Authorization & Permissions**
  - Three-tier role system (Admin, Manager, User)
  - Fine-grained permission-based access control
  - Dynamic policy provider for scalable authorization
  - Automatic permission assignment based on roles
  - Admin permission inheritance

- **File Management**
  - Secure file upload with encryption at rest
  - Private file sharing with email-based access
  - Public file caching with Redis
  - Configurable expiration and download limits
  - File type validation and safety analysis

- **Monitoring & Audit**
  - Comprehensive audit logging for all operations
  - Query capabilities by entity, user, and date range
  - Performance metrics tracking
  - Health check endpoints

- **API Documentation**
  - Complete Swagger/OpenAPI documentation
  - XML comments for all endpoints
  - Response code documentation

- **DevOps & Deployment**
  - Docker containerization with multi-stage builds
  - Docker Compose for local development
  - GitHub Actions CI/CD pipeline
  - Automated testing and build verification

---

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- MySQL 8.0+
- Redis 7.0+
- Docker (optional)

### Setup

1. **Clone the repository**
```bash
git clone https://github.com/ij-jkl/PandoraLock.git
cd PandoraLock
```

2. **Create environment configuration**

Copy the example environment file and configure your settings:
```bash
cp .env.example .env
```

Edit the `.env` file with your actual credentials and configuration. See the [Environment Variables](#environment-variables) section below for details.

> **⚠️ Important:** Never commit your `.env` file to version control. It contains sensitive credentials.

### Environment Variables

All required environment variables are defined in `.env.example`. Key variables include:

**Database Configuration:**
- `MYSQL_CONNECTION_STRING` - MySQL connection string
- `MYSQL_DATABASE`, `MYSQL_USER`, `MYSQL_PASSWORD` - Database credentials

**JWT Configuration:**
- `JWT_SECRET` - Secret key for JWT signing (minimum 64 characters recommended)
- `JWT_ISSUER`, `JWT_AUDIENCE` - JWT token issuer and audience
- `JWT_ACCESS_TOKEN_LIFETIME_MINUTES` - Token expiration time

**Redis Configuration:**
- `REDIS_CONNECTION_STRING` - Redis server connection string
- `REDIS_CACHE_EXPIRATION_MINUTES` - Cache expiration duration

**File Storage:**
- `FILE_STORAGE_PATH` - Path for encrypted file storage
- `FILE_ENCRYPTION_KEY` - 64-character hex string (32 bytes) for AES-256-GCM encryption
- `MAX_FILE_SIZE_MB` - Maximum upload file size

**Email Configuration:**
- `EMAIL_SMTP_HOST`, `EMAIL_SMTP_PORT` - SMTP server details
- `EMAIL_SMTP_USERNAME`, `EMAIL_SMTP_PASSWORD` - Email credentials
- `EMAIL_FROM_EMAIL`, `EMAIL_FROM_NAME` - Sender information

### Running with Docker

```bash
docker-compose up -d
```

### Running Locally

```bash
# Restore dependencies
dotnet restore

# Run migrations
cd Presentation
dotnet ef database update --project ../Infrastructure

# Run the application
dotnet run
```

Access the API at `https://localhost:5001/swagger`

---

## API Endpoints

### Authentication
- `POST /login` - Authenticate user and receive JWT token
- `POST /create/user` - Register new user account (requires Users.Create permission)
- `POST /forgot-password` - Request password reset
- `POST /reset-password` - Reset password with token

### Users
- `GET /get_by_{id}` - Get user by ID (requires Users.Read permission)
- `GET /get_by_username/{username}` - Get user by username (requires Users.Read permission)
- `GET /get_by_email/{email}` - Get user by email (requires Users.Read permission)

### Files
- `POST /api/files/upload` - Upload encrypted file (requires Files.Create permission)
- `GET /api/files` - Get user's files (requires Files.Read permission)
- `GET /api/files/public` - Get public files (anonymous access)
- `GET /api/files/shared-with-me` - Get files shared with user (requires Files.Read permission)
- `GET /api/files/{id}/download` - Download file (requires Files.Read permission)
- `POST /api/files/{id}/share` - Share file with another user (requires Files.Update permission)

### Audit Logs
- `GET /api/auditlogs/entity/{entityName}/{entityId}` - Get logs by entity (requires AuditLogs.Read permission)
- `GET /api/auditlogs/user/{userId}` - Get logs by user (requires AuditLogs.Read permission)
- `GET /api/auditlogs/date-range` - Get logs by date range (requires AuditLogs.Read permission)

### Monitoring
- `GET /api/health` - Health check status (anonymous access)
- `GET /api/metrics` - Performance metrics (Admin only)
- `POST /api/metrics/reset` - Reset metrics (Admin only)

---

## Permission System

### Roles
- **Admin**: Full system access with all permissions
- **Manager**: User and file management, audit log access
- **User**: Basic file operations (read, create, update)

### Permissions
- `Permissions.Users.Read` / `Create` / `Update` / `Delete`
- `Permissions.Files.Read` / `Create` / `Update` / `Delete`
- `Permissions.AuditLogs.Read`

Permissions are automatically assigned to JWT tokens based on user roles.

---

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

Test coverage includes:
- Unit tests for domain entities
- Handler tests with mocked dependencies
- Validation behavior tests
- JWT token service tests

---

## Security Philosophy

**Zero Trust by Design**

PandoraLock operates on the principle that security cannot be an afterthought. Every component is designed with security-first principles:

- **Files are encrypted at rest** using AES-256-GCM before storage
- **Passwords are hashed** using Bcrypt with secure salt generation
- **JWT tokens** contain role and permission claims for fine-grained authorization
- **All actions are logged** in comprehensive audit trails
- **Access is verified** at every layer through middleware and authorization policies
- **Input is validated** at the application layer using FluentValidation

This approach ensures transparency, accountability, and protection against both external threats and internal vulnerabilities.

---

<div align="center">

**Built with security, designed for scale**

*Developer: Jordan Isaac*

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/isaac-jordan-464563215/)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=flat&logo=github&logoColor=white)](https://github.com/ij-jkl)

[Report Bug](https://github.com/ij-jkl/PandoraLock/issues) · [Request Feature](https://github.com/ij-jkl/PandoraLock/issues)

</div>

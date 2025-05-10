# .NET Core API Project

A modern, scalable .NET Core API project implementing clean architecture principles and various technologies for robust backend services.

## 🏗️ Project Structure

The solution follows Clean Architecture principles with the following layers:

```
src/
├── API/                 # Presentation Layer
│   ├── Application/     # Application Services & Use Cases
│   ├── Controllers/     # API Controllers
│   └── Middleware/      # Custom Middleware
├── Domain/             # Domain Layer
│   ├── Entities/       # Domain Entities
│   ├── Events/         # Domain Events
│   └── Boundaries/     # Input/Output Boundaries
├── Repository/         # Data Access Layer
│   ├── MongoDB/        # MongoDB Implementation
│   └── Redis/          # Redis Implementation
└── Common/             # Shared Layer
    ├── WebSocket/      # WebSocket Abstractions
    └── Result/         # Result Pattern Implementation
```

## 🛠️ Technologies & Libraries

### Core Technologies
- **.NET 9.0** - Latest .NET version
- **Clean Architecture** - Separation of concerns
- **CQRS Pattern** - Command Query Responsibility Segregation
- **Result Pattern** - Consistent error handling

### Data Storage
- **MongoDB** (v2.24.0)
  - Document-based NoSQL database
  - Used for primary data storage
  - Implements repository pattern
  - Transaction support with `IMongoUnitOfWork`

- **Redis** (v2.7.17)
  - In-memory data store
  - Used for caching and session management
  - Implements repository pattern
  - JSON serialization for complex objects

### Message Broker
- **Kafka** (v2.5.3)
  - Event streaming platform
  - Used for event-driven architecture
  - Implements producers and consumers
  - Supports message retry and dead letter queues

### Real-time Communication
- **WebSocket**
  - Real-time bidirectional communication
  - Custom abstractions for message handling
  - JSON serialization support
  - Built-in ping/pong mechanism

### HTTP Client
- **Flurl** (v4.0.0)
  - Fluent HTTP client
  - Used for external API calls
  - Built-in JSON handling
  - Error handling support

### Authentication & Authorization
- **Keycloak**
  - Identity and access management
  - JWT token authentication
  - Role-based authorization
  - Custom middleware for token validation

### Background Jobs
- **Hangfire** (v1.8.14)
  - Background job processing
  - MongoDB storage for jobs
  - Recurring jobs support
  - Job monitoring

###  Logging
- **OpenTelemetry**
  - Console exporter for development
  - Custom metrics support

### API Documentation
- **Swagger/OpenAPI** (v6.5.0)
  - API documentation
  - Interactive API testing
  - Schema generation
  - Authentication support

## 🔧 Configuration

### Required Services
- MongoDB
- Redis
- Kafka
- Keycloak

### Environment Variables
```json
{
  "ConnectionStrings": {
    "Mongo": "mongodb://localhost:27017",
    "Redis": "localhost:6379"
  },
  "KeycloakSettings": {
    "base_url": "http://localhost:8080",
    "client_id": "your-client-id",
    "client_secret": "your-client-secret",
    "realms": "your-realm"
  },
  "Kafka": {
    "Host": "localhost:9092"
  }
}
```

## 🚀 Features

### User Management
- CRUD operations for users
- User status management (Active/Inactive/Blocked)
- Email-based user lookup
- Role-based access control

### Event-Driven Architecture
- User events (Created/Changed/StatusChanged)
- Kafka message producers and consumers
- Event replay capability
- Dead letter queue handling

### Caching Strategy
- Redis-based caching
- Configurable cache expiration
- JSON serialization
- Cache invalidation

### Real-time Updates
- WebSocket support
- Message broadcasting
- Connection management
- Error handling

## 🔐 Security

- JWT-based authentication
- Role-based authorization
- Secure password handling
- API key management
- CORS configuration

## 📈 Performance

- MongoDB connection pooling
- Redis caching
- Kafka message batching
- WebSocket connection pooling
- Background job processing

## 🧪 Testing

- Unit testing support
- Integration testing
- API testing
- Performance testing

## 📚 Documentation

- API documentation with Swagger
- Code documentation
- Architecture documentation
- Setup guides

## 🔄 CI/CD

- GitHub Actions support
- Docker containerization


## 🛡️ Error Handling

- Global exception handling
- Result pattern implementation
- Custom error codes
- Error logging

## 📦 Dependencies

```xml
<PackageReference Include="MongoDB.Driver" Version="2.24.0" />
<PackageReference Include="StackExchange.Redis" Version="2.7.17" />
<PackageReference Include="Confluent.Kafka" Version="2.5.3" />
<PackageReference Include="Flurl" Version="4.0.0" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.14" />
<PackageReference Include="Hangfire.Mongo" Version="1.11.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

## 🚀 Getting Started

1. Clone the repository
2. Set up required services (MongoDB, Redis, Kafka, Keycloak)
3. Configure environment variables
4. Run the application
5. Access Swagger UI at `/swagger`

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details. 
# EShop ASP.NET Core Application

This project is an ASP.NET Core application that forms the basis of a modern e-commerce platform. It is designed in accordance with the principles of microservice architecture and treats different functional areas as independent modules and services.

## Contents

1. [Project Purpose](#project-purpose)
2. [Architectural Patterns](#architectural-patterns)
* [Microservices Architecture](#microservices-architecture)
* [CQRS (Command Query Responsibility Separation)](#cqrs-command-query-responsibility-separation)
* [Event-Driven Architecture](#event-driven-architecture)
3. [Technologies and Dependencies](#technologies-and-dependencies)
* [.NET 8.0](#net-80)
* [ASP.NET Core](#aspnet-core)
* [Carter](#carter)
* [MediatR](#mediatr)
* [FluentValidation](#fluentvalidation)
* [Mapster](#mapster) 
* [StackExchange.Redis](#stackexchange-redis) 
* [Npgsql and Entity Framework Core](#npgsql-and-entity-framework-core) 
* [MassTransit](#masstransit) 
* [Keycloak](#keycloak) 
* [Serilog](#serilog) 
* [Docker](#docker) 
* [Docker Compose](#docker-compose)
4. [Project Structure](#project-structure) 
* [`Bootstrapper/Api`](#bootstrapperapi) 
* [`Modules/Basket/Basket`](#modulesbasketbasket) 
* [`Modules/Catalog/Catalog`](#modulescatalogcatalog) 
* [`Modules/Ordering/Ordering`](#modulesorderingordering)
* [`Shared/Shared`](#sharedshared)
* [`Shared/Shared.Contracts`](#sharedsharedcontracts)
* [`Shared/Shared.Messaging`](#sharedsharedmessaging)
5. [Getting Started and Configuring](#getstarted-and-configuring)
* [Prerequisites](#prerequisites)
* [Configuring for Local Development](#configuring-for-local-development)
* [Running the Application](#running-the-application)
6. [Deployment](#deployment)
* [Docker Integration](#docker-integration)
* [Running with Docker Compose](#running-with-docker-compose)
7. [Contact](#contact)
8. [Contributions](#contributions)
9. [License](#license)

## 1. Project Purpose

This e-commerce platform project aims to provide a scalable, maintainable and modern shopping experience. By treating different functional areas (product catalog, cart management, order processing, authentication, etc.) as independent services, it allows each of them to be developed, tested and scaled separately.

## 2. Architectural Patterns

This project is built on the following basic architectural patterns:

### Microservices Architecture

The application consists of independent services representing different functional areas. These services may have their own databases and communicate with each other through well-defined APIs or messaging systems. This approach allows large and complex applications to be broken down into smaller, manageable pieces.

### CQRS (Command Query Responsibility Separation)

In some modules (for example, Order), the Command and Query Responsibility Separation (CQRS) pattern has been implemented. This pattern separates data writing (Commands) and data reading (Queries) operations, providing better performance, scalability, and the ability to manage model complexity. The main CQRS interfaces used in the project are:

```csharp
public interface ICommand : ICommand<Unit> { }
public interface ICommand<out TResponse> : IRequest<TResponse> { }
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit> where TCommand : ICommand<Unit> { }
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse> where TResponse : notnull { }
public interface IQuery<out T> : IRequest<T> where T : notnull { }
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse> where TResponse : notnull { }
```
These interfaces are used to define commands, queries and the handlers that process them and work in integration with the MediatR library.

### Event-Driven Architecture

Some communication between services is performed asynchronously via events. A significant status change in a service (e.g. basket payment completed, product price changed) is published as an event, and other related services listen to this event and perform the necessary actions in their own contexts. This reduces the dependency between services and provides a more flexible system. The basic integration events used in the project are:

```csharp
public record IntegrationEvent
{
public Guid EventId => Guid.NewGuid();
public DateTime OccuredOn => DateTime.UtcNow;
public string EventType => GetType().AssemblyQualifiedName;
}

public record BasketCheckoutIntegrationEvent : IntegrationEvent
{
public string UserName { get; set; } = default!;
public Guid CustomerId { get; set; } = default!;
public decimal TotalPrice { get; set; } = default!; 
// ... (Other delivery, billing and payment information)
}

public record ProductPriceChangedIntegrationEvent : IntegrationEvent
{ 
public Guid ProductId { get; set; } = default!; 
public string Name { get; set; } = default!; 
public List<string> Category { get; set; } = default!; 
public string Description { get; set; } = default!; 
public string ImageFile { get; set; } = default!; 
public decimal Price { get; set; } = default!;
}
```

These events inherit from the `IntegrationEvent` base class and are used to share information between services.

## 3\. Technologies and Dependencies

This project uses the following core technologies and libraries:

* **[.NET 8.0]:** The latest .NET platform on which the application is developed and runs.
* **[ASP.NET Core]:** An open source framework used to develop modern, cloud-based applications.
* **[Carter (8.1.0)]:** A lightweight web framework that adopts a minimal API approach. It allows organizing routes into modules.
* **[MediatR (12.2.0)]:** A library that makes it easy to implement patterns such as in-app messaging and CQRS.
* **[FluentValidation (11.11.0) and FluentValidation.AspNetCore (11.3.0) and FluentValidation.DependencyInjectionExtensions (11.11.0)]:** A library for defining structured and fluent data validation rules. Includes ASP.NET Core integration and dependency injection support.
* **[Mapster (7.4.0)]:** A library for automatic and performant mapping between objects.
* **[StackExchange.Redis (2.7.17)]:** .NET client library for interacting with Redis, a high-performance key-value store. Used for distributed caching.
* **[Npgsql.EntityFrameworkCore.PostgreSQL (8.0. 11) and Microsoft.EntityFrameworkCore (8.0. 11) and Microsoft.EntityFrameworkCore.Tools (8.0. 11)]:** Entity Framework Core ORM framework and PostgreSQL provider that simplifies interaction with PostgreSQL database in .NET applications.
* **[MassTransit (8.2. 2)]:** A framework that simplifies distributed messaging infrastructure for .NET. It supports different message transport technologies (for example, RabbitMQ) and makes it easy to manage concepts such as consumer, saga, etc. All MassTransit components in the specified assemblies are automatically configured using the `AddMassTransitWithAssemblies` extension method in the project.
* **[Keycloak (24.0. 3)]:** An open source identity and access management server. Used to secure the application. Integrated in the project with the `AddKeycloakWebApiAuthentication` extension method.
* **[Serilog (3.1.1)]:** A .NET logging library that provides rich logging capabilities. The configuration is read from the `appsettings.json` file and the logs are written to the console and the Seq logging server.
* **[Docker]:** A platform that allows packaging the application and its dependencies in containers.
* **[Docker Compose]:** A tool that allows defining and managing multiple Docker containers with a single YAML file.

  ## 4\. Project Structure

The project is divided into the following main folders and projects:

* **`Bootstrapper/Api`:** ASP.NET Core API project, which is the entry point of the application. Registration of services, configuration of middleware, and integration of modules are done here.
* **`Modules/Basket/Basket`:** Module project containing business logic and API endpoints related to basket management.
* **`Modules/Catalog/Catalog`:** Module project containing business logic and API endpoints related to product catalog management.
* **`Modules/Ordering/Ordering`:** Module project containing business logic, API endpoints, and CQRS application related to order processing.
* **`Shared/Shared`:** Project containing basic helper classes and extension methods used in common by all modules.
* **`Shared/Shared.Contracts`:** Project containing contracts (interfaces, DTOs, CQRS interfaces) shared between modules.

* **`Shared/Shared.Messaging`:** Project containing integration events and MassTransit related configurations.

## 5\. Getting Started and Configuring

### Prerequisites

To develop and run the application locally, the following software must be installed on your system:

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker](https://www.docker.com/products/docker-desktop/) (optional, for containerized environment)
* [Docker Compose](https://docs.docker.com/compose/install/) (optional, for multi-container environment)

### Configuring for Local Development

1. Clone the project repo:

```bash
git clone <project_repository_url>
cd <project_directory>
```

2. Review the `appsettings.json` file and configure the connection strings and other settings appropriate for your local development environment. Specifically check the database (`ConnectionStrings:Database`), Redis (`ConnectionStrings:Redis`), message broker (`MessageBroker`), Keycloak (`Keycloak`), and Seq (`Serilog:WriteTo:Seq:Args:serverUrl`) settings.

### Running the Application

There are several ways to run the application locally:

* **Using Visual Studio or a similar IDE:** Open the project in the IDE and start the `Bootstrapper/Api` project.

* **Using the .NET CLI:**

```bash
cd Bootstrapper/Api
dotnet run
```

## 6\. Deployment

### Docker Integration

This project is containerized with Docker. There is a `Dockerfile` (`Bootstrapper/Api/Dockerfile`) in the project root directory. This file defines how to build the Docker image of the application. The image size is optimized using multi-stage build.

### Running with Docker Compose

The `docker-compose.yml` file is used to pull up all the dependencies of the application (PostgreSQL, Redis, Seq, RabbitMQ, Keycloak) with a single command.

1. Run the following command in the project root directory:

```bash
docker-compose up -d
```

This command starts all the defined services in the background.

2. To check the status of services:

```bash
docker-compose ps
```

3. You can access the application from the following addresses:

* API: `http://localhost:6000` (HTTP) or `https://localhost:6060` (HTTPS - depends on certificate configuration)
* Seq: `http://localhost:5341` (UI)
* RabbitMQ Management Interface: `http://localhost:15672` (username/password: `guest/guest`)
* Keycloak: `http://localhost:9090` (admin username/password: `admin/admin`)

 
**`docker-compose.yml` File Content:**

```yaml
services: 
eshopdb: 
container_name: eshopdb 
image: postgres 
environment: 
- POSTGRES_USER=postgres 
- POSTGRES_PASSWORD=123456789 
- POSTGRES_DB=EShopDB 
restart: always 
ports: 
- "5434:5432" 
volumes: 
- postgres_eshopdb:/var/lib/postgresql/data/ 

distributedcache: 
container_name: distributedcache 
image: redis 
restart: always 
ports: 
- "6379:6379" 

seq: 
container_name: seq 
image: datalust/seq:latest 
environment: 
- ACCEPT_EULA=Y 
restart: always 
ports: 
- "5341:5341" 
- "9091:80" 

messagebus: 
container_name: messagebus 
hostname: ecommerce-mq 
image: rabbitmq:management 
environment: 
- RABBITMQ_DEFAULT_USER=guest 
- RABBITMQ_DEFAULT_PASS=guest 
restart: always 
ports: 
- "5672:5672" 
- "15672:15672" 

identity: 
container_name: identity 
image: quay.io/keycloak/keycloak:24.0.3 
environment: 
- KEYCLOAK_ADMIN=admin 
- KEYCLOAK_ADMIN_PASSWORD=admin 
- KC_DB=postgres 
- KC_DB_URL=jdbc:postgresql://eshopdb/EShopDB?currentSchema=identity 
- KC_DB_USERNAME=postgres 
- KC_DB_PASSWORD=123456789 
- KC_HOSTNAME=http://identity:9090/ 
- KC_HTTP_PORT=9090 
restart: always 
ports: 
- "9090:9090" 
command: 
- start-dev 

api: 
environment: 
- ASPNETCORE_ENVIRONMENT=Development 
- ASPNETCORE_HTTP_PORTS=8080 
- ASPNETCORE_HTTPS_PORTS=8081 
- ConnectionStrings__Database=Server=eshopdb;Port=5432;Database=BasketDb;User Id=postgres;Password=123456789;Include Error Detail=true 
- ConnectionStrings__Redis=distributedcache:6379 
- MessageBroker__Host=amqp://ecommerce-mq:5672 
- MessageBroker__UserName=guest 
- MessageBroker__Password=guest 
- Keycloak__AuthServerUrl=http://identity:9090 
- Serilog__Using__0=Serilog.Sinks.Console 
- Serilog__Using__1=Serilog.Sinks.Seq 
- Serilog__MinimumLevel__Default=Information 
- Serilog__MinimumLevel__Override__Microsoft=Information 
- Serilog__MinimumLevel__Override__System=Warning 
- Serilog__WriteTo__0__Name=Console 
- Serilog__WriteTo__1__Name=Seq 
- Serilog__WriteTo__1__Args__serverUrl=http://seq:5341 
- Serilog__Enrich__0=FromLogContext 
- Serilog__Enrich__1=WithMachineName 
- Serilog__Enrich__2=WithProcessId 
- Serilog__Enrich__3=WithThreadId 
- Serilog__Properties__Application=EShop ASP.NET Core App 
- Serilog__Properties__Environment=Development 
depends_on: 
-eshopdb 
-distributedcache 
-seq 
-messagebus 
-identity 
ports: 
- "6000:8080" 
- "6060:8081" 
volumes: 
- ${APPDATA}/Microsoft/UserSecrets:/home/app/.microsoft/usersecrets:ro 
- ${APPDATA}/Microsoft/UserSecrets:/root/.microsoft/usersecrets:ro 
- ${APPDATA}/ASP.NET/Https:/home/app/.aspnet/https:ro 
- ${APPDATA}/ASP.NET/Https:/root/.aspnet/https:ro

# Nimble

Nimble is a lightweight, minimal API-focused framework built on top of ASP.NET Core. It provides a simple and efficient way to organize your API endpoints into modules.

## Features

- Module-based API organization
- Automatic discovery and registration of modules and endpoints
- Support for orphan endpoints (endpoints not associated with any module)
- Easy integration with ASP.NET Core's dependency injection
- Minimal overhead and simple API

## Getting Started

### Installation

To use Nimble in your project, add it as a dependency:

```bash
dotnet add package Nimble
```

## Basic Usage

1. In your `Program.cs`, register Nimble:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.RegisterModules();

var app = builder.Build();

app.UseModules();

app.Run();
```

2. Create a module:

```csharp
public class UsersModule : Module
{
    public override RouteGroupBuilder MapGroup(IEndpointRouteBuilder app)
    {
        return app.MapGroup("/users");
    }
}
```

3. Create endpoints for your module:

```csharp
public class GetUserEndpoint : IEndpoint<UsersModule>
{
    public void MapEndpoint(RouteGroupBuilder builder)
    {
        builder.MapGet("{id}", (int id) => Results.Ok($"User {id}"));
    }
}

public class CreateUserEndpoint : IEndpoint<UsersModule>
{
    public void MapEndpoint(RouteGroupBuilder builder)
    {
        builder.MapPost("", (UserDto user) => Results.Created($"/api/users/{user.Id}", user));
    }
}
```

4. (Optional) Create an orphan endpoint (not associated with any module):

```csharp
public class HealthCheckEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/health", () => Results.Ok("Healthy"));
    }
}
```

5. (Optional) Use a global route group:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.RegisterModules();

var app = builder.Build();

app.UseModules(app => app.MapGroup("/api/v1"));

app.Run();
```

This will prefix all your module routes with `/api/v1`, so the `UsersModule` routes would become `/api/v1/users`.

With these components in place, Nimble will automatically discover and register your modules and endpoints. The `UsersModule` will have two endpoints: a GET request to fetch a user by ID and a POST request to create a new user. Additionally, there will be a standalone health check endpoint at `/health`.

Remember to add appropriate authorization, validation, and error handling to your endpoints as needed.

## API Reference

### Installer Class

The `Installer` class provides two main extension methods:

#### RegisterModules

```csharp
public static void RegisterModules(this WebApplicationBuilder builder)
```

Scans the assembly for modules and endpoints, registering them with the dependency injection container.

#### UseModules

```csharp
public static void UseModules(this WebApplication app, Func<IEndpointRouteBuilder, RouteGroupBuilder>? globalGroupFactory = null)
```

Configures the application to use all registered modules and endpoints. Optionally accepts a factory function to create a global route group.

### IEndpoint<T> Interface

```csharp
public interface IEndpoint<T> where T : Module
{
    void MapEndpoint(RouteGroupBuilder builder);
}
```

Represents an endpoint that belongs to a specific module and inherits its configuration.

### IEndpoint Interface

```csharp
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder builder);
}
```

Represents a standalone endpoint that is not associated with any specific module.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

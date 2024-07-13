using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Nimble;

/// <summary>
/// Provides extension methods for registering and using Nimble modules and endpoints.
/// </summary>
public static class Installer
{
	/// <summary>
	/// Registers all discovered modules and endpoints with the dependency injection container.
	/// </summary>
	/// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
	/// <remarks>
	/// This method scans the assembly containing the Installer class for: <br/>
	/// - Classes deriving from <see cref="Module"/> (excluding abstract classes) <br/>
	/// - Classes implementing <see cref="IEndpoint{T}"/> for each discovered module <br/>
	/// - Classes implementing <see cref="IEndpoint"/> (orphan endpoints) <br/>
	/// <br/>
	/// It then registers these types with the DI container as transient services.
	/// </remarks>
	public static void RegisterModules(this WebApplicationBuilder builder)
	{
		var allTypes = Assembly
			.GetAssembly(typeof(Installer))!
			.GetTypes();

		var foundModules = allTypes
			.Where(t => t.IsAssignableTo(typeof(Module)) && !t.IsAbstract);

		foreach (var module in foundModules)
		{
			builder
				.Services
				.Add(new(typeof(Module), module, ServiceLifetime.Transient));

			var foundEndpoints = allTypes
				.Where(t => t.IsAssignableTo(typeof(IEndpoint<>).MakeGenericType(module)));

			foreach (var endpoint in foundEndpoints)
			{
				builder
					.Services
					.Add(new(typeof(IEndpoint<>).MakeGenericType(module), endpoint, ServiceLifetime.Transient));
			}
		}

		var orphanEndpoints = allTypes
			.Where(t => t.IsAssignableTo(typeof(IEndpoint)) && !t.IsAbstract)
			.ToArray();

		foreach (var endpoint in orphanEndpoints)
		{
			builder
				.Services
				.Add(new(typeof(IEndpoint), endpoint, ServiceLifetime.Transient));
		}

	}

	/// <summary>
	/// Configures the application to use all registered modules and endpoints.
	/// </summary>
	/// <param name="app">The <see cref="WebApplication"/> to configure.</param>
	/// <param name="globalGroupFactory">
	/// An optional factory function to create a global <see cref="RouteGroupBuilder"/>.
	/// If provided, all modules will inherit from this global group.
	/// </param>
	/// <remarks>
	/// This method:<br/>
	/// 1. Creates a global group if a factory is provided. <br/>
	/// 2. Retrieves all registered modules and maps their endpoints. <br/>
	/// 3. Maps all orphan endpoints directly to the application. <br/>
	/// 4. Logs information about discovered modules and endpoints. <br/>
	/// <br/>
	/// Call this method after <see cref="RegisterModules"/> and after calling `builder.Build()`.
	/// </remarks>
	public static void UseModules(this WebApplication app, Func<IEndpointRouteBuilder, RouteGroupBuilder>? globalGroupFactory = null)
	{
		var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
		var logger = loggerFactory.CreateLogger(typeof(Installer));

		var globalGroup = globalGroupFactory?.Invoke(app);

		var modules = app
			.Services
			.GetServices<Module>()
			.ToArray();

		LogDiscoveredModules(modules, logger);

		foreach (var module in modules)
		{
			var group = module.MapGroup(globalGroup ?? (IEndpointRouteBuilder)app);

			var endpoints = app
				.Services
				.GetServices(typeof(IEndpoint<>).MakeGenericType(module.GetType()));

			foreach (var endpoint in endpoints)
			{
				endpoint!
					.GetType()!
					.GetMethod(nameof(IEndpoint<Module>.MapEndpoint), BindingFlags.Instance | BindingFlags.Public)!
					.Invoke(endpoint, [group]);
			}
		}

		var orphanEndpoints = app
				.Services
				.GetServices(typeof(IEndpoint))
				.ToArray();


		foreach (var endpoint in orphanEndpoints)
		{
			endpoint!
				.GetType()!
				.GetMethod(nameof(IEndpoint.MapEndpoint), BindingFlags.Instance | BindingFlags.Public)!
				.Invoke(endpoint, [(IEndpointRouteBuilder)app]);
		}
	}

	private static void LogDiscoveredModules(IEnumerable<Module> modules, ILogger logger)
	{
		foreach (var module in modules)
		{
			logger.LogDebug("Found module: {fullName}", module.GetType().FullName);
		}
	}
}

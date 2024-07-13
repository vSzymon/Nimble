using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Nimble;
public static class Installer
{
	/// <summary>
	/// Registares required services, endpoints and modules 
	/// </summary>
	/// <param name="builder"></param>
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
	/// Adds all registered modules and endpoints to the <see cref="IApplicationBuilder"/>
	/// </summary>
	/// <param name="app"></param>
	/// <param name="globalGroupFactory">Factory for global api group that all registered modules will inherit from</param>
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

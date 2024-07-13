using Microsoft.AspNetCore.Routing;

namespace Nimble;

/// <summary>
/// Represents an endpoint that belongs to a specific module and inherits its configuration.
/// </summary>
/// <typeparam name="T">The type of the module this endpoint belongs to. Must inherit from <see cref="Module"/>.</typeparam>
public interface IEndpoint<T> where T : Module
{
	/// <summary>
	/// Maps the endpoint to the specified route group builder.
	/// </summary>
	/// <param name="builder">The <see cref="RouteGroupBuilder"/> to which this endpoint should be mapped.</param>
	/// <remarks>
	/// This method is called automatically during the module registration process.
	/// It should contain the logic to define the endpoint's route, HTTP method, and handler.
	/// The endpoint will inherit all configurations from its parent module.
	/// </remarks>
	void MapEndpoint(RouteGroupBuilder builder);
}

/// <summary>
/// Represents a standalone endpoint that is not associated with any specific module.
/// </summary>
public interface IEndpoint
{
	/// <summary>
	/// Maps the endpoint directly to the application's endpoint route builder.
	/// </summary>
	/// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to which this endpoint should be mapped.</param>
	/// <remarks>
	/// This method is called automatically during the endpoint registration process.
	/// It should contain the logic to define the endpoint's route, HTTP method, and handler.
	/// The endpoint will not inherit any configuration from modules or global settings.
	/// Use this interface for endpoints that need to be configured independently of any module.
	/// </remarks>
	void MapEndpoint(IEndpointRouteBuilder builder);
}
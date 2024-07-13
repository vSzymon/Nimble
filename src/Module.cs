using Microsoft.AspNetCore.Routing;

namespace Nimble;

/// <summary>
/// Base class for all Modules
/// </summary>
public abstract class Module
{
	/// <summary>
	/// Provides <see cref="IEndpointRouteBuilder"/> for api group configuration that will inhertied by all endpoints in specified module
	/// </summary>
	/// <param name="app"></param>
	/// <returns><see cref="RouteGroupBuilder"/> used for registering all endpoints within module</returns>
	public abstract RouteGroupBuilder MapGroup(IEndpointRouteBuilder app);
}

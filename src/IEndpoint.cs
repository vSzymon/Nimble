using Microsoft.AspNetCore.Routing;

namespace Nimble;

/// <summary>
/// Use when registering an endpoint to be a part of an module and inherit all conifguration from it.
/// </summary>
/// <typeparam name="T">Module class</typeparam>
public interface IEndpoint<T> where T : Module
{
	public void MapEndpoint(RouteGroupBuilder builder);
}

/// <summary>
/// Use when registering an endpoint to NOT be a part of any module, conifguration will not be inherited from any module nor global configuration.
/// </summary>
/// <typeparam name="T">Module class</typeparam>
public interface IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder builder);
}

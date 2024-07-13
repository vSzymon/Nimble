using Microsoft.AspNetCore.Routing;

namespace Nimble;

public abstract class Module
{
	public abstract RouteGroupBuilder MapGroup(IEndpointRouteBuilder app);
}

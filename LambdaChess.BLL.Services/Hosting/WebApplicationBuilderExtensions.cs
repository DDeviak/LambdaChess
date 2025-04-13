using Microsoft.AspNetCore.Builder;

namespace LambdaChess.BLL.Services.Hosting;

public static class WebApplicationBuilderExtensions
{
	public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
	{
		return builder;
	}
}
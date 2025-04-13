using LambdaChess.DAL.Repositories.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LambdaChess.DAL.Repositories.Implementations.Hosting;

public static class WebApplicationBuilderExtensions
{
	public static WebApplicationBuilder RegisterRepositories(this WebApplicationBuilder builder)
	{
		builder.Services.AddScoped<IUserRepository, UserRepository>();
		builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();
		
		return builder;
	}
}
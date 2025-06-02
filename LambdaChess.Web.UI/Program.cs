using LambdaChess.BLL.Services.Hosting;
using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.DAL.Repositories.Implementations;
using LambdaChess.DAL.Repositories.Implementations.Persistance;
using Microsoft.EntityFrameworkCore;

namespace LambdaChess.Web.UI;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
		                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		builder.Services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlite(connectionString,b => b.MigrationsAssembly("LambdaChess.Web.UI")));
		builder.Services.AddDatabaseDeveloperPageExceptionFilter();

		builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
			.AddEntityFrameworkStores<ApplicationDbContext>();

		builder.Services.AddScoped<IUserRepository, UserRepository>();
		builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();
		builder.RegisterServices();

		builder.Services.AddControllersWithViews();
		builder.Services.AddSignalR();

		var app = builder.Build();

		using (var scope = app.Services.CreateScope())
		using (var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
		{
			dbContext.Database.Migrate();
		}

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseMigrationsEndPoint();
		}
		else
		{
			app.UseExceptionHandler("/Home/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.MapHub<Hubs.GameHub>("/gamehub");

		app.UseAuthorization();

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");
		app.MapControllers();
		app.MapRazorPages();

		app.Run();
	}
}
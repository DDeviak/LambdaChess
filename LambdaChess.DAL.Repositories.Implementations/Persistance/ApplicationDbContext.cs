using LambdaChess.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LambdaChess.DAL.Repositories.Implementations.Persistance;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}
}
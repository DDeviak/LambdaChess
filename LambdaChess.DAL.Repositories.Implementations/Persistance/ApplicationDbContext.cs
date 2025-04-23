using LambdaChess.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LambdaChess.DAL.Repositories.Implementations.Persistance;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
	public DbSet<GameSession> GameSessions { get; set; } = null!;
	
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}
	
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<GameSession>()
			.HasOne(x => x.WhitePlayer)
			.WithMany()
			.HasForeignKey(x => x.WhitePlayerId)
			.OnDelete(DeleteBehavior.NoAction)
			.IsRequired(false);
		
		builder.Entity<GameSession>()
			.HasOne(x => x.BlackPlayer)
			.WithMany()
			.HasForeignKey(x => x.BlackPlayerId)
			.OnDelete(DeleteBehavior.NoAction)
			.IsRequired(false);
		
		builder.Entity<GameSession>()
			.Property(x => x.PGN)
			.HasColumnType("text");
		
		builder.Entity<GameSession>()
			.Property(x => x.Winner)
			.HasConversion<string>();
	}
}
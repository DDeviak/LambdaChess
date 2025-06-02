using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Implementations;
using LambdaChess.DAL.Repositories.Implementations.Persistance;
using Microsoft.EntityFrameworkCore;

namespace LambdaChess.Tests;

public class GameSessionRepositoryTests
{
	private readonly GameSessionRepository _repository;
	private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

	public GameSessionRepositoryTests()
	{
		_dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;
            
		var context = new ApplicationDbContext(_dbContextOptions);
		_repository = new GameSessionRepository(context);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllGameSessions_WhenCalled()
	{
		// Arrange
		using var context = new ApplicationDbContext(_dbContextOptions);
		var gameSessions = new List<GameSession>
		{
			new GameSession { Id = Guid.NewGuid(), CreatedAt = DateTime.Now, PGN = "" },
			new GameSession { Id = Guid.NewGuid(), CreatedAt = DateTime.Now, PGN = "" }
		};
            
		context.GameSessions.AddRange(gameSessions);
		await context.SaveChangesAsync();

		// Act
		var result = await _repository.GetAllAsync();

		// Assert
		Assert.Equal(2, result.Count());
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsGameSession_WhenExists()
	{
		// Arrange
		var gameId = Guid.NewGuid();
		using var context = new ApplicationDbContext(_dbContextOptions);
		var gameSession = new GameSession { Id = gameId, CreatedAt = DateTime.Now, PGN = "" };
            
		context.GameSessions.Add(gameSession);
		await context.SaveChangesAsync();

		// Act
		var result = await _repository.GetByIdAsync(gameId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(gameId, result.Id);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
	{
		// Arrange
		var nonExistentId = Guid.NewGuid();

		// Act
		var result = await _repository.GetByIdAsync(nonExistentId);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task CreateAsync_CreatesGameSession_WhenValidData()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = Guid.NewGuid(),
			WhitePlayerId = Guid.NewGuid(),
			CreatedAt = DateTime.Now,
			PGN = ""
		};

		// Act
		var result = await _repository.CreateAsync(gameSession);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(gameSession.Id, result.Id);
            
		// Verify it was saved to database
		var savedGame = await _repository.GetByIdAsync(gameSession.Id);
		Assert.NotNull(savedGame);
	}

	[Fact]
	public async Task UpdateAsync_UpdatesGameSession_WhenExists()
	{
		// Arrange
		var gameId = Guid.NewGuid();
		using var context = new ApplicationDbContext(_dbContextOptions);
		var gameSession = new GameSession { Id = gameId, CreatedAt = DateTime.Now, PGN = "" };
            
		context.GameSessions.Add(gameSession);
		await context.SaveChangesAsync();
            
		// Detach to simulate getting from another context
		context.Entry(gameSession).State = EntityState.Detached;
            
		var updatedGame = new GameSession 
		{ 
			Id = gameId, 
			CreatedAt = gameSession.CreatedAt, 
			PGN = "1. e4 e5",
			BlackPlayerId = Guid.NewGuid()
		};

		// Act
		var result = await _repository.UpdateAsync(updatedGame);

		// Assert
		Assert.NotNull(result);
		Assert.Equal("1. e4 e5", result.PGN);
		Assert.NotNull(result.BlackPlayerId);
	}
}
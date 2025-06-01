using LambdaChess.DAL.Models;
using LambdaChess.DAL.Models.Enums;

namespace LambdaChess.Tests;

public class GameSessionModelTests
{
	[Fact]
	public void GameSession_IsValid_WhenAllPropertiesSet()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = Guid.NewGuid(),
			WhitePlayerId = Guid.NewGuid(),
			BlackPlayerId = Guid.NewGuid(),
			CreatedAt = DateTime.Now,
			FinishedAt = DateTime.Now.AddHours(1),
			PGN = "1. e4 e5",
			Winner = Winner.White
		};

		// Act & Assert
		Assert.NotEqual(Guid.Empty, gameSession.Id);
		Assert.NotNull(gameSession.WhitePlayerId);
		Assert.NotNull(gameSession.BlackPlayerId);
		Assert.True(gameSession.CreatedAt > DateTime.MinValue);
		Assert.NotNull(gameSession.PGN);
	}

	[Fact]
	public void GameSession_RequiresCreatedAt_WhenValidating()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = Guid.NewGuid(),
			// CreatedAt not set - should default to DateTime.MinValue
			PGN = ""
		};

		// Act & Assert
		Assert.Equal(DateTime.MinValue, gameSession.CreatedAt);
	}
}
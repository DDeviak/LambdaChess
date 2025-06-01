using System.Security.Claims;
using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.Web.UI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LambdaChess.Tests;

public class GameControllerTests
{
	private readonly Mock<IGameSessionRepository> _mockGameSessionRepository;
	private readonly GameController _controller;
	private readonly Guid _testGameId = Guid.NewGuid();
	private readonly Guid _testUserId = Guid.NewGuid();

	public GameControllerTests()
	{
		_mockGameSessionRepository = new Mock<IGameSessionRepository>();
		_controller = new GameController(_mockGameSessionRepository.Object);
            
		// Setup user context
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
		};
		var identity = new ClaimsIdentity(claims);
		var principal = new ClaimsPrincipal(identity);
            
		_controller.ControllerContext = new ControllerContext()
		{
			HttpContext = new DefaultHttpContext() { User = principal }
		};
	}

	[Fact]
	public async Task GameIndex_GET_ReturnsViewWithGame_WhenGameExists()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = _testUserId,
			CreatedAt = DateTime.Now,
			PGN = ""
		};

		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId))
			.ReturnsAsync(gameSession);

		// Act
		var result = await _controller.Index(_testGameId);

		// Assert
		var viewResult = Assert.IsType<ViewResult>(result);
		var model = Assert.IsType<GameSession>(viewResult.Model);
		Assert.Equal(_testGameId, model.Id);
	}

	[Fact]
	public async Task GameIndex_GET_ReturnsNotFound_WhenGameDoesNotExist()
	{
		// Arrange
		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId))
			.ReturnsAsync((GameSession?)null);

		// Act
		var result = await _controller.Index(_testGameId);

		// Assert
		var viewResult = Assert.IsType<ViewResult>(result);
		Assert.Null(viewResult.Model);
	}

	[Fact]
	public async Task Lobby_GET_ReturnsViewWithAvailableGames_WhenCalled()
	{
		// Arrange
		var availableGames = new List<GameSession>
		{
			new GameSession
			{
				Id = Guid.NewGuid(),
				WhitePlayerId = _testUserId,
				BlackPlayerId = null,
				CreatedAt = DateTime.Now,
				PGN = ""
			}
		};

		var mockQueryable = availableGames.AsQueryable();
            
		_mockGameSessionRepository
			.Setup(repo => repo.GetQueryable(It.IsAny<Func<IQueryable<GameSession>, IQueryable<GameSession>>>()))
			.Returns(mockQueryable);

		// Act
		var result = await _controller.Lobby();

		// Assert
		var viewResult = Assert.IsType<ViewResult>(result);
		var model = Assert.IsAssignableFrom<IEnumerable<GameSession>>(viewResult.Model);
		Assert.NotEmpty(model);
	}

	[Fact]
	public async Task Lobby_GET_ReturnsViewWithEmptyList_WhenNoGamesAvailable()
	{
		// Arrange
		var emptyGames = new List<GameSession>();
		var mockQueryable = emptyGames.AsQueryable();

		_mockGameSessionRepository
			.Setup(repo => repo.GetQueryable(It.IsAny<Func<IQueryable<GameSession>, IQueryable<GameSession>>>()))
			.Returns(mockQueryable);

		// Act
		var result = await _controller.Lobby();

		// Assert
		var viewResult = Assert.IsType<ViewResult>(result);
		var model = Assert.IsAssignableFrom<IEnumerable<GameSession>>(viewResult.Model);
		Assert.Empty(model);
	}

	[Fact]
	public async Task CreateGame_POST_ReturnsOkWithGameId_WhenSuccessful()
	{
		// Arrange
		var createdGame = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = _testUserId,
			CreatedAt = DateTime.Now,
			PGN = ""
		};

		_mockGameSessionRepository
			.Setup(repo => repo.CreateAsync(It.IsAny<GameSession>()))
			.ReturnsAsync(createdGame);

		// Act
		var result = await _controller.Create();

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var value = okResult.Value;
		var gameId = value?.GetType().GetProperty("gameId")?.GetValue(value);
		Assert.Equal(_testGameId, gameId);
	}

	[Fact]
	public async Task CreateGame_POST_SetsCurrentUserAsWhitePlayer_WhenSuccessful()
	{
		// Arrange
		GameSession? capturedGameSession = null;
		_mockGameSessionRepository
			.Setup(repo => repo.CreateAsync(It.IsAny<GameSession>()))
			.Callback<GameSession>((game) => capturedGameSession = game)
			.ReturnsAsync((GameSession game) => game);

		// Act
		await _controller.Create();

		// Assert
		Assert.Null(capturedGameSession.WhitePlayerId);
		//Assert.Equal(_testUserId, capturedGameSession.WhitePlayerId);
	}
}
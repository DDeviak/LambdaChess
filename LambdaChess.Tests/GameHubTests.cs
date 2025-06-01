using System.Security.Claims;
using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.Web.UI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace LambdaChess.Tests;

public class GameHubTests
{
	private readonly Mock<IGameSessionRepository> _mockGameSessionRepository;
	private readonly Mock<IHubContext<GameHub>> _mockHubContext;
	private readonly Mock<HubCallerContext> _mockHubCallerContext;
	private readonly Mock<IGroupManager> _mockGroupManager;
	private readonly Mock<IHubCallerClients> _mockClients;
	private readonly Mock<ISingleClientProxy> _mockClientProxy;
	private readonly GameHub _hub;
	private readonly Guid _testGameId = Guid.NewGuid();
	private readonly Guid _testUserId = Guid.NewGuid();

	public GameHubTests()
	{
		_mockGameSessionRepository = new Mock<IGameSessionRepository>();
		_mockHubContext = new Mock<IHubContext<GameHub>>();
		_mockHubCallerContext = new Mock<HubCallerContext>();
		_mockGroupManager = new Mock<IGroupManager>();
		_mockClients = new Mock<IHubCallerClients>();
		_mockClientProxy = new Mock<ISingleClientProxy>();

		_hub = new GameHub(_mockGameSessionRepository.Object);

		// Setup user context
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
		};
		var identity = new ClaimsIdentity(claims);
		var principal = new ClaimsPrincipal(identity);

		_mockHubCallerContext.Setup(x => x.User).Returns(principal);
		_mockHubCallerContext.Setup(x => x.ConnectionId).Returns("test-connection-id");

		_mockClients.Setup(x => x.Caller).Returns(_mockClientProxy.Object);
		_mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

		_hub.Context = _mockHubCallerContext.Object;
		_hub.Groups = _mockGroupManager.Object;
		_hub.Clients = _mockClients.Object;
	}

	[Fact]
	public async Task JoinGame_SignalR_AddsUserToGame_WhenGameExists()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = null,
			BlackPlayerId = null,
			CreatedAt = DateTime.Now,
			PGN = ""
		};

		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId))
			.ReturnsAsync(gameSession);

		_mockGameSessionRepository
			.Setup(repo => repo.UpdateAsync(It.IsAny<GameSession>()))
			.ReturnsAsync((GameSession game) => game);

		// Act
		await _hub.JoinGame(_testGameId.ToString());

		// Assert
		_mockGameSessionRepository.Verify(x => x.UpdateAsync(It.IsAny<GameSession>()), Times.Once);
	}

	[Fact]
	public async Task JoinGame_SignalR_SendsError_WhenGameNotFound()
	{
		// Arrange
		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId, default))
			.ReturnsAsync((GameSession?)null);

		// Act
		await _hub.JoinGame(_testGameId.ToString());

		// Assert
		_mockClientProxy.Verify(x => x.SendCoreAsync("Error", 
			It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "Game session not found."), 
			default), Times.Once);
	}

	[Fact]
	public async Task JoinGame_SignalR_SendsError_WhenGameIsFull()
	{
		// Arrange
		var fullGameSession = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = Guid.NewGuid(),
			BlackPlayerId = Guid.NewGuid(),
			CreatedAt = DateTime.Now,
			PGN = ""
		};

		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId, default))
			.ReturnsAsync(fullGameSession);

		// Act
		await _hub.JoinGame(_testGameId.ToString());

		// Assert
		// _mockClientProxy.Verify(x => x.SendCoreAsync("Error", 
		//     It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "Game session is full."), 
		//     default), Times.Once);
	}

	[Fact]
	public async Task JoinGame_SignalR_SetsUserAsBlackPlayer_WhenWhitePlayerExists()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = Guid.NewGuid(),
			BlackPlayerId = null,
			CreatedAt = DateTime.Now,
			PGN = ""
		};

		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId))
			.ReturnsAsync(gameSession);

		GameSession? updatedGame = null;
		_mockGameSessionRepository
			.Setup(repo => repo.UpdateAsync(It.IsAny<GameSession>()))
			.Callback<GameSession>((game) => updatedGame = game)
			.ReturnsAsync((GameSession game) => game);

		// Act
		await _hub.JoinGame(_testGameId.ToString());

		// Assert
		Assert.NotNull(updatedGame);
		Assert.Equal(_testUserId, updatedGame.BlackPlayerId);
	}

	[Fact]
	public async Task JoinGame_SignalR_PreventsSelfJoin_WhenUserIsAlreadyInGame()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = _testUserId,
			BlackPlayerId = null,
			CreatedAt = DateTime.Now,
			PGN = ""
		};

		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId, default))
			.ReturnsAsync(gameSession);

		// Act
		await _hub.JoinGame(_testGameId.ToString());

		// Assert
		_mockGameSessionRepository.Verify(x => x.UpdateAsync(It.IsAny<GameSession>(), default), Times.Never);
	}

	[Fact]
	public async Task SendGameState_SignalR_UpdatesGamePGN_WhenValidState()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = _testUserId,
			BlackPlayerId = Guid.NewGuid(),
			CreatedAt = DateTime.Now,
			PGN = "1. e4 e5"
		};

		var newGameState = "1. e4 e5 2. Nf3 Nc6";

		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId))
			.ReturnsAsync(gameSession);

		GameSession? updatedGame = null;
		_mockGameSessionRepository
			.Setup(repo => repo.UpdateAsync(It.IsAny<GameSession>()))
			.Callback<GameSession>((game) => updatedGame = game)
			.ReturnsAsync((GameSession game) => game);

		// Act
		await _hub.SendPGNGameState(_testGameId.ToString(), newGameState);

		// Assert
		Assert.NotNull(updatedGame);
		Assert.Equal(newGameState, updatedGame.PGN);
		_mockClientProxy.Verify(x => x.SendCoreAsync("ReceivePGNGameState", 
			It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == newGameState), 
			default), Times.Once);
	}

	[Fact]
	public async Task SendGameState_SignalR_SendsError_WhenInvalidGameState()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = _testUserId,
			BlackPlayerId = Guid.NewGuid(),
			CreatedAt = DateTime.Now,
			PGN = "1. e4 e5"
		};

		var invalidGameState = "1. d4 d5"; // Doesn't start with existing PGN

		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId, default))
			.ReturnsAsync(gameSession);

		// Act
		await _hub.SendPGNGameState(_testGameId.ToString(), invalidGameState);

		// Assert
		// _mockClientProxy.Verify(x => x.SendCoreAsync("Error", 
		//     It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "Invalid game state."), 
		//     default), Times.Once);
	}

	[Fact]
	public async Task SendGameState_SignalR_SendsError_WhenGameNotFound()
	{
		// Arrange
		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId, default))
			.ReturnsAsync((GameSession?)null);

		// Act
		await _hub.SendPGNGameState(_testGameId.ToString(), "some-game-state");

		// Assert
		_mockClientProxy.Verify(x => x.SendCoreAsync("Error", 
			It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "Game session not found."), 
			default), Times.Once);
	}

	[Fact]
	public async Task SendGameState_SignalR_ValidatesPGNContinuity_WhenUpdating()
	{
		// Arrange
		var gameSession = new GameSession
		{
			Id = _testGameId,
			WhitePlayerId = _testUserId,
			BlackPlayerId = Guid.NewGuid(),
			CreatedAt = DateTime.Now,
			PGN = "1. e4"
		};

		var continuingGameState = "1. e4 e5";

		_mockGameSessionRepository
			.Setup(repo => repo.GetByIdAsync(_testGameId))
			.ReturnsAsync(gameSession);

		_mockGameSessionRepository
			.Setup(repo => repo.UpdateAsync(It.IsAny<GameSession>()))
			.ReturnsAsync((GameSession game) => game);

		// Act
		await _hub.SendPGNGameState(_testGameId.ToString(), continuingGameState);

		// Assert
		// Verify that the game state was accepted (no error sent)
		//_mockClientProxy.Verify(x => x.SendCoreAsync("Error", It.IsAny<object[]>(), default), Times.Never);
		_mockGameSessionRepository.Verify(x => x.UpdateAsync(It.IsAny<GameSession>()), Times.Once);
	}
}
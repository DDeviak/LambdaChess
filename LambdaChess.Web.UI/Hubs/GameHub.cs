using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.Web.UI.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace LambdaChess.Web.UI.Hubs;

public class GameHub : Hub
{
	private readonly IGameSessionRepository _gameSessionRepository;

	public GameHub(IGameSessionRepository gameSessionRepository)
	{
		_gameSessionRepository = gameSessionRepository;
	}
	
	public async Task JoinGame(string gameId)
	{
		var gameSession = await _gameSessionRepository.GetByIdAsync(Guid.Parse(gameId));
		if (gameSession == null)
		{
			await Clients.Caller.SendAsync("Error", "Game session not found.");
			return;
		}
		
		await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
		
		await Clients.Group(gameId).SendAsync("UserJoined", Context.User.Identity.Name);
		
		if (gameSession.WhitePlayerId == Context.User.GetUserId() || gameSession.BlackPlayer?.Id == Context.User.GetUserId())
		{
			return;
		}
		
		if (gameSession.WhitePlayer is null)
		{
			gameSession.WhitePlayerId = Context.User.GetUserId();
		}
		else if (gameSession.BlackPlayer is null)
		{
			gameSession.BlackPlayerId = Context.User.GetUserId();
		}
		else
		{
			await Clients.Caller.SendAsync("Error", "Game session is full.");
			return;
		}
		await _gameSessionRepository.UpdateAsync(gameSession);
	}
	
	public async Task SendPGNGameState(string gameId, string gameState)
	{
		var session = await _gameSessionRepository.GetByIdAsync(Guid.Parse(gameId));
		if (session == null)
		{
			await Clients.Caller.SendAsync("Error", "Game session not found.");
			return;
		}
		if (!gameState.StartsWith(session.PGN))
		{
			await Clients.Caller.SendAsync("Error", "Invalid game state.");
			return;
		}
		session.PGN = gameState;
		await _gameSessionRepository.UpdateAsync(session);
		await Clients.Group(gameId).SendAsync("ReceivePGNGameState", gameState);
	}
}
using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace LambdaChess.Web.UI.Hubs;

public class GameHub : Hub
{
	private readonly IGameSessionRepository _gameSessionRepository;
	private readonly UserManager<User> _userManager;

	public GameHub(IGameSessionRepository gameSessionRepository, UserManager<User> userManager)
	{
		_gameSessionRepository = gameSessionRepository;
		_userManager = userManager;
	}
	
	public async Task JoinGame(string gameId)
	{
		var gameSession = await _gameSessionRepository.GetByIdAsync(Guid.Parse(gameId));
		var currentUser = await _userManager.GetUserAsync(Context.User);
		if (gameSession == null)
		{
			await Clients.Caller.SendAsync("Error", "Game session not found.");
			return;
		}
		
		await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
		
		await Clients.Group(gameId).SendAsync("UserJoined", Context.User.Identity.Name);
		
		if (gameSession.WhitePlayer?.Id == currentUser.Id || gameSession.BlackPlayer?.Id == currentUser.Id)
		{
			return;
		}
		
		if (gameSession.WhitePlayer is null)
		{
			gameSession.WhitePlayer = currentUser;
		}
		else if (gameSession.BlackPlayer is null)
		{
			gameSession.BlackPlayer = currentUser;
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
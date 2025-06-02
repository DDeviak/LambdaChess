using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.Web.UI.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace LambdaChess.Web.UI.Hubs;

using System.Diagnostics;
using DAL.Models.Enums;
using Microsoft.EntityFrameworkCore;

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

	    string playerRole;

	    if (gameSession.WhitePlayerId is null)
	    {
	        gameSession.WhitePlayerId = Context.User.GetUserId();
	        playerRole = "white";
	    }
	    else if (gameSession.BlackPlayerId is null)
	    {
	        gameSession.BlackPlayerId = Context.User.GetUserId();
	        playerRole = "black";
	    }
	    else
	    {
	        await Clients.Caller.SendAsync("Error", "Game session is full.");
	        return;
	    }

	    await _gameSessionRepository.UpdateAsync(gameSession);

	    // Send the player's role (White or Black) to the client
	    await Clients.Caller.SendAsync("PlayerRoleAssigned", playerRole);
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
	
	public async Task RequestPGNGameState(string gameId)
	{
		var session = await _gameSessionRepository.GetByIdAsync(Guid.Parse(gameId));
		if (session == null)
		{
			await Clients.Caller.SendAsync("Error", "Game session not found.");
			return;
		}
		await Clients.Caller.SendAsync("ReceivePGNGameState", session.PGN);
	}

	public async Task RegisterGameEnd(string gameId, string gameResult) {
		var session = (await _gameSessionRepository.GetQueryable(q => q
			.Include(q => q.WhitePlayer)
			.Include(q => q.BlackPlayer)).ToListAsync()).
			FirstOrDefault(q => q.Id == Guid.Parse(gameId));

		if (session == null) {
			await Clients.Caller.SendAsync("Error", "Game session not found.");
			return;
		}
		session.FinishedAt = DateTime.Now;
		switch (gameResult) {
			case "drawn":
				session.Winner = Winner.None;
				session.WhitePlayer!.Draws++;
				session.BlackPlayer!.Draws++;
				break;
			case "Black":
				session.Winner = Winner.White;
				session.BlackPlayer!.Losses++;
				session.WhitePlayer!.Wins++;
				break;
			case "White":
				session.Winner = Winner.Black;
				session.BlackPlayer!.Wins++;
				session.WhitePlayer!.Losses++;
				break;
			default:
				await Clients.Caller.SendAsync("Error", "Given incomprehensible game result");
				return;
		}
		await _gameSessionRepository.UpdateAsync(session);
		Debug.Print("Game session has ended!");
		Console.WriteLine($"Winner: {session.BlackPlayer}");
		Console.WriteLine($"Loser: {session.WhitePlayer}");
	}
}
using LambdaChess.DAL.Models;
using LambdaChess.DAL.Models.Enums;
using LambdaChess.DAL.Repositories.Abstractions;

namespace LambdaChess.DAL.Repositories.Implementations;

public class GameSessionRepository : IGameSessionRepository
{
	private static readonly Dictionary<Guid, GameSession> GameSessions = new Dictionary<Guid, GameSession>() 
	{
		{ Guid.NewGuid(), new GameSession() { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, FinishedAt = null, WhitePlayer = new User() { Id = Guid.NewGuid(), UserName = "White" }, Winner = Winner.None } },
		{ Guid.NewGuid(), new GameSession() { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, FinishedAt = null, BlackPlayer = new User() { Id = Guid.NewGuid(), UserName = "Black" }, Winner = Winner.None } }
	};
	
	public Task<IEnumerable<GameSession>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(GameSessions.Values.AsEnumerable());
	}

	public Task<GameSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (GameSessions.TryGetValue(id, out var gameSession))
		{
			return Task.FromResult(gameSession);
		}

		return Task.FromResult<GameSession?>(null);
	}

	public Task<GameSession> CreateAsync(GameSession model, CancellationToken cancellationToken = default)
	{
		if (model.Id == Guid.Empty)
		{
			model.Id = Guid.NewGuid();
		}

		GameSessions.Add(model.Id, model);
		return Task.FromResult(model);
	}

	public Task<GameSession> UpdateAsync(GameSession model, CancellationToken cancellationToken = default)
	{
		if (GameSessions.ContainsKey(model.Id))
		{
			GameSessions[model.Id] = model;
		}

		return Task.FromResult(model);
	}

	public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		GameSessions.Remove(id);
		return Task.CompletedTask;
	}
}
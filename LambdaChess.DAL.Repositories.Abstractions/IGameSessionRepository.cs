using LambdaChess.DAL.Models;

namespace LambdaChess.DAL.Repositories.Abstractions;

public interface IGameSessionRepository : IRepository<GameSession, Guid>
{
	
}
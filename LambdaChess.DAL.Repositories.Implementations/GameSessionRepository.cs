using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.DAL.Repositories.Implementations.Persistance;

namespace LambdaChess.DAL.Repositories.Implementations;

public class GameSessionRepository : BaseRepository<GameSession, Guid>, IGameSessionRepository
{
	public GameSessionRepository(ApplicationDbContext dbContext) : base(dbContext)
	{
	}
}
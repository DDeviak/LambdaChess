using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.DAL.Repositories.Implementations.Persistance;

namespace LambdaChess.DAL.Repositories.Implementations;

public class UserRepository : BaseRepository<User, Guid>, IUserRepository
{
	public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
	{
	}
}
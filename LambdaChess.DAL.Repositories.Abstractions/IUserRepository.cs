using LambdaChess.DAL.Models;

namespace LambdaChess.DAL.Repositories.Abstractions;

public interface IUserRepository : IRepository<User, Guid>
{
	
}
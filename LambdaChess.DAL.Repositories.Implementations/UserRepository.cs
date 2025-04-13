using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;

namespace LambdaChess.DAL.Repositories.Implementations;

public class UserRepository : IUserRepository
{
	private Dictionary<Guid, User> _users = new();
	
	public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_users.Values.AsEnumerable());
	}

	public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (_users.TryGetValue(id, out var user))
		{
			return Task.FromResult(user);
		}

		return Task.FromResult<User?>(null);
	}

	public Task<User> CreateAsync(User model, CancellationToken cancellationToken = default)
	{
		if (model.Id == Guid.Empty)
		{
			model.Id = Guid.NewGuid();
		}

		_users.Add(model.Id, model);
		return Task.FromResult(model);
	}

	public Task<User> UpdateAsync(User model, CancellationToken cancellationToken = default)
	{
		if (_users.ContainsKey(model.Id))
		{
			_users[model.Id] = model;
		}

		return Task.FromResult(model);
	}

	public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		_users.Remove(id);
		return Task.CompletedTask;
	}
}
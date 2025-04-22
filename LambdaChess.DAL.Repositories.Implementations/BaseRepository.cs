using LambdaChess.DAL.Models.Abstractions;
using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.DAL.Repositories.Implementations.Persistance;
using Microsoft.EntityFrameworkCore;

namespace LambdaChess.DAL.Repositories.Implementations;

public abstract class BaseRepository<TModel, TKey> : IRepository<TModel, TKey>, IDisposable
where TModel : class, IModel<TKey>
	where TKey : struct
{
	private readonly ApplicationDbContext _dbContext;

	public BaseRepository(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}
	
	protected DbSet<TModel> DbSet => _dbContext.Set<TModel>();
	
	public async Task<IEnumerable<TModel>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await DbSet.ToListAsync(cancellationToken);
	}

	public async Task<TModel?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
	{
		var entity = await DbSet.FindAsync([id], cancellationToken).AsTask();
		if (entity is not null)
		{
			_dbContext.Entry(entity).State = EntityState.Detached;
		}
		return entity;
	}

	public async Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default)
	{
		await DbSet.AddAsync(model, cancellationToken);
		await _dbContext.SaveChangesAsync(cancellationToken);
		return model;
	}

	public async Task<TModel> UpdateAsync(TModel model, CancellationToken cancellationToken = default)
	{
		DbSet.Update(model);
		await _dbContext.SaveChangesAsync(cancellationToken);
		return model;
	}

	public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
	{
		var entity = DbSet.Find(id);
		if (entity is not null)
		{
			DbSet.Remove(entity);
			return _dbContext.SaveChangesAsync(cancellationToken);
		}
		return Task.CompletedTask;
	}
	
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_dbContext.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
using LambdaChess.DAL.Models.Abstractions;

namespace LambdaChess.DAL.Repositories.Abstractions;

public interface IRepository<TModel, TKey>
	where TModel : class, IModel<TKey>
	where TKey : struct
{
	public Task<IEnumerable<TModel>> GetAllAsync(CancellationToken cancellationToken = default);
	public Task<IEnumerable<TModel>> GetAllAsync() => GetAllAsync(CancellationToken.None);
	public Task<TModel?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
	public Task<TModel?> GetByIdAsync(TKey id) => GetByIdAsync(id, CancellationToken.None);
	public Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default);
	public Task<TModel> CreateAsync(TModel model) => CreateAsync(model, CancellationToken.None);
	public Task<TModel> UpdateAsync(TModel model, CancellationToken cancellationToken = default);
	public Task<TModel> UpdateAsync(TModel model) => UpdateAsync(model, CancellationToken.None);
	public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
	public Task DeleteAsync(TKey id) => DeleteAsync(id, CancellationToken.None);
	IQueryable<TModel> GetQueryable(Func<IQueryable<TModel>, IQueryable<TModel>> query);
}
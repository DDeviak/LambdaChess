namespace LambdaChess.DAL.Models.Abstractions;

public interface IModel<TKey>
	where TKey : struct
{
	TKey Id { get; set; }
}
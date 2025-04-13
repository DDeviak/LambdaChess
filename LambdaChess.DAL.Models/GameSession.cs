using LambdaChess.DAL.Models.Abstractions;
using LambdaChess.DAL.Models.Enums;

namespace LambdaChess.DAL.Models;

public class GameSession : IModel<Guid>
{
	public Guid Id { get; set; }
	public User WhitePlayer { get; set; } = null!;
	public User BlackPlayer { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public DateTime? FinishedAt { get; set; }
	public Winner Winner { get; set; }
}
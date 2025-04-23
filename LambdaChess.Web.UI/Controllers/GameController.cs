using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.Web.UI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LambdaChess.Web.UI.Controllers;

[Authorize]
public class GameController : BaseController
{
	private readonly IGameSessionRepository _gameSessionRepository;

	public GameController(IGameSessionRepository gameSessionRepository)
	{
		_gameSessionRepository = gameSessionRepository;
	}
	
	[HttpGet]
	public async Task<IActionResult> Index(Guid gameId)
	{
		var gameSession = await _gameSessionRepository.GetByIdAsync(gameId);
		return View(gameSession);
	}
	
	[HttpGet]
	public async Task<IActionResult> Lobby()
	{
		var model = (await _gameSessionRepository.GetAllAsync()).Where(x => x.WhitePlayer is null || x.BlackPlayer is null).ToList();
		return View(model);
	}
	
	[HttpPost]
	public async Task<IActionResult> Create()
	{
		var gameSession = new GameSession
		{
			CreatedAt = DateTime.Now,
			WhitePlayerId = User.GetUserId(),
			PGN = string.Empty
		};
		await _gameSessionRepository.CreateAsync(gameSession);
		return Ok(new{ gameId = gameSession.Id });
	}
}
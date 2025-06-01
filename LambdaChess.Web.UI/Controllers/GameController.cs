using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.Web.UI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
		var model = await Task.Run(() => (_gameSessionRepository
				.GetQueryable(q => q.Include(t => t.WhitePlayer).Include(t => t.BlackPlayer)).ToList())
			.Where(x => x.WhitePlayer is null || x.BlackPlayer is null).ToList());
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Create()
	{
		var gameSession = new GameSession
		{
			CreatedAt = DateTime.Now,
			WhitePlayerId = null, // not defined yet
			PGN = string.Empty
		};
		gameSession = await _gameSessionRepository.CreateAsync(gameSession);
		return Ok(new { gameId = gameSession.Id });
	}
}
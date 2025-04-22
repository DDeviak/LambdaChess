using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LambdaChess.Web.UI.Controllers;

[Authorize]
public class GameController : BaseController
{
	private readonly IGameSessionRepository _gameSessionRepository;
	private readonly UserManager<User> _userManager;

	public GameController(IGameSessionRepository gameSessionRepository, UserManager<User> userManager)
	{
		_gameSessionRepository = gameSessionRepository;
		_userManager = userManager;
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
			WhitePlayer = await _userManager.GetUserAsync(User),
			PGN = string.Empty
		};
		await _gameSessionRepository.CreateAsync(gameSession);
		return Ok(new{ gameId = gameSession.Id });
	}
}
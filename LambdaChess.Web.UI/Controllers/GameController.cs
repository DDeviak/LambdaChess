using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace LambdaChess.Web.UI.Controllers;

public class GameController : BaseController
{
	private readonly IGameSessionRepository _gameSessionRepository;

	public GameController(IGameSessionRepository gameSessionRepository)
	{
		_gameSessionRepository = gameSessionRepository;
	}
	
	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var model = await _gameSessionRepository.GetAllAsync();
		return View("Lobby", model);
	}
}
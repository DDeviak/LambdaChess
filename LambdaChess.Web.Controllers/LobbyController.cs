using LambdaChess.DAL.Repositories.Abstractions;
using LambdaChess.Web.UI.Pages;
using Microsoft.AspNetCore.Mvc;

namespace LambdaChess.Web.Controllers;

public class LobbyController : BaseController
{
	private readonly IGameSessionRepository _gameSessionRepository;

	public LobbyController(IGameSessionRepository gameSessionRepository)
	{
		_gameSessionRepository = gameSessionRepository;
	}

	public async Task<IActionResult> Index()
	{
		return this.View(new LobbyModel() { GameSessions = await _gameSessionRepository.GetAllAsync() });
	}
}
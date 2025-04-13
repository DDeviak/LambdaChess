using LambdaChess.DAL.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LambdaChess.Web.UI.Pages;

public class LobbyModel : PageModel
{
	public IEnumerable<GameSession> GameSessions { get; set; } = Enumerable.Empty<GameSession>();
	
	public void OnGet()
	{
		
	}
}
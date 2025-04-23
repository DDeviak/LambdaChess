using System.Security.Claims;

namespace LambdaChess.Web.UI.Extensions;

public static class Extensions
{
	public static Guid GetUserId(this ClaimsPrincipal user)
	{
		return Guid.Parse(user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
	}
}
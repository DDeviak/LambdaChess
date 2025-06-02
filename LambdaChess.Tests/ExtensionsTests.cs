using System.Security.Claims;
using LambdaChess.Web.UI.Extensions;

namespace LambdaChess.Tests;

public class ExtensionsTests
{
	[Fact]
	public void GetUserId_ReturnsCorrectGuid_WhenValidClaims()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, userId.ToString())
		};
		var identity = new ClaimsIdentity(claims);
		var principal = new ClaimsPrincipal(identity);

		// Act
		var result = principal.GetUserId();

		// Assert
		Assert.Equal(userId, result);
	}

	[Fact]
	public void GetUserId_ThrowsException_WhenNoNameIdentifierClaim()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.Email, "test@example.com")
		};
		var identity = new ClaimsIdentity(claims);
		var principal = new ClaimsPrincipal(identity);

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
	}
}
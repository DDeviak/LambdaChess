using LambdaChess.Web.UI.Controllers;
using LambdaChess.Web.UI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace LambdaChess.Tests;

public class HomeControllerTests
{
	private readonly Mock<ILogger<HomeController>> _mockLogger;
	private readonly HomeController _controller;

	public HomeControllerTests()
	{
		_mockLogger = new Mock<ILogger<HomeController>>();
		_controller = new HomeController(_mockLogger.Object);
	}

	[Fact]
	public void Home_GET_ReturnsViewResult_Always()
	{
		// Arrange
		// (Nothing to arrange - simple action)

		// Act
		var result = _controller.Index();

		// Assert
		Assert.IsType<ViewResult>(result);
	}

	[Fact]
	public void Privacy_GET_ReturnsViewResult_Always()
	{
		// Arrange
		// (Nothing to arrange - simple action)

		// Act
		var result = _controller.Privacy();

		// Assert
		Assert.IsType<ViewResult>(result);
	}

	[Fact]
	public void Error_GET_ReturnsViewWithErrorModel_WhenCalled()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.TraceIdentifier = "test-trace-id";
		_controller.ControllerContext = new ControllerContext()
		{
			HttpContext = httpContext
		};

		// Act
		var result = _controller.Error();

		// Assert
		var viewResult = Assert.IsType<ViewResult>(result);
		var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
		Assert.Equal("test-trace-id", model.RequestId);
		Assert.True(model.ShowRequestId);
	}
}
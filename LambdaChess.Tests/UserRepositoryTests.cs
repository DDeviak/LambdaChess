using LambdaChess.DAL.Models;
using LambdaChess.DAL.Repositories.Implementations;
using LambdaChess.DAL.Repositories.Implementations.Persistance;
using Microsoft.EntityFrameworkCore;

namespace LambdaChess.Tests;

public class UserRepositoryTests
{
	private readonly UserRepository _repository;
	private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

	public UserRepositoryTests()
	{
		_dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;
            
		var context = new ApplicationDbContext(_dbContextOptions);
		_repository = new UserRepository(context);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllUsers_WhenCalled()
	{
		// Arrange
		using var context = new ApplicationDbContext(_dbContextOptions);
		var users = new List<User>
		{
			new User { Id = Guid.NewGuid(), UserName = "user1@test.com" },
			new User { Id = Guid.NewGuid(), UserName = "user2@test.com" }
		};
            
		context.Users.AddRange(users);
		await context.SaveChangesAsync();

		// Act
		var result = await _repository.GetAllAsync();

		// Assert
		Assert.Equal(2, result.Count());
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsUser_WhenExists()
	{
		// Arrange
		var userId = Guid.NewGuid();
		using var context = new ApplicationDbContext(_dbContextOptions);
		var user = new User { Id = userId, UserName = "test@example.com" };
            
		context.Users.Add(user);
		await context.SaveChangesAsync();

		// Act
		var result = await _repository.GetByIdAsync(userId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(userId, result.Id);
		Assert.Equal("test@example.com", result.UserName);
	}

	[Fact]
	public async Task CreateAsync_CreatesUser_WhenValidData()
	{
		// Arrange
		var user = new User
		{
			Id = Guid.NewGuid(),
			UserName = "newuser@test.com",
			Email = "newuser@test.com"
		};

		// Act
		var result = await _repository.CreateAsync(user);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(user.Id, result.Id);
		Assert.Equal(user.UserName, result.UserName);
            
		// Verify it was saved to database
		var savedUser = await _repository.GetByIdAsync(user.Id);
		Assert.NotNull(savedUser);
	}
}
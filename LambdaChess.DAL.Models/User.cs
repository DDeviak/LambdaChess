using LambdaChess.DAL.Models.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace LambdaChess.DAL.Models;

public class User : IdentityUser<Guid>, IModel<Guid>
{
	public override Guid Id { get; set; }
}
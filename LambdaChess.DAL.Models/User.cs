using LambdaChess.DAL.Models.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace LambdaChess.DAL.Models;

public class User : IdentityUser<Guid>, IModel<Guid>
{
    public override Guid Id { get; set; }

    // Profile fields
    public uint Wins { get; set; }
	public uint Losses { get; set; }
	public uint Draws { get; set; }
}
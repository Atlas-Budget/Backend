using System;

namespace Api.Entities;

public class RefreshToken
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid UserId { get; set; }
	public User User { get; set; } = default!;

	public string TokenHash { get; set; } = default!;
	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	public DateTime ExpiresAtUtc { get; set; }
	public DateTime? RevokedAtUtc { get; set; }
}

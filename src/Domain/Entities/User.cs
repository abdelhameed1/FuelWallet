namespace FuelWallet.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
}

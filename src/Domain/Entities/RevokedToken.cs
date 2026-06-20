namespace FuelWallet.Domain.Entities;

public class RevokedToken : BaseEntity
{
    public string Jti { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
}

using FuelWallet.Application.FuelAuthorizations.Commands.CreateFuelAuthorization;
using FluentValidation.TestHelper;

namespace FuelWallet.Application.Tests.FuelAuthorizations;

public class CreateFuelAuthorizationCommandValidatorTests
{
    private readonly CreateFuelAuthorizationCommandValidator _validator = new();

    private static CreateFuelAuthorizationCommand Valid() => new()
    {
        WalletId = "WLT-1001",
        StationId = 101,
        PumpId = 1,
        RequestedAmount = 100m,
        RequestReference = "REQ-1"
    };

    [Fact]
    public void Passes_WhenCommandIsValid()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Fails_WhenWalletIdEmpty(string walletId)
    {
        var command = Valid() with { WalletId = walletId };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.WalletId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Fails_WhenRequestedAmountNotPositive(decimal amount)
    {
        var command = Valid() with { RequestedAmount = amount };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.RequestedAmount);
    }

    [Fact]
    public void Fails_WhenRequestReferenceEmpty()
    {
        var command = Valid() with { RequestReference = "" };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.RequestReference);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Fails_WhenStationIdNotPositive(int stationId)
    {
        var command = Valid() with { StationId = stationId };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.StationId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Fails_WhenPumpIdNotPositive(int pumpId)
    {
        var command = Valid() with { PumpId = pumpId };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.PumpId);
    }
}

using FluentValidation;

namespace FuelWallet.Application.FuelAuthorizations.Commands.CreateFuelAuthorization;

public class CreateFuelAuthorizationCommandValidator : AbstractValidator<CreateFuelAuthorizationCommand>
{
    public CreateFuelAuthorizationCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("wallet Id is required.");

        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0).WithMessage("requested amount must be greater than zero.");

        RuleFor(x => x.RequestReference)
            .NotEmpty().WithMessage("requestReference is required.");

        RuleFor(x => x.StationId)
            .GreaterThan(0).WithMessage("A valid station must be specified.");

        RuleFor(x => x.PumpId)
            .GreaterThan(0).WithMessage("A valid pump must be specified.");
    }
}
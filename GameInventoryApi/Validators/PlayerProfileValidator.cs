using FluentValidation;
using GameInventoryApi.Models;

namespace GameInventoryApi.Validators;

public class PlayerProfileValidator : AbstractValidator<PlayerProfile>
{
    public PlayerProfileValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Level).InclusiveBetween(1, 100);
        RuleFor(x => x.Experience).GreaterThanOrEqualTo(0);
    }
}

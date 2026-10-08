using FluentValidation;
using GameInventoryApi.Models;

namespace GameInventoryApi.Validators;

public class InventoryItemValidator : AbstractValidator<InventoryItem>
{
    public InventoryItemValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Quantity).InclusiveBetween(0, 9999);
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}

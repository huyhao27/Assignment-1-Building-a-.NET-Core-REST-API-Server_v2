using FluentValidation;
using GameInventoryApi.DTOs;

namespace GameInventoryApi.Validators;

public class PatchInventoryItemDtoValidator : AbstractValidator<PatchInventoryItemDto>
{
    public PatchInventoryItemDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.ItemId is not null || x.Name is not null || x.Quantity is not null || x.PlayerId is not null)
            .WithName("Body")
            .WithMessage("At least one field must be provided.");

        // Fields are optional, but a field that is sent must be valid.
        RuleFor(x => x.ItemId).NotEmpty().MaximumLength(50).When(x => x.ItemId is not null);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).When(x => x.Name is not null);
        RuleFor(x => x.Quantity).InclusiveBetween(0, 9999).When(x => x.Quantity is not null);
        RuleFor(x => x.PlayerId).NotEmpty().When(x => x.PlayerId is not null);
    }
}

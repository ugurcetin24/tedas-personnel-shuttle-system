using FluentValidation;
using Tedas.Shuttle.Application.DTOs.Shifts;
using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Application.Validators;

public sealed class UpdateShiftRequestValidator : AbstractValidator<UpdateShiftRequest>
{
    public UpdateShiftRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.ShiftType)
            .IsInEnum()
            .NotEqual((ShiftType)0);

        RuleFor(request => request.Capacity)
            .GreaterThan(0);
    }
}

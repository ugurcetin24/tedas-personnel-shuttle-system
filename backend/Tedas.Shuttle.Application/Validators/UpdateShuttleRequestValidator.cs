using FluentValidation;
using Tedas.Shuttle.Application.DTOs.Shuttles;

namespace Tedas.Shuttle.Application.Validators;

public sealed class UpdateShuttleRequestValidator : AbstractValidator<UpdateShuttleRequest>
{
    public UpdateShuttleRequestValidator()
    {
        RuleFor(request => request.PlateNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(request => request.Description)
            .MaximumLength(500);
    }
}

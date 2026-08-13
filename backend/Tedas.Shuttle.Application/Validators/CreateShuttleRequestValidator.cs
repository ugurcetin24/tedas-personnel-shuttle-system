using FluentValidation;
using Tedas.Shuttle.Application.DTOs.Shuttles;

namespace Tedas.Shuttle.Application.Validators;

public sealed class CreateShuttleRequestValidator : AbstractValidator<CreateShuttleRequest>
{
    public CreateShuttleRequestValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.PlateNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(request => request.Description)
            .MaximumLength(500);
    }
}

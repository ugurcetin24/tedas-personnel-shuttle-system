using FluentValidation;
using Tedas.Shuttle.Application.DTOs.Drivers;

namespace Tedas.Shuttle.Application.Validators;

public sealed class UpdateDriverRequestValidator : AbstractValidator<UpdateDriverRequest>
{
    public UpdateDriverRequestValidator()
    {
        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Phone)
            .MaximumLength(30);

        RuleFor(request => request.LicenseNumber)
            .NotEmpty()
            .MaximumLength(50);
    }
}


using FluentValidation;
using Tedas.Shuttle.Application.DTOs.Personnel;

namespace Tedas.Shuttle.Application.Validators;

public sealed class CreatePersonnelRequestValidator : AbstractValidator<CreatePersonnelRequest>
{
    public CreatePersonnelRequestValidator()
    {
        RuleFor(request => request.RegistrationNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Department)
            .MaximumLength(150);

        RuleFor(request => request.Title)
            .MaximumLength(150);

        RuleFor(request => request.Phone)
            .MaximumLength(30);

        RuleFor(request => request.Email)
            .MaximumLength(200)
            .EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.Address)
            .MaximumLength(500);

        RuleFor(request => request.Latitude)
            .InclusiveBetween(-90m, 90m)
            .When(request => request.Latitude.HasValue);

        RuleFor(request => request.Longitude)
            .InclusiveBetween(-180m, 180m)
            .When(request => request.Longitude.HasValue);
    }
}

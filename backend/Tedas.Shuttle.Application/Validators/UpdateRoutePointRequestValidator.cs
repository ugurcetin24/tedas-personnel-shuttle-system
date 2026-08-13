using FluentValidation;
using Tedas.Shuttle.Application.DTOs.RoutePoints;

namespace Tedas.Shuttle.Application.Validators;

public sealed class UpdateRoutePointRequestValidator : AbstractValidator<UpdateRoutePointRequest>
{
    public UpdateRoutePointRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.Address)
            .MaximumLength(500);

        RuleFor(request => request.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(request => request.Longitude)
            .InclusiveBetween(-180, 180);
    }
}


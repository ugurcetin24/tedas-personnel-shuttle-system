using FluentValidation;
using Tedas.Shuttle.Application.DTOs.Assignments;

namespace Tedas.Shuttle.Application.Validators;

public sealed class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(request => request.PersonnelId)
            .NotEmpty();

        RuleFor(request => request.ShuttleShiftId)
            .NotEmpty();
    }
}


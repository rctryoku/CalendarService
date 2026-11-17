using Calendar.Core.Features.Calendars;
using FastEndpoints;
using FluentValidation;

namespace Calendar.Api.Features.Calendars.Create;

public sealed class CreateCalendarValidator : Validator<CreateCalendarRequest>
{
    public CreateCalendarValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(UserCalendar.NameMaxLength);
        RuleFor(request => request.Owner).NotEmpty().MaximumLength(UserCalendar.OwnerMaxLength);
    }
}

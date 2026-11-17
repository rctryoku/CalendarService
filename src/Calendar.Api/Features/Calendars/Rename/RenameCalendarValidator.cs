using Calendar.Core.Features.Calendars;
using FastEndpoints;
using FluentValidation;

namespace Calendar.Api.Features.Calendars.Rename;

public sealed class RenameCalendarValidator : Validator<RenameCalendarRequest>
{
    public RenameCalendarValidator() =>
        RuleFor(request => request.Name).NotEmpty().MaximumLength(UserCalendar.NameMaxLength);
}

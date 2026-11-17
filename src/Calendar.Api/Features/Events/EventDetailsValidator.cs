using Calendar.Core.Features.Events;
using FluentValidation;

namespace Calendar.Api.Features.Events;

public sealed class EventDetailsValidator : AbstractValidator<IEventDetails>
{
    public EventDetailsValidator()
    {
        RuleFor(details => details.Title).NotEmpty().MaximumLength(CalendarEvent.TitleMaxLength);
        RuleFor(details => details.Description).MaximumLength(CalendarEvent.DescriptionMaxLength);
        RuleFor(details => details.StartDate).NotEmpty();
        RuleFor(details => details.EndDate)
            .NotEmpty()
            .GreaterThan(details => details.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}

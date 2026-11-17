using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Recurrence;
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

        RuleFor(details => details.ResourceIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("ResourceIds must not contain duplicates.");
        RuleForEach(details => details.ResourceIds).NotEmpty();

        When(details => details.Recurrence is not null, AddRecurrenceRules);
    }

    private void AddRecurrenceRules()
    {
        RuleFor(details => details.Recurrence!.Frequency).IsInEnum();
        RuleFor(details => details.Recurrence!.Interval).InclusiveBetween(1, RecurrenceRule.MaxInterval);
        RuleFor(details => details.Recurrence!.Count).InclusiveBetween(1, RecurrenceRule.MaxCount);
        RuleFor(details => details)
            .Must(OccurrencesDoNotOverlap)
            .WithName(nameof(IEventDetails.Recurrence))
            .WithMessage("Each occurrence must end before the next one starts.");
    }

    private static bool OccurrencesDoNotOverlap(IEventDetails details) =>
        details.EndDate - details.StartDate <= details.Recurrence!.ToRule().Step;
}

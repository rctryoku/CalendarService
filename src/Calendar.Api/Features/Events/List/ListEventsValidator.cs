using FastEndpoints;
using FluentValidation;

namespace Calendar.Api.Features.Events.List;

public sealed class ListEventsValidator : Validator<ListEventsRequest>
{
    public const int MaxRangeDays = 366;

    public ListEventsValidator()
    {
        RuleFor(request => request.From).NotEmpty();
        RuleFor(request => request.To)
            .NotEmpty()
            .GreaterThan(request => request.From)
            .WithMessage("To must be after From.");
        RuleFor(request => request)
            .Must(request => request.To - request.From <= TimeSpan.FromDays(MaxRangeDays))
            .WithName(nameof(ListEventsRequest.To))
            .WithMessage($"The range from From to To can be at most {MaxRangeDays} days.");
    }
}

using FastEndpoints;

namespace Calendar.Api.Features.Events.Create;

public sealed class CreateEventValidator : Validator<CreateEventRequest>
{
    public CreateEventValidator() => Include(new EventDetailsValidator());
}

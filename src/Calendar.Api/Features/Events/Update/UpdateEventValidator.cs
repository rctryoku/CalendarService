using FastEndpoints;

namespace Calendar.Api.Features.Events.Update;

public sealed class UpdateEventValidator : Validator<UpdateEventRequest>
{
    public UpdateEventValidator() => Include(new EventDetailsValidator());
}

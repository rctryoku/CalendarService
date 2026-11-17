using Calendar.Api.Features.Events;
using Calendar.Api.Features.Events.Create;
using Calendar.Core.Features.Events;

namespace Calendar.UnitTests.Features.Events;

public sealed class EventDetailsValidatorTests
{
    private static readonly DateTimeOffset Nine = new(2026, 11, 18, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Ten = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private readonly EventDetailsValidator _validator = new();

    [Fact]
    public void Validate_ValidEvent_HasNoErrors()
    {
        var result = _validator.Validate(Event());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankTitle_HasTitleError(string title)
    {
        var result = _validator.Validate(Event() with { Title = title });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(IEventDetails.Title));
    }

    [Fact]
    public void Validate_TitleTooLong_HasTitleError()
    {
        var title = new string('x', CalendarEvent.TitleMaxLength + 1);

        var result = _validator.Validate(Event() with { Title = title });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(IEventDetails.Title));
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasDescriptionError()
    {
        var description = new string('x', CalendarEvent.DescriptionMaxLength + 1);

        var result = _validator.Validate(Event() with { Description = description });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(IEventDetails.Description));
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_HasEndDateError()
    {
        var result = _validator.Validate(Event() with { StartDate = Ten, EndDate = Nine });

        var error = Assert.Single(result.Errors);
        Assert.Equal("EndDate must be after StartDate.", error.ErrorMessage);
    }

    [Fact]
    public void Validate_EndDateEqualsStartDate_HasEndDateError()
    {
        var result = _validator.Validate(Event() with { EndDate = Nine });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(IEventDetails.EndDate));
    }

    [Fact]
    public void Validate_EndDateInOtherTimeZoneButLaterInstant_HasNoErrors()
    {
        var tenFifteenInCopenhagen = new DateTimeOffset(2026, 11, 18, 10, 15, 0, TimeSpan.FromHours(1));

        var result = _validator.Validate(Event() with { StartDate = Nine, EndDate = tenFifteenInCopenhagen });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MissingDates_HasStartDateAndEndDateErrors()
    {
        var result = _validator.Validate(Event() with { StartDate = default, EndDate = default });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(IEventDetails.StartDate));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(IEventDetails.EndDate));
    }

    private static CreateEventRequest Event() => new()
    {
        Title = "Standup",
        Description = "Daily sync",
        StartDate = Nine,
        EndDate = Ten,
    };
}

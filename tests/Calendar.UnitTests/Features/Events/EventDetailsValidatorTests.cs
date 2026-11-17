using Calendar.Api.Features.Events;
using Calendar.Api.Features.Events.Create;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Recurrence;

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

    [Theory]
    [InlineData(-60)]
    [InlineData(0)]
    public void Validate_EndDateNotAfterStartDate_HasEndDateError(int minutesAfterStart)
    {
        var result = _validator.Validate(Event() with { EndDate = Nine.AddMinutes(minutesAfterStart) });

        var error = Assert.Single(result.Errors);
        Assert.Equal("EndDate must be after StartDate.", error.ErrorMessage);
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

    [Fact]
    public void Validate_ValidRecurrence_HasNoErrors()
    {
        var result = _validator.Validate(Event() with { Recurrence = Weekly() with { Count = 10 } });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(RecurrenceRule.MaxInterval + 1)]
    public void Validate_IntervalOutOfRange_HasError(int interval)
    {
        var result = _validator.Validate(Event() with { Recurrence = Weekly() with { Interval = interval } });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Validate_UnknownFrequency_HasError(int frequency)
    {
        var result = _validator.Validate(Event() with { Recurrence = Weekly() with { Frequency = (RecurrenceFrequency)frequency } });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(RecurrenceRule.MaxCount + 1)]
    public void Validate_CountOutOfRange_HasError(int count)
    {
        var result = _validator.Validate(Event() with { Recurrence = Weekly() with { Count = count } });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_OccurrenceLongerThanInterval_HasError()
    {
        var twoDayEvent = Event() with { EndDate = Nine.AddDays(2), Recurrence = Weekly() with { Frequency = RecurrenceFrequency.Daily } };

        var result = _validator.Validate(twoDayEvent);

        var error = Assert.Single(result.Errors);
        Assert.Equal("Each occurrence must end before the next one starts.", error.ErrorMessage);
    }

    [Fact]
    public void Validate_SameResourceTwice_HasError()
    {
        var room = Guid.NewGuid();

        var result = _validator.Validate(Event() with { ResourceIds = [room, room] });

        var error = Assert.Single(result.Errors);
        Assert.Equal("ResourceIds must not contain duplicates.", error.ErrorMessage);
    }

    private static RecurrenceRequest Weekly() => new() { Frequency = RecurrenceFrequency.Weekly };

    private static CreateEventRequest Event() => new()
    {
        Title = "Standup",
        Description = "Daily sync",
        StartDate = Nine,
        EndDate = Ten,
    };
}

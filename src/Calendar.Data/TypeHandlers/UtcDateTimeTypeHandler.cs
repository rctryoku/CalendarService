using System.Data;
using System.Globalization;
using Dapper;

namespace Calendar.Data.TypeHandlers;

/// <summary>
/// Stores UTC times as fixed-width ISO 8601 text, so string comparison in SQL matches time order.
/// </summary>
internal sealed class UtcDateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffffffZ";

    public override void SetValue(IDbDataParameter parameter, DateTime value) =>
        parameter.Value = value.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture);

    public override DateTime Parse(object value) =>
        DateTime.ParseExact((string)value, Format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
}

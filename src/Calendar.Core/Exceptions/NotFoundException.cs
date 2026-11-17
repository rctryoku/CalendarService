namespace Calendar.Core.Exceptions;

public sealed class NotFoundException(string entityName, Guid id)
    : Exception($"{entityName} '{id}' was not found.");

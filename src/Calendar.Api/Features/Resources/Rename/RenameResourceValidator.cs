using Calendar.Core.Features.Resources;
using FastEndpoints;
using FluentValidation;

namespace Calendar.Api.Features.Resources.Rename;

public sealed class RenameResourceValidator : Validator<RenameResourceRequest>
{
    public RenameResourceValidator() =>
        RuleFor(request => request.Name).NotEmpty().MaximumLength(Resource.NameMaxLength);
}

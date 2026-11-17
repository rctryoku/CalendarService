using Calendar.Core.Features.Resources;
using FastEndpoints;
using FluentValidation;

namespace Calendar.Api.Features.Resources.Create;

public sealed class CreateResourceValidator : Validator<CreateResourceRequest>
{
    public CreateResourceValidator() =>
        RuleFor(request => request.Name).NotEmpty().MaximumLength(Resource.NameMaxLength);
}

using TxConcert.Domain.Common;
using FluentValidation;

namespace TxConcert.Application.Features.ExternalAudits.Commands.CreateRequestAudit;

public sealed class CreateRequestAuditValidator : AbstractValidator<CreateRequestAuditCommand>
{
    public CreateRequestAuditValidator()
    {
        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .MaximumLength(Constants.Validation.MaxIdLength);

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(Constants.Validation.MaxShortStringLength);

        RuleFor(x => x.RequestUrl)
            .NotEmpty()
            .MaximumLength(Constants.Validation.MaxUrlLength);

        RuleFor(x => x.RequestBody)
            .NotNull();

        RuleFor(x => x.ResponseTimeMs)
            .GreaterThanOrEqualTo(0);
    }
}

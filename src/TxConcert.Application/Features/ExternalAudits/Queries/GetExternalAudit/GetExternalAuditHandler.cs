using TxConcert.Domain.Features.ExternalAudit;
using MediatR;

namespace TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudit;

public sealed class GetExternalAuditHandler(IExternalAuditRepository repository)
    : IRequestHandler<GetExternalAuditQuery, ExternalAuditResponse?>
{
    public async Task<ExternalAuditResponse?> Handle(GetExternalAuditQuery request, CancellationToken ct)
    {
        ExternalAuditEntity? entity = await repository.GetByIdAsync(request.Id, ct);
        return entity is null ? null : ExternalAuditResponse.From(entity);
    }
}

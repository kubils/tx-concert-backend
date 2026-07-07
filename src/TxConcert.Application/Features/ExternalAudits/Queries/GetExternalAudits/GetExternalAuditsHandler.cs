using TxConcert.Application.Common.Responses;
using TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudit;
using TxConcert.Domain.Features.ExternalAudit;
using MediatR;

namespace TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudits;

public sealed class GetExternalAuditsHandler(IExternalAuditRepository repository)
    : IRequestHandler<GetExternalAuditsQuery, PaginatedResponse<ExternalAuditResponse>>
{
    public async Task<PaginatedResponse<ExternalAuditResponse>> Handle(
        GetExternalAuditsQuery request,
        CancellationToken ct)
    {
        (IReadOnlyList<ExternalAuditEntity> items, long total) =
            await repository.GetPaginatedFilteredAsync(
                request.Limit,
                request.Offset,
                request.Type,
                request.TargetId,
                ct);

        return PaginatedResponse<ExternalAuditResponse>.From(
            items.Select(ExternalAuditResponse.From).ToList(),
            total,
            request.Limit,
            request.Offset);
    }
}

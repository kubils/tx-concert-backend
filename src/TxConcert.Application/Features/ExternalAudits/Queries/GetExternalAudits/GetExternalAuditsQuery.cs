using TxConcert.Application.Common.Responses;
using TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudit;
using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudits;

public sealed record GetExternalAuditsQuery(
    int Limit = Constants.Pagination.DefaultLimit,
    int Offset = Constants.Pagination.DefaultOffset,
    string? Type = null,
    string? TargetId = null) : IQuery<PaginatedResponse<ExternalAuditResponse>>;

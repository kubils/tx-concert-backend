using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudit;

public sealed record GetExternalAuditQuery(string Id) : IQuery<ExternalAuditResponse?>;

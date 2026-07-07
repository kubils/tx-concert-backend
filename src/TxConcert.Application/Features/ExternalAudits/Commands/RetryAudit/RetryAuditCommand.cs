using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.ExternalAudits.Commands.RetryAudit;

/// <summary>
/// Marks an external audit for retry and increments attempt count.
/// </summary>
public sealed record RetryAuditCommand(string AuditId) : ICommand;

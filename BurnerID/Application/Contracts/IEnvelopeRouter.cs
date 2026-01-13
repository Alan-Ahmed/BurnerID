using Domain.Models;
using Domain.ValueObjects;

namespace Application.Contracts;

public interface IEnvelopeRouter
{
    Task DeliverAsync(UserId recipient, Envelope envelope, CancellationToken ct);
}

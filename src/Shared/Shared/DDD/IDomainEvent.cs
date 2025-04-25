using MediatR;

namespace Shared.DDD;
public interface IDomainEvent : INotification
{
    Guid EventID => Guid.NewGuid();
    public DateTime OccuredOn => DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName!;
}

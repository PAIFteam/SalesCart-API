namespace SalesCart.Core.Application.Commons;

public abstract class BaseMessage<TMessage>(Guid correlationId)
{
    public Guid CorrelationId { get; set;} = correlationId;
}

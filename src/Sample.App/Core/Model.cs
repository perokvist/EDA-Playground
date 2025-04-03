using System.Text.Json.Serialization;

namespace Sample.App.Core;


public record Event(Guid EventId);
public record Command([property: JsonIgnore] Guid Id);
public record State(Guid Id);

public record DomainEvent(Guid Id) : Event(Guid.NewGuid());
public record IntegrationEvent() : Event(Guid.NewGuid());



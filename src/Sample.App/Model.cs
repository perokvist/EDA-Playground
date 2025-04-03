using Sample.App.Core;

namespace Sample.App;

public record SampleCommand(Guid Id, string Text) : Command(Id);
public record SampleState(Guid Id, string Text) : State(Id);
public record SampleEvent(Guid Id, string Text) : DomainEvent(Id);
public record SampleIntegrationEvent(string Text) : IntegrationEvent;


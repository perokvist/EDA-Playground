namespace Sample.App;


public record Event(Guid EventId);

public record SampleCommand(string Text);

public record SampleState(Guid Id, string Text);

public record SampleEvent(string Text) :Event(Guid.NewGuid());


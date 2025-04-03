using Sample.App.Core;

namespace Sample.App;

public record SampleDecider() : Decider(
            InitialState: new SampleState(Guid.Empty, "None"),
            Decide: (c, s) => (c, s) switch
            {
                (SampleCommand cmd, SampleState state) => [new SampleEvent(cmd.Id, cmd.Text)],
                _ => []
            },
            Evolve: (s, e) => (s, e) switch
            {
                (SampleState state, SampleEvent @event) => state with { Text = @event.Text },
                _ => s
            },
            IsTerminal: s => false);


namespace Sample.App.Core;
public record Decider<TCommand, TEvent, TState>(
    State InitialState,
    Func<TCommand, TState, TEvent[]> Decide,
    Func<TState, TEvent, TState> Evolve,
    Func<TState, bool> IsTerminal);

public record Decider(
    State InitialState,
    Func<Command, State, Event[]> Decide,
    Func<State, Event, State> Evolve,
    Func<State, bool> IsTerminal)
    : Decider<Command, Event, State>(InitialState, Decide, Evolve, IsTerminal);


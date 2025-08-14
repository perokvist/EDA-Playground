using Dapr.Client;
using Sample.App.Core;
using Sample.App.Dapr;
using Sample.App.Infra;
using IntegrationPublisher = System.Func<Sample.App.Core.Event[], System.Threading.Tasks.Task>;

namespace Sample.App.Modules.Sample;

public class SampleModule(DaprClient dapr, IntegrationPublisher pub)
{
    public Task Dispatch(Command command)
        => dapr.ExecuteTestDouble(Constants.StateStore, Constants.StateEventStore, command, command switch
        {
            SampleCommand cmd => new SampleDecider(),
            _ => throw new ArgumentException(nameof(command)),
        });

    public Task<T> Query<T>() => Task.FromResult<T>(default);

    public Task When(Event @event)
        => @event switch
        {
            SampleEvent e => pub([new SampleIntegrationEvent(e.Text)]),
            _ => Task.CompletedTask
        };
}

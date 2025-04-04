using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Sample.App.Core;
using Sample.App.Infra;

namespace Sample.App.Tests;

public class JsonTests
{
    [Fact]
    public void EventSerialization()
    {

        var @event = new SampleEvent(Guid.NewGuid(), "test");

        var opt = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opt.TypeInfoResolverChain.Clear();
        opt.TypeInfoResolverChain.Add(new PolymorphicEventTypeResolver());
        opt.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());


        var json = JsonSerializer.Serialize(new Event[] { @event }, opt);

        Assert.Contains("text", json);

        var events = JsonSerializer.Deserialize<Event[]>(json, opt);

        Assert.IsType<SampleEvent>(events.FirstOrDefault());

    }
}

using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;
using Google.Protobuf;
using Sample.App.Core;
using Sample.App.Infra;
using Sample.App.Modules.Sample;

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

    [Fact]
    public void CloudEvent()
    { 
        var @event = new SampleEvent(Guid.NewGuid(), "test");

        var opt = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opt.TypeInfoResolverChain.Clear();
        opt.TypeInfoResolverChain.Add(new PolymorphicEventTypeResolver());
        opt.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());

        var cloudEvent = new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = @event.GetType().Name,
            Source = new Uri("test:sample"),
            DataContentType = MediaTypeNames.Application.Json,
            Data = JsonSerializer.Serialize(new Event[] { @event }, opt)
        };

        var formatter = new JsonEventFormatter();
        var bytes = formatter.EncodeStructuredModeMessage(cloudEvent, out var contentType);
        string json = Encoding.UTF8.GetString(bytes.Span);

    }

    public record TestState(Guid Id, string Text);

    [Fact]
    public void Foo()
    {

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var state = new TestState(Guid.NewGuid(), "test");

        var transactionBytes = JsonSerializer.SerializeToUtf8Bytes(state, options);
        var transactionValue = ByteString.CopyFrom(transactionBytes);


        var saveBytes = JsonSerializer.SerializeToUtf8Bytes(state, options);
        var saveValue = ByteString.CopyFrom(saveBytes);

        Assert.Equal(transactionValue, saveValue);
    }
}

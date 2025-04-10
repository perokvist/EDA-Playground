using System.Data;

namespace Sample.App.Dapr;

public class DaprEventStore(global::Dapr.Client.DaprClient client) //, ILogger<DaprEventStore> logger)
{
    public static class Naming
    {
        public static string StreamKey(string streamName, long version) => $"{streamName}|{version}";

        public static string StreamHead(string streamName) => $"{streamName}|head";
    }

    public string StoreName { get; set; } = "statestore";

    public Func<string, Dictionary<string, string>> MetaProvider { get; set; } = streamName => [];

    public Task<long> AppendToStreamAsync(string streamName, long version, params EventData[] events)
    => AppendToStreamAsync(
        streamName,
        Concurrency.Match(version),
        events);

    public Task<long> AppendToStreamAsync(string streamName, params EventData[] events)
        => AppendToStreamAsync(
            streamName,
            Concurrency.Ignore(),
            events);

    public async Task<long> AppendToStreamAsync(string streamName, Action<StreamHead> concurrencyGuard, params EventData[] events)
    {
        var streamHeadKey = Naming.StreamHead(streamName);
        var meta = MetaProvider(streamName);
        var (head, headetag) = await client.GetStateAndETagAsync<StreamHead>(StoreName, streamHeadKey, metadata: meta);

        if (head == null)
            head = new StreamHead();

        if (events.Length == 0)
            return head.Version;

        concurrencyGuard(head);

        var newVersion = head.Version + events.Length;
        var versionedEvents = events
            .Select((e, i) => new EventData(e.EventId, e.EventName, streamName, e.Data, head.Version + (i + 1)))
            .ToArray();

        head = new StreamHead(newVersion);

        await client.StateTransactionAsync(StoreName, streamName, streamHeadKey, head, headetag, meta, versionedEvents);

        return newVersion;
    }

    public async Task<StreamHead?> GetStreamMetaData(string streamName)
    {
        var meta = MetaProvider(streamName);

        var head = await client.GetStateEntryAsync<StreamHead>(StoreName, $"{streamName}|head", metadata: meta);

        return head.Value;
    }

    public async IAsyncEnumerable<EventData> LoadEventStreamAsync(string streamName, long version)
    {
        var head = await GetStreamMetaData(streamName);

        if (head == null)
            yield break;

        var meta = MetaProvider(streamName);

        await foreach (var e in client.LoadAsyncBulkEventsAsync(StoreName, streamName, version, meta, head))
            yield return e;
        yield break;
    }

    public record StreamHead(long Version = 0)
    {
        public StreamHead() : this(0)
        { }
    }

    public class Concurrency
    {
        public static Action<StreamHead> Match(long version) => head =>
        {
            if (head.Version != version)
                throw new DBConcurrencyException($"wrong version - expected {version} but was {head.Version}");
        };

        public static Action<StreamHead> Ignore() => _ => { };
    }
}

public record EventData(string EventId, string EventName, string StreamName, object Data, long Version = 0)
{
    public static EventData Create(string eventName, object data, long version = 0) => new(Guid.NewGuid().ToString(), eventName, "unknown", data, version);

    public static EventData Create(string eventName, string streamName, object data, long version = 0) => new(Guid.NewGuid().ToString(), eventName, streamName, data, version);
}


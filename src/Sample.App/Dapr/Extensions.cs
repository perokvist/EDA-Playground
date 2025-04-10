using Dapr.Client;
using Sample.App.Core;
using System.Buffers;
using System.Data;
using System.Text.Json;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using static Sample.App.Dapr.DaprEventStore;

namespace Sample.App.Dapr;

public static class Extensions
{
    public static DaprEventStore PartitionPerStream(this DaprEventStore daprEventStore)
    {
        daprEventStore.MetaProvider = streamName => new Dictionary<string, string>
                {
                    { "partitionKey", streamName },
                    { "contentType", "application/json" }
                };
        return daprEventStore;
    }

    public static DaprEventStore PartitionAllStream(this DaprEventStore daprEventStore, string allStream = "all")
        => daprEventStore.PartitionCustom(allStream);

    public static DaprEventStore PartitionCustom(this DaprEventStore daprEventStore, string partitionKey)
    {
        daprEventStore.MetaProvider = streamName => new Dictionary<string, string>
                {
                    { "partitionKey", partitionKey },
                    { "contentType", "application/json" }
                };
        return daprEventStore;
    }

    public static async Task<(IEnumerable<EventData> Events, long Version)> LoadSlicesAsync(
        this DaprClient client, string storeName, ILogger logger,
        string streamName, long version, Dictionary<string, string> meta, StreamHead head)
    {
        var eventSlices = new List<EventData[]>();
        using (logger.BeginScope("Loading head {streamName}. Starting version {version}", streamName, head.Version))
        {
            var next = head.Version;

            while (next != 0 && next >= version)
            {
                var sliceKey = Naming.StreamKey(streamName, next);
                var slice = await client.GetStateAsync<EventData[]>(storeName, sliceKey, metadata: meta);

                logger.LogDebug("Slice {sliceKey} loaded range : {firstVersion} - {lastVersion}", sliceKey, slice.First().Version, slice.Last().Version);
                next = slice.First().Version - 1;

                if (next < version)
                {
                    logger.LogDebug("Version within slice. Next : {next}. Version : {version}", next, version);
                    eventSlices.Add(slice.Where(e => e.Version >= version).ToArray());
                    break;
                }

                logger.LogDebug("Adding slice. Next : {next}. Version : {version}", next, version);

                eventSlices.Add(slice);
            }

            logger.LogDebug("Done reading. Got {sliceCount} slices.", eventSlices.Count);

            var events = eventSlices
                .Reverse<EventData[]>()
                .SelectMany(e => e)
                .ToArray();

            return (events, events.LastOrDefault()?.Version ?? head.Version);
        }
    }
        public static async IAsyncEnumerable<EventData> LoadAsyncBulkEventsAsync(
        this DaprClient client, string storeName,
        string streamName, long version, Dictionary<string, string> meta, StreamHead head, int chunkSize = 20)
    {
        var keys = Enumerable
            .Range(version == default ? 1 : (int)version, (int)(head.Version) + (version == default ? default : 1))
            .Where(x => x <= head.Version)
            .Select(x => Naming.StreamKey(streamName, x))
            .ToList();

        if (!keys.Any())
            yield break;

        foreach (var chunk in keys.BuildChunks(chunkSize))
        {
            var events = (await client.GetBulkStateAsync(storeName, chunk.ToArray(), null, metadata: meta))
                .Select(x => JsonSerializer.Deserialize<EventData>(x.Value))
                .OrderBy(x => x.Version);

            foreach (var e in events)
                yield return e;
        }
    }

    static IEnumerable<IEnumerable<T>> BuildChunks<T>(this IEnumerable<T> list, int batchSize)
    {
        int total = 0;
        while (total < list.Count())
        {
            yield return list.Skip(total).Take(batchSize);
            total += batchSize;
        }
    }

    public static async Task StateTransactionAsync(this DaprClient client,
        string storeName,
        string streamName, string streamHeadKey, StreamHead head, string headetag, Dictionary<string, string> meta, EventData[] versionedEvents)
    {
        var eventsReq = versionedEvents.Select(x => new StateTransactionRequest(
                Naming.StreamKey(streamName, x.Version), 
                JsonSerializer.SerializeToUtf8Bytes(x, client.JsonSerializerOptions), StateOperationType.Upsert, metadata: meta));
        var headReq = new StateTransactionRequest(streamHeadKey, JsonSerializer.SerializeToUtf8Bytes(head), StateOperationType.Upsert,
            metadata: meta,
            etag: string.IsNullOrWhiteSpace(headetag) ? null : headetag);
        var reqs = new List<StateTransactionRequest>();
        reqs.AddRange(eventsReq);
        reqs.Add(headReq);

        //var outboxOperations = versionedEvents.Select(x => new StateTransactionRequest(
        //key: Naming.StreamKey(streamName, x.Version),
        //       value: JsonSerializer.SerializeToUtf8Bytes(new[] { new SampleEvent(Guid.NewGuid(), "temp pub") }, client.JsonSerializerOptions),
        //       operationType: StateOperationType.Upsert,
        //       metadata: new Dictionary<string, string>
        //       {
        //        { "outbox.projection", "true" },
        //        { "contentType", "application/json" },
        //        { "datacontenttype", "application/json" }
        //       }
        //   ));

        //reqs.AddRange(outboxOperations);

        await client.ExecuteStateTransactionAsync(storeName, reqs, metadata: meta);
    }

    public static T EventAs<T>(this EventData eventData, JsonSerializerOptions options = null)
     => eventData.Data switch
     {
         JsonElement d => d.ToObject<T>(options),
         T d => d,
         _ => throw new Exception($"Data was not of type {typeof(T).Name}")
     };

    public static T ToObject<T>(this JsonElement element, JsonSerializerOptions options = null)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(bufferWriter))
            element.WriteTo(writer);
        var result = JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
        return result;
    }
}

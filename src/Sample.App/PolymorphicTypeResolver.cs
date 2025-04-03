using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using Sample.App.Core;

namespace Sample.App;

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        Type basePointType = typeof(Event);
        if (jsonTypeInfo.Type == basePointType)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(DomainEvent), nameof(DomainEvent).ToLower()),
                    new JsonDerivedType(typeof(IntegrationEvent), nameof(IntegrationEvent).ToLower()),
                    new JsonDerivedType(typeof(SampleEvent), nameof(SampleEvent).ToLower()),
                    new JsonDerivedType(typeof(SampleIntegrationEvent), nameof(SampleIntegrationEvent).ToLower()),
                }
            };
        }

        return jsonTypeInfo;
    }
}

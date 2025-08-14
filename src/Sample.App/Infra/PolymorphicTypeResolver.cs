using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using Sample.App.Core;
using Sample.App.Modules.Sample;

namespace Sample.App.Infra;

public class PolymorphicEventTypeResolver() : PolymorphicTypeResolver<Event>(true, typeof(DomainEvent), typeof(IntegrationEvent), typeof(SampleEvent), typeof(SampleIntegrationEvent));

public class PolymorphicStateTypeResolver() : PolymorphicTypeResolver<State>(false, typeof(SampleState));


public class PolymorphicTypeResolver<T>(bool nullIfNoMatch = true, params Type[] derivedTypes) : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        Type basePointType = typeof(T);
        if (jsonTypeInfo.Type == basePointType)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };

            derivedTypes
                .Select(x => new JsonDerivedType(x, x.Name.ToLower()))
                .ToList()
                .ForEach(jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add);

            return jsonTypeInfo;
        }

        return nullIfNoMatch ? null : jsonTypeInfo;
    }
}



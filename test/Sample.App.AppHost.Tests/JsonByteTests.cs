using System.Text.Json;
using Google.Protobuf;

namespace Sample.App.AppHost.Tests;

public class JsonByteTests
{
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

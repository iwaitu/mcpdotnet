using System.Threading.Channels;
using McpDotNet.Client;
using McpDotNet.Configuration;
using McpDotNet.Protocol.Messages;
using McpDotNet.Protocol.Transport;
using McpDotNet.Protocol.Types;

namespace McpDotNet.Tests.Client;

public class McpClientFactoryTests
{
    private readonly McpClientOptions _defaultOptions = new()
    {
        ClientInfo = new() { Name = "TestClient", Version = "1.0.0" }
    };

    [Fact]
    public async Task CreateAsync_WithInvalidArgs_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>("serverConfig", () => McpClientFactory.CreateAsync((McpServerConfig)null!, _defaultOptions));

        await Assert.ThrowsAsync<ArgumentNullException>("clientOptions", () => McpClientFactory.CreateAsync(
            new McpServerConfig()
            {
                Name = "name",
                Id = "id",
                TransportType = TransportTypes.StdIo,
            }, (McpClientOptions)null!));

        await Assert.ThrowsAsync<ArgumentException>("serverConfig", () => McpClientFactory.CreateAsync(
            new McpServerConfig()
            {
                Name = "name",
                Id = "id",
                TransportType = "somethingunsupported",
            },
            _defaultOptions));

        await Assert.ThrowsAsync<InvalidOperationException>(() => McpClientFactory.CreateAsync(
            new McpServerConfig()
            {
                Name = "name",
                Id = "id",
                TransportType = TransportTypes.StdIo,
            },
            _defaultOptions,
            (_, __) => null!));
    }

    [Fact]
    public async Task CreateAsync_WithValidStdioConfig_CreatesNewClient()
    {
        // Arrange
        var serverConfig = new McpServerConfig
        {
            Id = "test-server",
            Name = "Test Server",
            TransportType = TransportTypes.StdIo,
            Location = "/path/to/server",
            TransportOptions = new Dictionary<string, string>
            {
                ["arguments"] = "--test arg",
                ["workingDirectory"] = "/working/dir"
            }
        };

        // Act
        var client = await McpClientFactory.CreateAsync(
            serverConfig,
            _defaultOptions,
            (_, __) => new NopTransport());

        // Assert
        Assert.NotNull(client);
        // We could add more assertions here about the client's configuration
    }

    [Fact]
    public async Task CreateAsync_WithNoTransportOptions_CreatesNewClient()
    {
        // Arrange
        var serverConfig = new McpServerConfig
        {
            Id = "test-server",
            Name = "Test Server",
            TransportType = TransportTypes.StdIo,
            Location = "/path/to/server",
        };

        // Act
        var client = await McpClientFactory.CreateAsync(
            serverConfig,
            _defaultOptions,
            (_, __) => new NopTransport());

        // Assert
        Assert.NotNull(client);
        // We could add more assertions here about the client's configuration
    }

    [Fact]
    public async Task CreateAsync_WithValidSseConfig_CreatesNewClient()
    {
        // Arrange
        var serverConfig = new McpServerConfig
        {
            Id = "test-server",
            Name = "Test Server",
            TransportType = TransportTypes.Sse,
            Location = "http://localhost:8080"
        };

        // Act
        var client = await McpClientFactory.CreateAsync(
            serverConfig,
            _defaultOptions,
            (_, __) => new NopTransport());

        // Assert
        Assert.NotNull(client);
        // We could add more assertions here about the client's configuration
    }

    [Fact]
    public async Task CreateAsync_WithSse_CreatesCorrectTransportOptions()
    {
        // Arrange
        var serverConfig = new McpServerConfig
        {
            Id = "test-server",
            Name = "Test Server",
            TransportType = TransportTypes.Sse,
            Location = "http://localhost:8080",
            TransportOptions = new Dictionary<string, string>
            {
                ["connectionTimeout"] = "10",
                ["maxReconnectAttempts"] = "2",
                ["reconnectDelay"] = "5",
                ["header.test"] = "the_header_value"
            }
        };

        // Act
        var client = await McpClientFactory.CreateAsync(
            serverConfig,
            _defaultOptions,
            (_, __) => new NopTransport());

        // Assert
        Assert.NotNull(client);
        // We could add more assertions here about the client's configuration
    }

    [Theory]
    [InlineData("connectionTimeout", "not_a_number")]
    [InlineData("maxReconnectAttempts", "invalid")]
    [InlineData("reconnectDelay", "bad_value")]
    public async Task McpFactory_WithInvalidTransportOptions_ThrowsFormatException(string key, string value)
    {
        // arrange
        var config = new McpServerConfig
        {
            Id = "test-server",
            Name = "Test Server",
            TransportType = TransportTypes.Sse,
            Location = "http://localhost:8080",
            TransportOptions = new Dictionary<string, string>
            {
                [key] = value
            }
        };

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(() => McpClientFactory.CreateAsync(config, _defaultOptions));
    }

    private sealed class NopTransport : IClientTransport
    {
        private readonly Channel<IJsonRpcMessage> _channel = Channel.CreateUnbounded<IJsonRpcMessage>();

        public bool IsConnected => true;

        public ChannelReader<IJsonRpcMessage> MessageReader => _channel.Reader;

        public Task ConnectAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public ValueTask DisposeAsync() => default;

        public Task SendMessageAsync(IJsonRpcMessage message, CancellationToken cancellationToken = default)
        {
            switch (message)
            {
                case JsonRpcRequest request:
                    _channel.Writer.TryWrite(new JsonRpcResponse
                    {
                        Id = ((JsonRpcRequest)message).Id,
                        Result = new InitializeResult()
                        {
                            Capabilities = new ServerCapabilities(),
                            ProtocolVersion = "2024-11-05",
                            ServerInfo = new Implementation()
                            {
                                Name = "NopTransport",
                                Version = "1.0.0"
                            },
                        }
                    });
                    break;
            }

            return Task.CompletedTask;
        }
    }
}

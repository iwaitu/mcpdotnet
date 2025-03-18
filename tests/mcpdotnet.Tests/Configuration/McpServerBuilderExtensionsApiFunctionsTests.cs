using McpDotNet.Configuration;
using McpDotNet.Protocol.Types;
using McpDotNet.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using static McpDotNet.Tests.Configuration.McpServerBuilderExtensionsToolsTests;

namespace McpDotNet.Tests.Configuration
{
    public class McpServerBuilderExtensionsApiFunctionsTests
    {
        private readonly Mock<IMcpServerBuilder> _builder;
        private readonly ServiceCollection _services;

        public McpServerBuilderExtensionsApiFunctionsTests()
        {
            _services = new ServiceCollection();
            _builder = new Mock<IMcpServerBuilder>();
            _builder.SetupGet(b => b.Services).Returns(_services);
        }

        [Fact]
        public void Adds_Tools_To_Server()
        {
            
        }
    }
}

using Xunit;

namespace DiscordChatExporter.Cli.Tests.Fixtures;

[CollectionDefinition(nameof(ExportWrapperCollection))]
public class ExportWrapperCollection : ICollectionFixture<ExportWrapperFixture>
{
}
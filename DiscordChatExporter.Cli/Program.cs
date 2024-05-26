using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CliFx;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;

namespace DiscordChatExporter.Cli;

public static class Program
{
    // Explicit references because CliFx relies on reflection and we're publishing with trimming enabled
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExportAllCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExportChannelsCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExportDirectMessagesCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExportGuildCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GetChannelsCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GetDirectChannelsCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GetGuildsCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GuideCommand))]
    [DynamicDependency(
        DynamicallyAccessedMemberTypes.All,
        typeof(ThreadInclusionModeBindingConverter)
    )]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TruthyBooleanBindingConverter))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PartitionLimit))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MessageFilter))]
    public static async Task<int> Main(string[] args) =>
        await new CliApplicationBuilder()
            .AddCommand<ExportAllCommand>()
            .AddCommand<ExportChannelsCommand>()
            .AddCommand<ExportDirectMessagesCommand>()
            .AddCommand<ExportGuildCommand>()
            .AddCommand<GetChannelsCommand>()
            .AddCommand<GetDirectChannelsCommand>()
            .AddCommand<GetGuildsCommand>()
            .AddCommand<GuideCommand>()
            .Build()
            .RunAsync(args);
}

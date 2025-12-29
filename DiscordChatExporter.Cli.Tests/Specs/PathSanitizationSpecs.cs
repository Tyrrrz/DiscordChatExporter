using System.IO;
using DiscordChatExporter.Core.Utils.Extensions;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class PathSanitizationSpecs
{
    [Fact]
    public void Path_with_question_mark_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("How to do this?");

        // Assert
        result.Should().Be("How to do this_");
    }

    [Fact]
    public void Path_with_colon_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("Title: Subtitle");

        // Assert
        result.Should().Be("Title_ Subtitle");
    }

    [Fact]
    public void Path_with_asterisk_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("File*Name");

        // Assert
        result.Should().Be("File_Name");
    }

    [Fact]
    public void Path_with_backslash_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("Path\\File");

        // Assert
        result.Should().Be("Path_File");
    }

    [Fact]
    public void Path_with_forward_slash_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("Path/File");

        // Assert
        result.Should().Be("Path_File");
    }

    [Fact]
    public void Path_with_double_quote_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("Say \"Hello\"");

        // Assert
        result.Should().Be("Say _Hello_");
    }

    [Fact]
    public void Path_with_less_than_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("Value<10");

        // Assert
        result.Should().Be("Value_10");
    }

    [Fact]
    public void Path_with_greater_than_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("Value>10");

        // Assert
        result.Should().Be("Value_10");
    }

    [Fact]
    public void Path_with_pipe_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("Option A | Option B");

        // Assert
        result.Should().Be("Option A _ Option B");
    }

    [Fact]
    public void Path_with_multiple_invalid_characters_is_sanitized()
    {
        // Act
        var result = Path.EscapeFileName("How? Why: This/That <works> |right|");

        // Assert
        result.Should().Be("How_ Why_ This_That _works_ _right_");
    }

    [Fact]
    public void Path_with_valid_characters_is_not_changed()
    {
        // Act
        var result = Path.EscapeFileName("Valid File Name 123");

        // Assert
        result.Should().Be("Valid File Name 123");
    }

    [Fact]
    public void Path_with_special_but_valid_characters_is_not_changed()
    {
        // Act
        var result = Path.EscapeFileName("File (with) [brackets] & symbols! @#$%^");

        // Assert
        result.Should().Be("File (with) [brackets] & symbols! @#$%^");
    }
}

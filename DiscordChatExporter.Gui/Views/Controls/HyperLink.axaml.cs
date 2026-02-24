using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DiscordChatExporter.Gui.Utils.Extensions;

namespace DiscordChatExporter.Gui.Views.Controls;

public partial class HyperLink : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<HyperLink>();

    public static readonly StyledProperty<ICommand?> CommandProperty =
        Button.CommandProperty.AddOwner<HyperLink>();

    public static readonly StyledProperty<object?> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<HyperLink>();

    // If Url is set and Command is not set, clicking will open this URL in the default browser.
    public static readonly StyledProperty<string?> UrlProperty = AvaloniaProperty.Register<
        HyperLink,
        string?
    >(nameof(Url));

    public HyperLink() => InitializeComponent();

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public string? Url
    {
        get => GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    private void TextBlock_OnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (Command is not null)
        {
            if (Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }
        else if (
            !string.IsNullOrWhiteSpace(Url)
            && Uri.TryCreate(Url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        )
        {
            Process.StartShellExecute(uri.AbsoluteUri);
        }
    }
}

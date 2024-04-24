using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace DiscordChatExporter.Gui.Views.Controls;

public partial class HyperLink : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<HyperLink>();

    public static readonly StyledProperty<ICommand?> CommandProperty =
        Button.CommandProperty.AddOwner<HyperLink>();

    public static readonly StyledProperty<object?> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<HyperLink>();

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

    private void TextBlock_OnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (Command is null)
            return;

        if (!Command.CanExecute(CommandParameter))
            return;

        Command.Execute(CommandParameter);
    }
}

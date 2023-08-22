using System.Windows;

namespace DiscordChatExporter.Gui.Views.Controls;

public partial class RevealablePasswordBox
{
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
        nameof(Password),
        typeof(string),
        typeof(RevealablePasswordBox),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
        )
    );

    public static readonly DependencyProperty IsRevealedProperty = DependencyProperty.Register(
        nameof(IsRevealed),
        typeof(bool),
        typeof(RevealablePasswordBox),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None)
    );

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public bool IsRevealed
    {
        get => (bool)GetValue(IsRevealedProperty);
        set => SetValue(IsRevealedProperty, value);
    }

    public RevealablePasswordBox()
    {
        InitializeComponent();
    }
}

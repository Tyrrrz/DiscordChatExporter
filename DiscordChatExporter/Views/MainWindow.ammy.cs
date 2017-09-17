using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DiscordChatExporter.ViewModels;

namespace DiscordChatExporter.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void TokenTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Flush binding
                var textBox = (TextBox)sender;
                var binding = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
                binding?.UpdateSource();

                // Execute command
                ((IMainViewModel) DataContext).PullChannelsCommand.Execute(null);
            }
        }
    }
}
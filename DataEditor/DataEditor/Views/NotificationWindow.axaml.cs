using Avalonia.Controls;

namespace DataEditor;

public partial class NotificationWindow : Window
{
    public NotificationWindow(
        string textBoxText = "Notification text",
        string headerText = "Warning",
        string okButtonText = "OK")
    {
        InitializeComponent();

        this.Title = headerText;
        InfoArea.Text = textBoxText;
        OkButton.Content = okButtonText;

        OkButton.Click += OkButton_Click;
    }

    private void OkButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(true);
    }
}
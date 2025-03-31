using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DataEditor.ViewModels;
using DataEditor.Models.Enums;
using DataEditor.Support;

namespace DataEditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
            AppData.Instance.Wnd = this;
            this.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

            ShowScreen(Screen.DefaultScreen);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            if (ModesPanel.IsVisible)
            {
                viewModel?.KeyDownModeCommand.Execute(e);
            } else if (StepsPanel.IsVisible)
            {
                viewModel?.KeyDownStepCommand.Execute(e);
            }
        }

        private void ShowScreen(Screen screen)
        {
            DefaultScreen.IsVisible = screen == Screen.DefaultScreen;
            LogInScreen.IsVisible = screen == Screen.LogIn;
            SignUpScreen.IsVisible = screen == Screen.SignUp;
            EmptyMainScreen.IsVisible = screen == Screen.EmptyMainScreen;
            NavigationArea.IsVisible = screen == Screen.EmptyMainScreen;
        }

        private void ShowPanel(DataEditor.Models.Enums.Panel panel)
        {
            ModesPanel.IsVisible = panel == DataEditor.Models.Enums.Panel.Mode;
            StepsPanel.IsVisible = panel == DataEditor.Models.Enums.Panel.Steps;
        }

        private void OnLogInButtonClick(object sender, RoutedEventArgs e)
        {
            ShowScreen(Screen.LogIn);
        }

        private void OnSignUpButtonClick(object sender, RoutedEventArgs e)
        {
            ShowScreen(Screen.SignUp);
        }

        private void OnLogInOkButtonClick(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text;
            var password = this.FindControl<TextBox>("PasswordTextBox").Text;

            var result = DatabaseHelper.ValidateUser(login, password);
            switch (result)
            {
                case LoginResult.OK:
                    ShowScreen(Screen.EmptyMainScreen);
                    break;
                case LoginResult.WrongPassword:
                    LogInStatusLabel.Text = "Invalid password.";
                    break;
                case LoginResult.NoUser:
                    LogInStatusLabel.Text = "User not found.";
                    break;
                case LoginResult.InvalidInput:
                    LogInStatusLabel.Text = "Please enter both login and password.";
                    break;
            }
        }

        private void OnSignUpOkButtonClick(object sender, RoutedEventArgs e)
        {
            var login = SignUpLoginTextBox.Text;
            var password = this.FindControl<TextBox>("SignUpPasswordTextBox").Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                SignUpStatusLabel.Text = "Please enter both login and password.";
                return;
            }

            if (!DatabaseHelper.IsValidPassword(password))
            {
                SignUpStatusLabel.Text = "Password must be at least 6 characters long and contain at least one letter and one number.";
                return;
            }

            var success = DatabaseHelper.CreateUser(login, password);
            if (success)
            {
                ShowScreen(Screen.EmptyMainScreen);
            }
            else
            {
                SignUpStatusLabel.Text = "User already exists. Please create an unique one.";
            }
        }

        private void OnModsButtonClick(object sender, RoutedEventArgs e)
        {
            ShowPanel(DataEditor.Models.Enums.Panel.Mode);
        }
        private void OnStepsButtonClick(object sender, RoutedEventArgs e)
        {
            ShowPanel(DataEditor.Models.Enums.Panel.Steps);
        }
        private void OnSigoutButtonClick(object sender, RoutedEventArgs e)
        {
            ShowPanel(DataEditor.Models.Enums.Panel.Empty);
            ShowScreen(Screen.DefaultScreen);
        }
    }
}
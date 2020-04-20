using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            usernameTxt.Text = "Idan1";
            passwordBox.Password = "Idan1";
        }

        private void Button_Click1(object sender, System.Windows.RoutedEventArgs e)
        {
            usernameTxt.Text = "Uriel1";
            passwordBox.Password = "Uriel1";
        }
    }
}

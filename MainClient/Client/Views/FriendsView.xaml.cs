using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for FriendsView.xaml
    /// </summary>
    public partial class FriendsView : UserControl
    {
        public FriendsView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var tBox = sender as TextBox;
                searchByQueryButton.Command.Execute(tBox.Text);
            }
        }

    }
}

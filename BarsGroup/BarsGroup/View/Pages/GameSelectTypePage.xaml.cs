using BarsGroup.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace BarsGroup.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для GameSelectTypePage.xaml
    /// </summary>
    public partial class GameSelectTypePage : Page
    {
        public GameSelectTypePage()
        {
            InitializeComponent();
            if (Core.CurrentUser.ID == 0)
            {
                LogInBtn.Visibility = Visibility.Visible;
            }
        }

        private void StartGameBot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartGameHuman_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser.ID == 0)
            {
                MessageBox.Show("Для игры в этом режиме необходимо авторизоваться", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            this.NavigationService.Navigate(new SearchGamePage());
        }

        private void StartGameLocal_Click(object sender, RoutedEventArgs e)
        {
            Windows.PlayingFieldLocalWindow playingFieldLocal = new Windows.PlayingFieldLocalWindow();
            playingFieldLocal.Show();
        }

        private void LogInBtn_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogInPage());
        }
    }
}

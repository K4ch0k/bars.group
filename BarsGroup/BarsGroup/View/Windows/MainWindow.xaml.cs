using BarsGroup.Model;
using BarsGroup.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BarsGroup.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Pages.LogInPage());
            try
            {
                var a = Core.db.Database.Connection;
                SeeScoreBtn.Visibility = Visibility.Visible;
            }
            catch
            {
                MessageBox.Show("Отсутсвует соединение с БД.\nФункции приложения ограничены", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }

        // Действия, происходящие при навигации (переход с одной страницы на другую)
        private void MainFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (Core.CurrentUser.ID != 0)
            {
                StartNewGameBtn.Visibility = Visibility.Visible;
                StartGameBtn.Visibility = Visibility.Visible;
            }
        }


        #region Кнопки на главной
        private void StartGameBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartNewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.GameSelectTypePage());
        }

        private void SeeScoreBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ScoreTablePage());
        }

        private async void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser.ID != 0)
            {
                try
                {
                    var ThisUser = await Core.db.Users.FindAsync(Core.CurrentUser.ID);
                    ThisUser.LoggedIn = false;
                    ThisUser.StatusID = 1;
                    await Core.db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (ex.InnerException.InnerException == null)
                    {
                        MessageBox.Show(ex.InnerException.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MessageBox.Show(ex.InnerException.ToString() + "\n" +
                                      ex.InnerException.InnerException.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Core.CurrentUser = new Users();

                StartNewGameBtn.Visibility = Visibility.Hidden;
                StartGameBtn.Visibility = Visibility.Hidden;

                MainFrame.Navigate(new Pages.LogInPage());
            }
            else
                this.Close();
        }

        #endregion

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Core.CurrentUser.ID != 0)
            {
                try
                {
                    var ThisUser = await Core.db.Users.FindAsync(Core.CurrentUser.ID);
                    ThisUser.LoggedIn = false;
                    ThisUser.StatusID = 1;
                    await Core.db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (ex.InnerException.InnerException == null)
                    {
                        MessageBox.Show(ex.InnerException.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MessageBox.Show(ex.InnerException.ToString() + "\n" +
                                      ex.InnerException.InnerException.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Core.CurrentUser = new Users();
            }
        }
    }
}

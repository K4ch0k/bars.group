using BarsGroup.Model;
using BarsGroup.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BarsGroup.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для LogInPage.xaml
    /// </summary>
    public partial class LogInPage : Page
    {
        public LogInPage()
        {
            InitializeComponent();
        }

        public List<Users> AllUsers { get; set; } = new List<Users>();

        #region Показать/Скрыть пароль
        private void SeePswrdBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SeePswrdLbl.Content = PasswordTxtBox.Password;
            SeePswrdLbl.Visibility = Visibility.Visible;
            PasswordTxtBox.Visibility = Visibility.Hidden;
        }

        private void SeePswrdBtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordTxtBox.Visibility = Visibility.Visible;
            SeePswrdLbl.Visibility = Visibility.Hidden;
        }
        #endregion

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RegisterPage());
        }

        private async void LogInBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder errors = new StringBuilder();
                if (String.IsNullOrWhiteSpace(LoginTxtBox.Text))
                    errors.AppendLine("Поле \"Логин\" обязательно для заполнения");
                else if (LoginTxtBox.Text.Contains(" "))
                    errors.AppendLine("Поле \"Логин\" не должно содержать пробелов");
                if (String.IsNullOrWhiteSpace(PasswordTxtBox.Password))
                    errors.AppendLine("Поле \"Пароль\" обязательно для заполнения");
                else if (PasswordTxtBox.Password.Contains(" "))
                    errors.AppendLine("Поле \"Пароль\" не должно содержать пробелов");
                if (errors.Length > 0)
                {
                    MessageBox.Show(errors.ToString(), "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                try
                {
                    Core.db = new MainEntities();
                    AllUsers = Core.db.Users.ToList();
                }
                catch
                {
                    MessageBox.Show("Отсутсвует соединение с БД.\nАвторизация невозможна", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                var SearchUser = AllUsers.Find(item => item.Login == LoginTxtBox.Text);
                if (SearchUser == null)
                {
                    MessageBox.Show("Пользователь не найден", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                if (SearchUser.Password != PasswordTxtBox.Password)
                {
                    MessageBox.Show("Данные введены некорректно", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                
                if (SearchUser.LoggedIn == true)
                {
                    MessageBox.Show("Пользователь уже авторизовался в системе!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                
                SearchUser.LoggedIn = true;
                SearchUser.StatusID = 2;
                await Core.db.SaveChangesAsync();

                Core.CurrentUser = SearchUser;

                this.NavigationService.Navigate(new GameSelectTypePage());
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
        }
    }
}

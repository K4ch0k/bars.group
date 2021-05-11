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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
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

        private void LogInBtn_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogInPage());
        }

        private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
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
                if (String.IsNullOrWhiteSpace(Password2TxtBox.Password))
                    errors.AppendLine("Поле \"Повторите пароль\" обязательно для заполнения");
                else if (Password2TxtBox.Password.Contains(" "))
                    errors.AppendLine("Поле \"Повторите пароль\" не должно содержать пробелов");

                if (PasswordTxtBox.Password != Password2TxtBox.Password)
                    errors.AppendLine("Поля \"Пароль\" и \"Повторите пароль\" должны совпадать");

                if (errors.Length > 0)
                {
                    MessageBox.Show(errors.ToString(), "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                Users SearchUser;
                try
                {
                    Core.db = new MainEntities();
                    AllUsers = Core.db.Users.ToList();
                    SearchUser = AllUsers.Find(item => item.Login == LoginTxtBox.Text);
                }
                catch
                {
                    MessageBox.Show("Отсутсвует соединение с БД.\nРегистрация невозможна", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (SearchUser != null)
                {
                    MessageBox.Show("Пользователь c указанным логином уже существует", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                Users NewUser = new Users
                {
                    Login = LoginTxtBox.Text,
                    Password = PasswordTxtBox.Password,
                    StatusID = 1
                };
                Core.db.Users.Add(NewUser);
                await Core.db.SaveChangesAsync();

                Statistics NewUserStatisticsBot = new Statistics
                {
                    Quantity = 0,
                    QuantityWin = 0,
                    QuantityLose = 0,
                    GameTypeID = 1,
                    UserID = NewUser.ID
                };
                Core.db.Statistics.Add(NewUserStatisticsBot);

                Statistics NewUserStatisticsOnLine = new Statistics
                {
                    Quantity = 0,
                    QuantityWin = 0,
                    QuantityLose = 0,
                    GameTypeID = 2,
                    UserID = NewUser.ID
                };
                Core.db.Statistics.Add(NewUserStatisticsOnLine);

                await Core.db.SaveChangesAsync();

                this.NavigationService.Navigate(new LogInPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString() + "\n" +
                    ex.InnerException.InnerException.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using BarsGroup.Model;
using BarsGroup.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BarsGroup.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для SearchGamePage.xaml
    /// </summary>
    public partial class SearchGamePage : Page
    {
        public SearchGamePage()
        {
            InitializeComponent();
        }

        #region Bindings
        public DispatcherTimer Timer = new DispatcherTimer();
        public int Seconds { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Hours { get; set; } = 0;

        public UsersInOnLineGame ThisUserInGame { get; set; } = new UsersInOnLineGame();

        public bool GameFind { get; set; } = false;

        #endregion

        public void StartTimer()
        {
            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += new EventHandler(TickTimer);
            Timer.Start();
        }

        public async void TickTimer(object sender, EventArgs e)
        {
            Seconds++;
            if (Seconds == 59)
            {
                Seconds = 0;
                Minutes++;
            }
            if (Minutes == 59)
            {
                Minutes = 0;
                Hours++;
            }
            MainTimer.Text = string.Format("{0:00}:{1:00}:{2:00}", Hours, Minutes, Seconds);

            try
            {
                Core.db = new MainEntities();

                List<UsersInOnLineGame> AllUsersInGame = Core.db.UsersInOnLineGame.Where(item => item.GameID == ThisUserInGame.GameID).ToList();
                if (AllUsersInGame.Count() == 2)
                {
                    Timer.Stop();
                    var USerChangeStatus = await Core.db.Users.FindAsync(Core.CurrentUser.ID);
                    USerChangeStatus.StatusID = 4;

                    var GameChangeStatus = await Core.db.OnLineGames.FindAsync(ThisUserInGame.GameID);
                    GameChangeStatus.StatusID = 2;

                    OnLineGames GameConnect = await Core.db.OnLineGames.FindAsync(ThisUserInGame.GameID);
                    GameConnect.DateStart = DateTime.Now;
                    GameConnect.LocationOfCheckers = "";

                    await Core.db.SaveChangesAsync();
                    Core.CurrentUser = USerChangeStatus;

                    Windows.PlayingFieldHumanWindow NewPlayingField = new Windows.PlayingFieldHumanWindow(ThisUserInGame);
                    GameFind = true;
                    this.NavigationService.Navigate(new GameSelectTypePage());
                    NewPlayingField.ShowDialog();
                }
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Core.db = new MainEntities();
                var AllGames = Core.db.OnLineGames.ToList();
                OnLineGames FindGame = AllGames.Find(item => item.StatusID == 1);
                if (FindGame == null)
                {
                    OnLineGames CreateNewGame = new OnLineGames()
                    {
                        StatusID = 1
                    };
                    Core.db.OnLineGames.Add(CreateNewGame);
                    await Core.db.SaveChangesAsync();

                    var ChangeStatusUser = await Core.db.Users.FindAsync(Core.CurrentUser.ID);
                    ChangeStatusUser.StatusID = 3;

                    ThisUserInGame = new UsersInOnLineGame()
                    {
                        UserID = Core.CurrentUser.ID,
                        GameID = CreateNewGame.ID,
                        StatusID = 1
                    };

                    Core.db.UsersInOnLineGame.Add(ThisUserInGame);
                    await Core.db.SaveChangesAsync();
                    Core.CurrentUser = ChangeStatusUser;
                    StartTimer();
                }
                else
                {
                    var AllUsersInGame = Core.db.UsersInOnLineGame.Where(item => item.GameID == FindGame.ID).ToList();

                    if (AllUsersInGame.Count() == 1)
                    {
                        var ChangeStatus = await Core.db.Users.FindAsync(Core.CurrentUser.ID);
                        ChangeStatus.StatusID = 4;

                        ThisUserInGame = new UsersInOnLineGame()
                        {
                            UserID = Core.CurrentUser.ID,
                            GameID = FindGame.ID,
                            StatusID = 2
                        };

                        Core.db.UsersInOnLineGame.Add(ThisUserInGame);
                        await Core.db.SaveChangesAsync();
                        Core.CurrentUser = ChangeStatus;

                        Windows.PlayingFieldHumanWindow NewPlayingField = new Windows.PlayingFieldHumanWindow(ThisUserInGame);
                        GameFind = true;
                        this.NavigationService.Navigate(new GameSelectTypePage());
                        NewPlayingField.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось подключиться к игре", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.NavigationService.Navigate(new GameSelectTypePage());
                    }
                }
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

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GameFind == false)
                {
                    if (ThisUserInGame.StatusID == 1)
                    {
                        var AllUsersInGames = Core.db.OnLineGames.ToList();
                        var GameChange = AllUsersInGames.Find(item => item.ID == ThisUserInGame.GameID);
                        GameChange.StatusID = 3;
                    }
                    var CurrentUser = await Core.db.Users.FindAsync(Core.CurrentUser.ID);
                    CurrentUser.StatusID = 2;
                    await Core.db.SaveChangesAsync();
                    MessageBox.Show("Поиск игры прекращен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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

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
using System.Windows.Threading;

namespace BarsGroup.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для PlayingFieldHumanWindow.xaml
    /// </summary>
    public partial class PlayingFieldHumanWindow : Window
    {
        public PlayingFieldHumanWindow(UsersInOnLineGame NewThisUser)
        {
            InitializeComponent();

            ThisUser = NewThisUser;
            Core.db = new MainEntities();

            List<UsersInOnLineGame> AllEnemy = Core.db.UsersInOnLineGame.Where(item => item.GameID == ThisUser.GameID).ToList();
            var Enemy = AllEnemy.Find(item => item.UserID != ThisUser.UserID);

            
            this.Title = "Игра против пользователя с ником " + Enemy.Users.Login;
            if (ThisUser.StatusID == 1)
            {
                this.Title += " (Ваш ход)";
                MessageBox.Show("Вы играете за черные шашки", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                this.Title += " (Ход противника)";
                MessageBox.Show("Вы играете за белые шашки", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            CurrentGame = ThisUser.OnLineGames;

            for (int i = 0; i <= 7; i++)
            {
                StackPanel newStackpanel = new StackPanel();
                newStackpanel.Orientation = Orientation.Horizontal;
                newStackpanel.Name = "N" + i;
                PlayingField.Children.Add(newStackpanel);
                for (int j = 0; j <= 7; j++)
                {
                    Button NewBtn = new Button();
                    NewBtn.Name = "N" + i + j;
                    NewBtn.Content = "" + m[j] + (i + 1);
                    NewBtn.Style = this.TryFindResource("Shashka") as Style;

                    if (NewBtn.Name.ToString().Substring(1) == "33" || NewBtn.Name.ToString().Substring(1) == "44")
                    {
                        NewBtn.Background = Brushes.White;
                        NewBtn.Foreground = Brushes.White;
                    }
                    if (NewBtn.Name.ToString().Substring(1) == "34" || NewBtn.Name.ToString().Substring(1) == "43")
                    {
                        NewBtn.Background = Brushes.Black;
                        NewBtn.Foreground = Brushes.Black;
                    }

                    NewBtn.Click += Shashka_Click;
                    NewBtn.MouseEnter += Shashka_MouseEnter;
                    NewBtn.MouseLeave += Shashka_MouseLeave;
                    newStackpanel.Children.Add(NewBtn);
                }
            }

            StartTimer();
        }

        #region Нужно
        char[] m = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };

        public UsersInOnLineGame ThisUser { get; set; }
        public OnLineGames CurrentGame { get; set; } = new OnLineGames();

        public string[,] CurrentGameArea { get; set; } = new string[8,8];

        public Button Shashka1 { get; set; } = new Button();
        public Button Shashka2 { get; set; } = new Button();
        public Button Shashka3 { get; set; } = new Button(); // Нужна для проверок

        public int User1Quantity { get; set; } = 2; // Количество черных клеток на поле
        public int User2Quantity { get; set; } = 2; // Количество белых клеток на поле

        public int WalksNumber { get; set; } = 1;
        public string WhoWalks { get; set; }

        public DispatcherTimer Timer = new DispatcherTimer();
        public int Seconds { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Hours { get; set; } = 0;

        public FlowDocument GameLogDocument { get; set; } = new FlowDocument();

        public UsersInOnLineGame CurrentUserInGame { get; set; } = new UsersInOnLineGame();

        #endregion

        private async void Shashka_Click(object sender, RoutedEventArgs e)
        {
            if (Shashka1.Content == null)
            {
                Shashka1 = (Button)sender;

                if (WalksNumber % 2 == 0 && ThisUser.StatusID == 2)
                {
                    if (Shashka1.Background != Brushes.White)
                    {
                        ChangeBtnNewBtn();
                        return;
                    }
                }
                else if (WalksNumber % 2 == 1 && ThisUser.StatusID == 1)
                {
                    if (Shashka1.Background != Brushes.Black)
                    {
                        ChangeBtnNewBtn();
                        return;
                    }
                }
                else
                {
                    ChangeBtnNewBtn();
                    return;
                }
                //Shashka1.Background = Brushes.RoyalBlue;
            }
            else
            {
                Shashka2 = (Button)sender;
                Shashka3 = Shashka2;

                if (Shashka2.Background == Brushes.Black || Shashka2.Background == Brushes.White)
                {
                    ChangeBtnNewBtn();
                    return;
                }

                var a = Shashka1.Name.ToString().Substring(1);
                var b = Shashka2.Name.ToString().Substring(1);
                var a1 = (a[0] - '0');
                var a2 = (a[1] - '0');
                var b1 = (b[0] - '0');
                var b2 = (b[1] - '0');
                var f = false;
                var dvijenie = 0; //    1 - сверху вниз;
                                  //    2 - снизу вверх;
                                  //    3 - слева направо;
                                  //    4 - справа на лево;
                                  //    5 - диагональ слева на направо (сверху вниз)
                                  //    6 - диагональ справа на лево (снизу вверх)
                                  //    7 - диагональ слева на направо (снизу вверх)
                                  //    8 - диагональ справа на лево (сверху вниз)

                #region Вычисление вида движения
                if (a1 < b1)
                    dvijenie = 1;
                if (a1 > b1)
                    dvijenie = 2;

                if (a2 < b2)
                    dvijenie = 3;
                if (a2 > b2)
                    dvijenie = 4;

                if (a1 < b1 && a2 < b2)
                    dvijenie = 5;
                if (a1 > b1 && a2 > b2)
                    dvijenie = 6;

                if (a1 > b1 && a2 < b2)
                    dvijenie = 7;
                if (a1 < b1 && a2 > b2)
                    dvijenie = 8;
                #endregion

                // Проверка на то, возможно ли сделать ход
                switch (dvijenie)
                {
                    case 1: //    1 - сверху вниз
                        for (int i = 0; i <= 7; i++)
                        {
                            if (f == true)
                            {
                                var ShashkaChanged = FindButton(PlayingField, "N" + i + a2);

                                if (!CheckShashkaChanged(ShashkaChanged))
                                {
                                    ChangeBtnNewBtn();
                                    return;
                                }

                                if (ShashkaChanged.Background != Brushes.White && ShashkaChanged.Background != Brushes.Black)
                                {
                                    if (Shashka1 == FindButton(PlayingField, "N" + (i - 1) + a2))
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }
                                    Shashka2 = ShashkaChanged;
                                    b = Shashka2.Name.ToString().Substring(1);
                                    f = false;
                                    break;
                                }
                            }
                            if (a1 == i)
                                f = true;
                        }
                        break;
                    case 2: //    2 - снизу вверх
                        for (int i = 7; i >= 0; i--)
                        {
                            if (f == true)
                            {
                                var ShashkaChanged = FindButton(PlayingField, "N" + i + a2);

                                if (!CheckShashkaChanged(ShashkaChanged))
                                {
                                    ChangeBtnNewBtn();
                                    return;
                                }

                                if (ShashkaChanged.Background != Brushes.White && ShashkaChanged.Background != Brushes.Black)
                                {
                                    if (Shashka1 == FindButton(PlayingField, "N" + (i + 1) + a2))
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }
                                    if (a2 + 1 == i)
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }
                                    Shashka2 = ShashkaChanged;
                                    b = Shashka2.Name.ToString().Substring(1);
                                    f = false;
                                    break;
                                }
                            }
                            if (a1 == i)
                                f = true;
                        }
                        break;
                    case 3: //    3 - слева направо
                        for (int i = 0; i <= 7; i++)
                        {
                            if (f == true)
                            {
                                var ShashkaChanged = FindButton(PlayingField, "N" + a1 + i);

                                if (!CheckShashkaChanged(ShashkaChanged))
                                {
                                    ChangeBtnNewBtn();
                                    return;
                                }

                                if (ShashkaChanged.Background != Brushes.White && ShashkaChanged.Background != Brushes.Black)
                                {
                                    if (Shashka1 == FindButton(PlayingField, "N" + a1 + (i - 1)))
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }
                                    Shashka2 = ShashkaChanged;
                                    b = Shashka2.Name.ToString().Substring(1);
                                    f = false;
                                    break;
                                }
                            }
                            if (a2 == i)
                                f = true;
                        }
                        break;
                    case 4: //    4 - справа на лево
                        for (int i = 7; i >= 0; i--)
                        {
                            if (f == true)
                            {
                                var ShashkaChanged = FindButton(PlayingField, "N" + a1 + i);

                                if (!CheckShashkaChanged(ShashkaChanged))
                                {
                                    ChangeBtnNewBtn();
                                    return;
                                }

                                if (ShashkaChanged.Background != Brushes.White && ShashkaChanged.Background != Brushes.Black)
                                {
                                    if (Shashka1 == FindButton(PlayingField, "N" + a1 + (i + 1)))
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }
                                    Shashka2 = ShashkaChanged;
                                    b = Shashka2.Name.ToString().Substring(1);
                                    f = false;
                                    break;
                                }
                            }
                            if (a2 == i)
                                f = true;
                        }
                        break;
                    case 5: //    5 - диагональ слева на направо (сверху вниз)
                        for (int i = 0; i <= 7; i++)
                        {
                            for (int j = 0; j <= 7; j++)
                            {
                                if (a1 == i && a2 == j)
                                {
                                    f = true;
                                    a1++;
                                    a2++;

                                    var ShashkaChanged = FindButton(PlayingField, "N" + a1 + a2);

                                    if (!CheckShashkaChanged(ShashkaChanged))
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }

                                    if (ShashkaChanged.Background != Brushes.White && ShashkaChanged.Background != Brushes.Black)
                                    {
                                        if (Shashka1 == FindButton(PlayingField, "N" + (a1 - 1) + (a2 - 1)))
                                        {
                                            ChangeBtnNewBtn();
                                            return;
                                        }
                                        Shashka2 = ShashkaChanged;
                                        b = Shashka2.Name.ToString().Substring(1);
                                        b1 = (b[0] - '0');
                                        b2 = (b[1] - '0');
                                    }
                                }
                                if (b1 == a1 && b2 == a2)
                                {
                                    f = false;
                                    a1 = -1;
                                    a2 = -1;
                                }
                            }
                        }
                        break;
                    case 6: //    6 - диагональ справа на лево (снизу вверх)
                        for (int i = 7; i >= 0; i--)
                        {
                            for (int j = 7; j >= 0; j--)
                            {
                                if (a1 == i && a2 == j)
                                {
                                    f = true;
                                    a1--;
                                    a2--;

                                    var ShashkaChanged = FindButton(PlayingField, "N" + a1 + a2);

                                    if (!CheckShashkaChanged(ShashkaChanged))
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }

                                    if (ShashkaChanged.Background != Brushes.White && ShashkaChanged.Background != Brushes.Black)
                                    {
                                        if (Shashka1 == FindButton(PlayingField, "N" + (a1 + 1) + (a2 + 1)))
                                        {
                                            ChangeBtnNewBtn();
                                            return;
                                        }
                                        Shashka2 = ShashkaChanged;
                                        b = Shashka2.Name.ToString().Substring(1);
                                        b1 = (b[0] - '0');
                                        b2 = (b[1] - '0');
                                    }
                                }
                                if (b1 == a1 && b2 == a2)
                                {
                                    f = false;
                                    a1 = -1;
                                    a2 = -1;
                                }
                            }
                        }
                        break;
                    case 7: //    7 - диагональ слева на направо (снизу вверх)
                        for (int i = 7; i >= 0; i--)
                        {
                            for (int j = 0; j <= 7; j++)
                            {
                                if (a1 == i && a2 == j)
                                {
                                    f = true;
                                    a1--;
                                    a2++;

                                    var ShashkaChanged = FindButton(PlayingField, "N" + a1 + a2);

                                    if (!CheckShashkaChanged(ShashkaChanged))
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }

                                    if (ShashkaChanged.Background != Brushes.White && ShashkaChanged.Background != Brushes.Black)
                                    {
                                        if (Shashka1 == FindButton(PlayingField, "N" + (a1 + 1) + (a2 - 1)))
                                        {
                                            ChangeBtnNewBtn();
                                            return;
                                        }
                                        Shashka2 = ShashkaChanged;
                                        b = Shashka2.Name.ToString().Substring(1);
                                        b1 = (b[0] - '0');
                                        b2 = (b[1] - '0');
                                    }
                                }
                                if (b1 == a1 && b2 == a2)
                                {
                                    f = false;
                                    a1 = -1;
                                    a2 = -1;
                                }
                            }
                        }
                        break;
                    case 8: //    8 - диагональ справа на лево (сверху вниз)
                        for (int i = 0; i <= 7; i++)
                        {
                            for (int j = 7; j >= 0; j--)
                            {
                                if (a1 == i && a2 == j)
                                {
                                    f = true;
                                    a1++;
                                    a2--;

                                    var ShashkaChanged = FindButton(PlayingField, "N" + a1 + a2);

                                    if (!CheckShashkaChanged(ShashkaChanged))
                                    {
                                        ChangeBtnNewBtn();
                                        return;
                                    }

                                    if (ShashkaChanged.Background != Brushes.White && ShashkaChanged.Background != Brushes.Black)
                                    {
                                        if (Shashka1 == FindButton(PlayingField, "N" + (a1 - 1) + (a2 + 1)))
                                        {
                                            ChangeBtnNewBtn();
                                            return;
                                        }
                                        Shashka2 = ShashkaChanged;
                                        b = Shashka2.Name.ToString().Substring(1);
                                        b1 = (b[0] - '0');
                                        b2 = (b[1] - '0');
                                    }
                                }
                                if (b1 == a1 && b2 == a2)
                                {
                                    f = false;
                                    a1 = -1;
                                    a2 = -1;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (Shashka1.Background == Shashka2.Background || Shashka3 != Shashka2 || f == true)
                {
                    ChangeBtnNewBtn();
                    return;
                }

                a1 = (a[0] - '0');
                a2 = (a[1] - '0');
                b1 = (b[0] - '0');
                b2 = (b[1] - '0');
                f = false;

                Paragraph newparagraph = new Paragraph();
                Run newRun = new Run();
                newRun.Text = "Ход " + WalksNumber + "\n";
                if (WalksNumber % 2 == 0)
                    newRun.Text += "Белые делают ход и захватывают поля: ";
                else
                    newRun.Text += "Черные делают ход и захватывают поля: ";

                //Зарисовка клеток
                switch (dvijenie)
                {
                    case 1:
                        for (int i = 0; i <= 7; i++)
                        {
                            if (a1 == i)
                                f = true;
                            if (f == true)
                            {
                                var ShashkaChanged = FindButton(PlayingField, "N" + i + a2);
                                ChangeColorShashka(ShashkaChanged);
                                newRun.Text += ShashkaChanged.Content.ToString() + " ";
                            }
                            if (b1 == i)
                                f = false;
                        }
                        break;
                    case 2:
                        for (int i = 7; i >= 0; i--)
                        {
                            if (a1 == i)
                                f = true;
                            if (f == true)
                            {
                                var ShashkaChanged = FindButton(PlayingField, "N" + i + a2);
                                ChangeColorShashka(ShashkaChanged);
                                newRun.Text += ShashkaChanged.Content.ToString() + " ";
                            }
                            if (b1 == i)
                                f = false;
                        }
                        break;
                    case 3:
                        for (int i = 0; i <= 7; i++)
                        {
                            if (a2 == i)
                                f = true;
                            if (f == true)
                            {
                                var ShashkaChanged = FindButton(PlayingField, "N" + a1 + i);
                                ChangeColorShashka(ShashkaChanged);
                                newRun.Text += ShashkaChanged.Content.ToString() + " ";
                            }
                            if (b2 == i)
                                f = false;
                        }
                        break;
                    case 4:
                        for (int i = 7; i >= 0; i--)
                        {
                            if (a2 == i)
                                f = true;
                            if (f == true)
                            {
                                var ShashkaChanged = FindButton(PlayingField, "N" + a1 + i);
                                ChangeColorShashka(ShashkaChanged);
                                newRun.Text += ShashkaChanged.Content.ToString() + " ";
                            }
                            if (b2 == i)
                                f = false;
                        }
                        break;
                    case 5:
                        for (int i = 0; i <= 7; i++)
                        {
                            for (int j = 0; j <= 7; j++)
                            {
                                if (a1 == i && a2 == j)
                                {
                                    var ShashkaChanged = FindButton(PlayingField, "N" + i + j);
                                    ChangeColorShashka(ShashkaChanged);
                                    newRun.Text += ShashkaChanged.Content.ToString() + " ";
                                    a1++;
                                    a2++;
                                }
                                if (b1 == i && b2 == j)
                                {
                                    a1 = -1;
                                    a2 = -1;
                                }
                            }
                        }
                        break;
                    case 6:
                        for (int i = 7; i >= 0; i--)
                        {
                            for (int j = 7; j >= 0; j--)
                            {
                                if (a1 == i && a2 == j)
                                {
                                    var ShashkaChanged = FindButton(PlayingField, "N" + i + j);
                                    ChangeColorShashka(ShashkaChanged);
                                    newRun.Text += ShashkaChanged.Content.ToString() + " ";
                                    a1--;
                                    a2--;
                                }
                                if (b1 == i && b2 == j)
                                {
                                    a1 = -1;
                                    a2 = -1;
                                }
                            }
                        }
                        break;
                    case 7:
                        for (int i = 7; i >= 0; i--)
                        {
                            for (int j = 0; j <= 7; j++)
                            {
                                if (a1 == i && a2 == j)
                                {
                                    var ShashkaChanged = FindButton(PlayingField, "N" + i + j);
                                    ChangeColorShashka(ShashkaChanged);
                                    newRun.Text += ShashkaChanged.Content.ToString() + " ";
                                    a1--;
                                    a2++;
                                }
                                if (b1 == i && b2 == j)
                                {
                                    a1 = -1;
                                    a2 = -1;
                                }
                            }
                        }
                        break;
                    case 8:
                        for (int i = 0; i <= 7; i++)
                        {
                            for (int j = 7; j >= 0; j--)
                            {
                                if (a1 == i && a2 == j)
                                {
                                    var ShashkaChanged = FindButton(PlayingField, "N" + i + j);
                                    ChangeColorShashka(ShashkaChanged);
                                    newRun.Text += ShashkaChanged.Content.ToString() + " ";
                                    a1++;
                                    a2--;
                                }
                                if (b1 == i && b2 == j)
                                {
                                    a1 = -1;
                                    a2 = -1;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                ChangeBtnNewBtn();
                WalksNumber++;

                if (WalksNumber % 2 == 0)
                    WhoWalks = "Белые";
                else
                    WhoWalks = "Черные";
                WhoWalksTxt.Text = WhoWalks;
                WalksNumerTxt.Text = WalksNumber.ToString();

                User1Quantity = 0;
                User2Quantity = 0;

                for (int i = 0; i <= 7; i++)
                {
                    for (int j = 0; j <= 7; j++)
                    {
                        var ShashkaFind = FindButton(PlayingField, "N" + i + j);
                        if (ShashkaFind.Background == Brushes.Black)
                        {
                            User1Quantity++;
                            CurrentGameArea[i, j] = "B" + i + j;
                        }
                        else if (ShashkaFind.Background == Brushes.White)
                        {
                            User2Quantity++;
                            CurrentGameArea[i, j] = "W" + i + j;
                        }
                        else
                        {
                            CurrentGameArea[i, j] = "N" + i + j;
                        }
                    }
                }
                ScoreBlack.Text = User1Quantity.ToString();
                ScoreWhite.Text = User2Quantity.ToString();

                newRun.Text += "\nБелых шашек на поле: " + User2Quantity;
                newRun.Text += "\nЧерных шашек на поле: " + User1Quantity;
                newRun.Text += "\nВремени с начала игры прошло: " + string.Format("{0:00}:{1:00}:{2:00}", Hours, Minutes, Seconds);
                newparagraph.Inlines.Add(newRun);
                GameLogDocument.Blocks.Add(newparagraph);

                foreach (var item in CurrentGameArea)
                {
                    CurrentGame.LocationOfCheckers += item + " ";
                }

                CurrentGame.GameLog = "";
                /*
                foreach (var item in GameLogDocument.Blocks)
                {
                    CurrentGame.GameLog += item.ToString();
                    CurrentGame.GameLog += "\n\n";
                }
                */

                try
                {
                    var EditGame = await Core.db.OnLineGames.FindAsync(CurrentGame.ID);
                    EditGame = CurrentGame;

                    await Core.db.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }

            }
        }

        #region Показать/Скрыть позицию шашки, на которую пользователь навелся курсором
        private void Shashka_MouseEnter(object sender, MouseEventArgs e)
        {
            var CurrentBtn = (Button)sender;
            CurrentBtn.Foreground = Brushes.Black;
        }

        private void Shashka_MouseLeave(object sender, MouseEventArgs e)
        {
            var CurrentBtn = (Button)sender;
            CurrentBtn.Foreground = CurrentBtn.Background;
        }
        #endregion

        //Сброс шашки ( если пользователь хочет походить другой шашкой )
        private void PlayingField_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeBtnNewBtn();
        }

        public static Button FindButton(Visual vis, string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(vis); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(vis, i);

                Button button = child as Button;
                if (button != null)
                {
                    if (name == (string)button.Name) return button;
                }

                Button res = FindButton(child, name);
                if (res != null) return res;
            }
            return null;
        }

        public void ChangeBtnNewBtn()
        {
            Shashka1 = new Button();
            Shashka2 = new Button();
            Shashka3 = new Button();
        }

        public void ChangeColorShashka(Button btn)
        {
            if (WalksNumber % 2 == 0)
            {
                btn.Background = Brushes.White;
                btn.Foreground = Brushes.White;
            }
            else
            {
                btn.Background = Brushes.Black;
                btn.Foreground = Brushes.Black;
            }
        }

        public bool CheckShashkaChanged(Button change)
        {
            if (change == null)
                return false;

            if (WalksNumber % 2 == 0)
            {
                if (change.Background == Brushes.White)
                {
                    ChangeBtnNewBtn();
                    return false;
                }
            }
            else
            {
                if (change.Background == Brushes.Black)
                {
                    ChangeBtnNewBtn();
                    return false;
                }
            }
            return true;
        }

        private void LoseBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult UserSelect = MessageBox.Show("Вы действительно хотите сдаться?\nЕсли вы нажмете \"Да\" то вам будет засчитано поражение", "Предупреждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

            if (UserSelect == MessageBoxResult.Yes)
            {
                var a = Seconds / 60;
                decimal total = (Minutes + (Hours * 60) + (Seconds / 60));

                Paragraph newparagraph = new Paragraph();
                Run newRun = new Run();
                newRun.Text = "Ход " + WalksNumber + "\n";


                if (WalksNumber % 2 == 0)
                {
                    MessageBox.Show("Ваш противник сдался\nВаши очки: " + User1Quantity * total, "Вы выиграли!", MessageBoxButton.OK, MessageBoxImage.Information);
                    newRun.Text += "Белые сдались";
                }
                else
                {
                    MessageBox.Show("Ваш противник сдался\nВаши очки: " + User2Quantity * total, "Вы выиграли!", MessageBoxButton.OK, MessageBoxImage.Information);
                    newRun.Text += "Черные сдались";
                }
                newparagraph.Inlines.Add(newRun);
                GameLogDocument.Blocks.Add(newparagraph);
                newparagraph.Inlines.Clear();
                newRun.Text = "Итоговый счет игры:\n"
                            + "Всего белых шашек на поле:" + User1Quantity + "; Очки белого:" + (User2Quantity * total)
                            + "\nВсего черных шашек на поле:" + User2Quantity + "; Очки черного:" + (User1Quantity * total);
                newparagraph.Inlines.Add(newRun);
                GameLogDocument.Blocks.Add(newparagraph);
                this.Close();
            }
        }

        private void SeeLogBtn_Click(object sender, RoutedEventArgs e)
        {

            foreach (Window item in Application.Current.Windows)
            {
                if (item.Name == "LogGameWindow")
                {
                    MessageBox.Show("Можно открыть только одно окно с логом игры", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            var newGameLogDocument = GameLogDocument;
            GameLogWindow window = new GameLogWindow(newGameLogDocument);
            window.Show();
        }

        private void NextStepBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeBtnNewBtn();

            Paragraph newparagraph = new Paragraph();
            Run newRun = new Run();
            newRun.Text = "Ход " + WalksNumber + "\n";
            if (WalksNumber % 2 == 0)
            {
                newRun.Text += "Белые пропускают ход";
            }
            else
            {
                newRun.Text += "Черные пропускают ход";
            }

            WalksNumber++;
            if (WalksNumber % 2 == 0)
            {
                WhoWalks = "Белые";
            }
            else
            {
                WhoWalks = "Черные";
            }
            WhoWalksTxt.Text = WhoWalks;
            WalksNumerTxt.Text = WalksNumber.ToString();
            newparagraph.Inlines.Add(newRun);
            GameLogDocument.Blocks.Add(newparagraph);
        }

        public void StartTimer()
        {
            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += new EventHandler(TickTimer);
            Timer.Start();
        }

        public void TickTimer(object sender, EventArgs e)
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

            /*
            Core.db = new MainEntities();

            OnLineGames Gamesdb = await Core.db.OnLineGames.FindAsync(CurrentGame.ID);

            if (CurrentGame != Gamesdb)
            {
                CurrentGame = Gamesdb;

                string[] words = CurrentGame.LocationOfCheckers.Split(new char[] { ' ' });

                var a = 0;

                for (int i = 0; i <= 7; i++)
                {
                    for (int j = 0; j <= 7; j++)
                    {
                        // Удалить все кнопки с поля и заново поставить
                        CurrentGameArea[i, j] = words[a];
                        a++;

                    }
                }
            }
            */


            if (WalksNumber % 2 == 0)
                WhoWalks = "Белые";
            else
                WhoWalks = "Черные";
            WhoWalksTxt.Text = WhoWalks;
            WalksNumerTxt.Text = WalksNumber.ToString();
            InfoTimer.Text = string.Format("{0:00}:{1:00}", Hours, Minutes);
            InfoTimer.ToolTip = string.Format("{0:00}:{1:00}:{2:00}", Hours, Minutes, Seconds);
            ScoreBlack.Text = User1Quantity.ToString();
            ScoreWhite.Text = User2Quantity.ToString();
        }
    }
}

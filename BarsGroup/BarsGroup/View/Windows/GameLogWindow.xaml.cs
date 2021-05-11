using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace BarsGroup.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для GameLogWindow.xaml
    /// </summary>
    public partial class GameLogWindow : Window
    {
        public GameLogWindow(FlowDocument GameLog)
        {
            InitializeComponent();
            MainFlowDocument.Document = GameLog;
        }

        private void SaveLogBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Select = new OpenFileDialog();
            Select.Title = "Выберите папку, в которую необходимо сохранить лог игры";
            Select.Filter = "xaml files (*.xaml)|*.xaml|rtf files (*.rtf)|*.rtf";

            if (Select.ShowDialog() == true)
            {
                using (FileStream fs = File.Open(Select.FileName, FileMode.Create))
                {
                    if (MainFlowDocument.Document != null)
                    {
                        XamlWriter.Save(MainFlowDocument.Document, fs);
                        MessageBox.Show("Файл сохранен");
                    }
                }
            }
        }

        private void LogGameWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainFlowDocument.Document = null;
        }

        private void SelectFontSizeRadioBtn(object sender, RoutedEventArgs e)
        {
            RadioButton SelectFontSize = (RadioButton)sender;
            switch (Convert.ToInt32(SelectFontSize.Content))
            {
                case 16:
                    MainFlowDocument.Document.FontSize = 16;
                    break;
                case 18:
                    MainFlowDocument.Document.FontSize = 18;
                    break;
                case 20:
                    MainFlowDocument.Document.FontSize = 20;
                    break;
                case 24:
                    MainFlowDocument.Document.FontSize = 24;
                    break;
                case 30:
                    MainFlowDocument.Document.FontSize = 30;
                    break;
                case 32:
                    MainFlowDocument.Document.FontSize = 32;
                    break;
                case 34:
                    MainFlowDocument.Document.FontSize = 34;
                    break;
                case 38:
                    MainFlowDocument.Document.FontSize = 38;
                    break;
                case 50:
                    MainFlowDocument.Document.FontSize = 50;
                    break;
                default:
                    MainFlowDocument.Document.FontSize = 16;
                    break;
            }
        }
    }
}

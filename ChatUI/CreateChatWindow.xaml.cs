using ChatUI.DataBase;
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

namespace NetworkProgramming_ExamNew
{
    /// <summary>
    /// Interaction logic for CreateChatWindow.xaml
    /// </summary>
    public partial class CreateChatWindow : Window
    {
        private DataBase dataBase { get; set; } 

        public CreateChatWindow()
        {
            InitializeComponent();

            dataBase = new DataBase();
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!dataBase.IsChatExists(ChatNameTextBox.Text))
            {
                dataBase.InsertChat(ChatNameTextBox.Text);
                this.Close();
            }
            else
            {
                MessageBox.Show("Chat with the same name is already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

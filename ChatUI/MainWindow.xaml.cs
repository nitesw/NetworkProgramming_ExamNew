using ChatUI.DataBase;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetworkProgramming_ExamNew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataBase dataBase { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            dataBase = new DataBase();

            UsernameTextBox.MaxLength = 128;
            PassPasswordBox.MaxLength = 64;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataBase.IsUsernameExists(UsernameTextBox.Text))
            {
                MessageBox.Show("User with the same username is already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if(UsernameTextBox.Text.Trim().ToLower().Contains("admin"))
                {
                    if(dataBase.InsertUser(UsernameTextBox.Text, PassPasswordBox.Password.ToString(), 1))
                    {
                        MessageBox.Show("Successfully registered!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ChatWindow chatWindow = new ChatWindow();
                        this.Hide();
                        chatWindow.ShowDialog();
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    if (dataBase.InsertUser(UsernameTextBox.Text, PassPasswordBox.Password.ToString(), 0))
                    {
                        MessageBox.Show("Successfully registered!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ChatWindow chatWindow = new ChatWindow();
                        this.Hide();
                        chatWindow.ShowDialog();
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!dataBase.IsUserExists(UsernameTextBox.Text, PassPasswordBox.Password.ToString()))
            {
                MessageBox.Show("The login or password is incorrect!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Successfully logged in!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ChatWindow chatWindow = new ChatWindow();
                this.Hide();
                chatWindow.ShowDialog();
                this.Show();
            }
        }
    }
}
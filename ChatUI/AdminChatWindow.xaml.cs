using ChatUI.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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
    /// Interaction logic for AdminChatWindow.xaml
    /// </summary>
    public partial class AdminChatWindow : Window
    {
        private int currentUserId;
        private string remoteIP = "192.168.1.104";
        private short remotePort = 8080;

        IPEndPoint remoteEndPoint;
        UdpClient client = new UdpClient();

        private bool isListening = false;

        private DataBase dataBase { get; set; }

        public AdminChatWindow(int userId)
        {
            InitializeComponent();

            dataBase = new DataBase();
            currentUserId = userId;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            UsernameLabel.Content = dataBase.GetUsernameByUserId(userId);

            foreach (var item in dataBase.GetAllChats())
            {
                ListBoxItem boxItem = new ListBoxItem();
                boxItem.Tag = item.Key;
                boxItem.Content = item.Value;

                ChatsListBox.Items.Add(boxItem);
            }

            ChatsListBox.SelectedIndex = 0;
        }

        private async void Listen()
        {
            while (isListening)
            {
                var result = await client.ReceiveAsync();
                string message = Encoding.Unicode.GetString(result.Buffer);

                if (message.StartsWith("<NEW_CHAT>") || message.StartsWith("<DELETE_CHAT>"))
                {
                    RefreshChatList();
                }
                else if (message.Contains(":"))
                {
                    string[] parts = message.Split(':');
                    string username = parts[0].Trim();
                    int userId = dataBase.GetUserIdByUsername(username);
                    int chatId = dataBase.GetIdOfTheLastMessageFromUser(userId);

                    ListBoxItem selectedItem = (ListBoxItem)ChatsListBox.SelectedItem;
                    if ((int)selectedItem.Tag == chatId)
                    {
                        MessagesListBox.Items.Add(message);
                    }

                }
            }
        }
        private void JoinDisconnectBtnTB_Click(object sender, RoutedEventArgs e)
        {
            if (JoinDisconnectBtnTB.Content.ToString() == "Connect")
            {
                try
                {
                    SendMessage("<JOIN>");
                    isListening = true;
                    Listen();

                    dataBase.InsertLogs(currentUserId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    JoinDisconnectBtnTB.Content = "Disconnect";
                    JoinDisconnectBtnTB.Background = Brushes.OrangeRed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (JoinDisconnectBtnTB.Content.ToString() == "Disconnect")
            {
                SendMessage("<LEAVE>");
                isListening = false;

                MessagesListBox.Items.Clear();
                dataBase.UpdateLeaveLogs(currentUserId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                JoinDisconnectBtnTB.Content = "Connect";
                JoinDisconnectBtnTB.Background = Brushes.LightGreen;
            }
        }

        private void SendMsgButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isListening)
            {
                MessageBox.Show("Connect first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (string.IsNullOrWhiteSpace(MessageTextBox.Text) || MessageTextBox.Text == "Enter your text here...")
            {
                MessageBox.Show("Enter a message first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SendMessage(MessageTextBox.Text);
            MessageTextBox.Text = "";
        }
        private void SendMessage(string message)
        {
            if (message == "<JOIN>" || message == "<LEAVE>" || message == "<NEW_CHAT>" || message == "<DELETE_CHAT>")
            {
                byte[] bytes = Encoding.Unicode.GetBytes(message);
                client.Send(bytes, bytes.Length, remoteEndPoint);
            }
            else
            {
                if (ChatsListBox.SelectedItem != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(dataBase.GetUsernameByUserId(currentUserId));
                    stringBuilder.Append(": ");
                    stringBuilder.Append(message);

                    message = stringBuilder.ToString();

                    byte[] bytes = Encoding.Unicode.GetBytes(message);
                    client.Send(bytes, bytes.Length, remoteEndPoint);

                    ListBoxItem selectedItem = (ListBoxItem)ChatsListBox.SelectedItem;

                    int tag = (int)selectedItem.Tag;
                    dataBase.InsertMessage(currentUserId, tag, message);
                }
                else
                {
                    MessageBox.Show("Select chat first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExitBtnTB_Click(object sender, RoutedEventArgs e)
        {
            if (JoinDisconnectBtnTB.Content.ToString() == "Disconnect")
            {
                SendMessage("<LEAVE>");
                isListening = false;

                dataBase.UpdateLeaveLogs(currentUserId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                JoinDisconnectBtnTB.Content = "Join chat";
                JoinDisconnectBtnTB.Background = Brushes.LightGreen;

                this.Close();
            }
            else
            {
                this.Close();
            }
        }

        private void MessageTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Enter your text here...";
                textBox.Foreground = Brushes.DimGray;
            }
        }

        private void MessageTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Foreground == Brushes.DimGray)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBar_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Foreground == Brushes.DimGray)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBar_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Enter username here...";
                textBox.Foreground = Brushes.DimGray;
            }
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                SendMsgButton_Click(sender, e);
            }
        }

        private void ChatsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatsListBox.SelectedItem != null)
            {
                MessagesListBox.Items.Clear();
                ListBoxItem selectedListBoxItem = (ListBoxItem)ChatsListBox.SelectedItem;
                int chatId = (int)selectedListBoxItem.Tag;

                foreach (var message in dataBase.GetAllMessagesByChatId(chatId))
                {
                    MessagesListBox.Items.Add(message);
                }
            }
        }

        public void RefreshChatList()
        {
            ChatsListBox.Items.Clear();
            foreach (var item in dataBase.GetAllChats())
            {
                ListBoxItem boxItem = new ListBoxItem();
                boxItem.Tag = item.Key;
                boxItem.Content = item.Value;

                ChatsListBox.Items.Add(boxItem);
            }
            ChatsListBox.SelectedIndex = 0;
        }
        private void CreateChatBtnTB_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                CreateChatWindow createChatWindow = new CreateChatWindow();
                createChatWindow.ShowDialog();

                SendMessage("<NEW_CHAT>");
            }
            else
            {
                MessageBox.Show("Connect first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void GetAllMsgsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                AdminShowListBox.Items.Clear();
                foreach (var message in dataBase.GetAllMessages())
                {
                    AdminShowListBox.Items.Add(message);
                }
            }
            else
            {
                MessageBox.Show("Connect first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void GetAllUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                AdminShowListBox.Items.Clear();
                foreach (var user in dataBase.GetAllUsers())
                {
                    AdminShowListBox.Items.Add(user);
                }
            }
            else
            {
                MessageBox.Show("Connect first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }      
        }

        private void GetAllLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                AdminShowListBox.Items.Clear();
                foreach (var log in dataBase.GetAllLogs())
                {
                    AdminShowListBox.Items.Add(log);
                }
            }
            else
            {
                MessageBox.Show("Connect first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void GetUserMsgBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                if (!string.IsNullOrWhiteSpace(SearchTextBar.Text) || SearchTextBar.Text != "Enter username here...")
                {
                    if (dataBase.IsUsernameExists(SearchTextBar.Text))
                    {
                        AdminShowListBox.Items.Clear();
                        foreach (var message in dataBase.GetAllMessagesByUserId(dataBase.GetUserIdByUsername(SearchTextBar.Text)))
                        {
                            AdminShowListBox.Items.Add(message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("User with this username does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Enter username first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Connect first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void MakeUserAdminBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                if (!string.IsNullOrWhiteSpace(SearchTextBar.Text) || SearchTextBar.Text != "Enter username here...")
                {
                    if (dataBase.IsUsernameExists(SearchTextBar.Text))
                    {
                        if(dataBase.UpdateUserAdminRightsByUserId(dataBase.GetUserIdByUsername(SearchTextBar.Text)))
                        {
                            MessageBox.Show($"Success! User {SearchTextBar.Text} is now admin!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            SearchTextBar.Text = "";
                        }
                        else
                        {
                            MessageBox.Show($"Success! User {SearchTextBar.Text} is now default user!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            SearchTextBar.Text = "";
                        }
                    }
                    else
                    {
                        SearchTextBar.Text = "";
                        MessageBox.Show("User with this username does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Enter username first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Connect first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void DeleteChatBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                if (ChatsListBox.SelectedIndex != null)
                {
                    ListBoxItem selectedListBoxItem = (ListBoxItem)ChatsListBox.SelectedItem;
                    int chatId = (int)selectedListBoxItem.Tag;

                    dataBase.DeleteChat(chatId);
                    SendMessage("<DELETE_CHAT>");
                }
                else
                {
                    MessageBox.Show("Select chat first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            else
            {
                MessageBox.Show("Connect first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
    }
}

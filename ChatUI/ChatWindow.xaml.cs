﻿using System;
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
using ChatUI.DataBase;
using System.Globalization;
using ChatServer;

namespace NetworkProgramming_ExamNew
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private int currentUserId;
        private string remoteIP = "192.168.1.104";
        private short remotePort = 8080;

        IPEndPoint remoteEndPoint;
        UdpClient client = new UdpClient();

        private bool isListening = false;

        private DataBase dataBase { get; set; }

        public ChatWindow(int userId)
        {
            InitializeComponent();

            dataBase = new DataBase();
            currentUserId = userId;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            UsernameLabel.Content = dataBase.GetUsernameByUserId(userId);
        }

        private async void Listen()
        {
            while (isListening)
            {
                var result = await client.ReceiveAsync();
                string message = Encoding.Unicode.GetString(result.Buffer);

                if (message.Contains(":"))
                {
                    MessagesListBox.Items.Add(message);
                }
            }
        }
        private void JoinDisconnectBtnTB_Click(object sender, RoutedEventArgs e)
        {
            if (JoinDisconnectBtnTB.Content.ToString() == "Join chat")
            {
                try
                {
                    SendMessage("<JOIN>");
                    isListening = true;
                    Listen();

                    dataBase.InsertLogs(currentUserId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    JoinDisconnectBtnTB.Content = "Leave chat";
                    JoinDisconnectBtnTB.Background = Brushes.OrangeRed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (JoinDisconnectBtnTB.Content.ToString() == "Leave chat")
            {
                SendMessage("<LEAVE>");
                isListening = false;

                MessagesListBox.Items.Clear();
                dataBase.UpdateLeaveLogs(currentUserId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                JoinDisconnectBtnTB.Content = "Join chat";
                JoinDisconnectBtnTB.Background = Brushes.LightGreen;
            }
        }

        private void SendMsgButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isListening)
            {
                MessageBox.Show("Join a chat first!", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (message == "<JOIN>" || message == "<LEAVE>")
            {
                byte[] bytes = Encoding.Unicode.GetBytes(message);
                client.Send(bytes, bytes.Length, remoteEndPoint);
            }
            else
            {
                byte[] bytes = Encoding.Unicode.GetBytes(dataBase.GetUsernameByUserId(currentUserId) + ": " + message);
                client.Send(bytes, bytes.Length, remoteEndPoint);
                dataBase.InsertMessage(currentUserId, message);
            }
        }

        private void ExitBtnTB_Click(object sender, RoutedEventArgs e)
        {
            if (JoinDisconnectBtnTB.Content.ToString() == "Leave chat")
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

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                SendMsgButton_Click(sender, e);
            }
        }
    }
}

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
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
        }

        private Socket client = null;
        private void JoinDisconnectBtnTB_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string buttonContent = button.Content.ToString().Trim();

            if (buttonContent == "Join chat")
            {
                try
                {
                    IPAddress connectionIp = IPAddress.Parse("192.168.1.104");
                    int connectionPort = 8080;

                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client.Connect(connectionIp, connectionPort);

                    button.Content = "Leave chat";
                    button.Background = Brushes.OrangeRed;


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (buttonContent == "Leave chat")
            {
                client?.Close();
                client = null;

                button.Content = "Join chat";
                button.Background = Brushes.LightGreen;
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
    }
}

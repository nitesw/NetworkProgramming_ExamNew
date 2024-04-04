using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static System.Net.Mime.MediaTypeNames;

namespace ChatUI.DataBase
{
    public class DataBase
    {
        private string connectionString = "Data Source=DESKTOP-ROR88SC;Initial Catalog=ChatDB;Integrated Security=True;";
        private SqlConnection connection;

        public DataBase()
        {
            connection = new SqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public int GetUserIdByUsername(string username)
        {
            string queryString = $"SELECT Id, Username FROM Users WHERE Username = @Username";
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@Username", username);

                adapter.SelectCommand = command;
                adapter.Fill(table);
            }

            if (table.Rows.Count == 1)
            {
                return (int)table.Rows[0]["Id"];
            }
            else
            {
                return -1;
            }
        }
        public string GetUsernameByUserId(int userId)
        {
            string queryString = $"SELECT Id, Username FROM Users WHERE Id = '{userId}'";
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                adapter.SelectCommand = command;
                adapter.Fill(table);
            }

            if (table.Rows.Count == 1)
            {
                return (string)table.Rows[0]["Username"];
            }
            else
            {
                return "Error";
            }
        }
        public bool IsUserExists(string username, string password)
        {
            string queryString = $"SELECT Id, Username, Password FROM Users WHERE Username = @Username AND Password = @Password";

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                adapter.SelectCommand = command;
                adapter.Fill(table);
            }

            return table.Rows.Count == 1;
        }

        public bool InsertUser(string username, string password, byte isAdmin)
        {
            string queryString = $"INSERT INTO Users (Username, Password, IsAdmin) VALUES (@Username, @Password, {isAdmin})";
            
            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                try
                {
                    OpenConnection();
                    if (command.ExecuteNonQuery() == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    return false;
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        private bool LogExists(int userId)
        {
            string queryString = $"SELECT COUNT(*) FROM Logs WHERE UserId = '{userId}'";
            int count = 0;

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                try
                {
                    OpenConnection();
                    count = (int)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    CloseConnection();
                }
            }

            return count > 0;
        }
        public void InsertLogs(int userId, string connectionTime)
        {
            if (!LogExists(userId))
            {
                string queryString = $"INSERT INTO Logs (UserId, ConnectionTime) VALUES ('{userId}', '{connectionTime}')";

                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    try
                    {
                        OpenConnection();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                    finally
                    {
                        CloseConnection();
                    }
                }
            }
            else
            {
                string queryString = $"UPDATE Logs SET ConnectionTime = '{connectionTime}', DisconnectionTime = NULL" +
                    $" WHERE UserId = '{userId}'";

                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    try
                    {
                        OpenConnection();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                    finally
                    {
                        CloseConnection();
                    }
                }
            }
        }
        public void UpdateLeaveLogs(int userId, string disconnectionTime)
        {
            string queryString = $"UPDATE Logs SET DisconnectionTime = '{disconnectionTime}' WHERE UserId = '{userId}'";

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                try
                {
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public void InsertMessage(int userId, int chatId, string text)
        {
            string queryString = "INSERT INTO Messages (UserId, ChatId, Text) VALUES (@UserId, @ChatId, @Text)";

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@ChatId", chatId);
                command.Parameters.AddWithValue("@Text", text);

                try
                {
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public Dictionary<int, string> GetAllChats()
        {
            Dictionary<int, string> items = new Dictionary<int, string>();
            string queryString = "SELECT * FROM Chats";
            SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
            DataTable table = new DataTable();

            adapter.Fill(table);

            foreach (DataRow row in table.Rows)
            {
                int id = Convert.ToInt32(row["Id"]);
                string name = row["Name"].ToString();
                items.Add(id, name);
            }

            if (items.Count > 0)
            {
                return items;
            }
            else
            {
                return null;
            }
        }
        public List<string> GetAllMessages(int chatId)
        {
            List<string> messages = new List<string>();
            string queryString = $"SELECT * FROM Messages WHERE ChatId = '{chatId}'";
            SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
            DataTable table = new DataTable();

            adapter.Fill(table);

            foreach (DataRow row in table.Rows)
            {
                string message = row["Text"].ToString();
                messages.Add(message);
            }

            return messages;
        }
        public int GetIdOfTheLastMessageFromUser(int userId)
        {
            string queryString = $"SELECT * FROM Messages WHERE UserId = '{userId}' ORDER BY SendTime DESC";
            SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
            DataTable table = new DataTable();

            adapter.Fill(table);

            int chatId = (int)table.Rows[0]["ChatId"];

            return chatId;
        }

        public void InsertGroup(string name)
        {
            string queryString = $"INSERT INTO Chats ([Name]) VALUES (@Name)";

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@Name", name);

                try
                {
                    OpenConnection();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }
    }
}

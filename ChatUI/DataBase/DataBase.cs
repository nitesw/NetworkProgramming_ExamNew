using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

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
            string queryString = $"SELECT Id, Username FROM Users WHERE Username = '{username}'";
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
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
        public bool IsUserExists(string username, string password)
        {
            string queryString = $"SELECT Id, Username, Password FROM Users WHERE Username = '{username}' AND Password = '{password}'";

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                adapter.SelectCommand = command;
                adapter.Fill(table);
            }

            return table.Rows.Count == 1;
        }

        public bool InsertUser(string username, string password, byte isAdmin)
        {
            string queryString = $"INSERT INTO Users (Username, Password, IsAdmin) VALUES ('{username}', '{password}', {isAdmin})";
            
            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
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
        public void InsertLogs(int userId)
        {
            string queryString = $"INSERT INTO Logs (UserId) VALUES ('{userId}')";

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
    }
}

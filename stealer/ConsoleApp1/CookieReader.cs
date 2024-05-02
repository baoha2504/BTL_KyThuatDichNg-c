using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace ConsoleApp1
{
    internal class CookieReader
    {
        private string ChromeCookiesPath;
        public CookieReader(string chromeCookiesPath)
        {
            ChromeCookiesPath = chromeCookiesPath;
        }
        public void ReadCookies()
        {
            string connectionString = $"Data Source={ChromeCookiesPath};";
            string query = "SELECT name, value FROM cookies";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string cookieName = reader["name"].ToString();
                        string cookieValue = reader["value"].ToString();

                        Console.WriteLine($"Cookie: {cookieName}={cookieValue}");
                    }
                }
            }
        }
    }
}

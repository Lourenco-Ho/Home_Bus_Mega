using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Home_Bus_Mega
{
    static class MTR_database
    {
        const string DB_PATH = "MTR_data.db",
            TABLE_LRT_LINES_AND_STATIONS = "light_rail_routes_and_stops";

        public static string[] Get_Station_Name_By_Station_Code(string station_code)
        {
            using (var connection = Get_Connection())
            {
                connection.Open();
                var command = new SQLiteCommand($"SELECT DISTINCT Chinese_Name, English_Name FROM {TABLE_LRT_LINES_AND_STATIONS} WHERE Stop_Code = '{station_code}'", connection);
                using (var reader = command.ExecuteReader())
                {
                    System.Diagnostics.Debug.WriteLine(reader);
                    while (reader.Read())
                    {
                        return new string[] { reader["Chinese_Name"].ToString(), reader["English_Name"].ToString() };
                    }
                }
                connection.Close();
            }
            return new string[] { };
        }

        public static string[] Get_Station_Name_By_Station_ID(string station_id)
        {
            using (var connection = Get_Connection())
            {
                connection.Open();
                string command_string = $"SELECT * FROM {TABLE_LRT_LINES_AND_STATIONS} WHERE Stop_ID = '{station_id}'";
                var command = new SQLiteCommand(command_string, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return new string[] { reader["Chinese_Name"].ToString(), reader["English_Name"].ToString() };
                    }
                }
                connection.Close();
            }
            return new string[]{ };
        }

        private static SQLiteConnection Get_Connection()
        {
            return new SQLiteConnection($"Data Source={DB_PATH};Version=3");
        }
    }
}

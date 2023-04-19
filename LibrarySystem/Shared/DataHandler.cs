using System.Data;
using System.Data.SqlClient;

namespace LibrarySystem.Shared
{
    public class DataHandler
    {
        private readonly string _connectionString;
        public class DataItem
        {
            public string FileName { get; set; } = "";
            public string ToWho { get; set; } = "";
            public string Link { get; set; } = "";
        }

        public DataHandler(string connectionString)
        {
            _connectionString = connectionString;
            CheckDatabase();
        }

        public void AddData(string title, string towho, string linktofile)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();

            command.CommandText = "INSERT INTO PrintTB (FileName, ToWho, Link) VALUES (@Title, @ToWho, @Link)";
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@ToWho", towho);
            command.Parameters.AddWithValue("@Link", linktofile);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 8152) // String or binary data would be truncated
                {
                    // Truncate the Link parameter value to fit within the column size limit
                    int linkMaxLength = 200; // or whatever maximum length you have defined for the Link column
                    string truncatedLink = linktofile.Substring(0, Math.Min(linktofile.Length, linkMaxLength));
                    command.Parameters["@Link"].Value = truncatedLink;

                    // Retry the SQL command with the truncated link value
                    command.ExecuteNonQuery();
                }
                else
                {
                    // Handle other types of SQL exceptions
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Handle other types of exceptions
                throw;
            }
        }




        public void DeleteData(string title, string towho, string linktofile)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM PrintTB WHERE FileName = @title AND ToWho = @towho AND Link = @linktofile", connection))
            {
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@towho", towho);
                command.Parameters.AddWithValue("@linktofile", linktofile);
                command.CommandType = CommandType.Text;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<DataItem> GetData()
        {
            var data = new List<DataItem>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT * FROM PrintTB", connection))
            {
                command.CommandType = CommandType.Text;
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new DataItem
                        {
                            FileName = reader.GetString(reader.GetOrdinal("FileName")),
                            ToWho = reader.GetString(reader.GetOrdinal("ToWho")),
                            Link = reader.GetString(reader.GetOrdinal("Link"))
                        };

                        data.Add(item);
                    }
                }
            }

            return data;
        }
        private void CheckDatabase()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check if database exists
                using (var command = new SqlCommand($"SELECT COUNT(*) FROM [master].[sys].[databases] WHERE name = '3DDB'", connection))
                {
                    var count = (int)command.ExecuteScalar();
                    if (count == 0)
                    {
                        // Create database
                        using (var command2 = new SqlCommand("CREATE DATABASE 3DDB", connection))
                        {
                            command2.ExecuteNonQuery();
                        }
                    }
                }

                // Check if PrintTB table exists, create it if not
                using (var command3 = new SqlCommand($"IF OBJECT_ID('PrintTB', 'U') IS NULL BEGIN CREATE TABLE PrintTB (FileName NVARCHAR(50), ToWho NVARCHAR(50), Link NVARCHAR(200)) END", connection))
                {
                    command3.ExecuteNonQuery();
                }
            }
        }

    }

}


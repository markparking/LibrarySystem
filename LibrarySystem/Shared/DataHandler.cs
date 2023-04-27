using LibrarySystem.Pages;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace LibrarySystem.Shared
{
    public class DataHandler
    {
        private readonly string _connectionString;
        public class DataItem
        {
            public string BookTitle { get; set; } = "";
            public int Author_ID { get; set; }

            public int PublishYear { get; set; }
            public string Genre { get; set; }

            public bool Borrowed { get; set; } = false;

            public string AuthorName { get; set; } = "";
        }

        public DataHandler(string connectionString)
        {
            _connectionString = connectionString;
            CheckDatabase();
        }

        public void AddData(string booktitle, int authorid, int publishyear, string genre, bool borrowed, string authorname)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();

            // Check if the author already exists in the Authors table
            command.CommandText = "SELECT AuthorID FROM Authors WHERE AuthorName = @AuthorName;";
            command.Parameters.AddWithValue("@AuthorName", authorname);
            connection.Open();
            int? existingAuthorId = command.ExecuteScalar() as int?;
            connection.Close();

            // If the author doesn't exist, insert a new row into the Authors table
            if (!existingAuthorId.HasValue)
            {
                command.CommandText = "INSERT INTO Authors (AuthorName) VALUES (@AuthorName); SELECT SCOPE_IDENTITY();";
                connection.Open();
                existingAuthorId = Convert.ToInt32(command.ExecuteScalar());
                connection.Close();
            }

            // Insert a new row into the Books table
            command.CommandText = "INSERT INTO Books (Title, Author_ID, PublishYear, Genre, Borrowed) VALUES (@BookTitle, @Author_ID, @PublishYear, @Genre, @Borrowed)";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@BookTitle", booktitle);
            command.Parameters.AddWithValue("@Author_ID", existingAuthorId.Value);
            command.Parameters.AddWithValue("@PublishYear", publishyear);
            command.Parameters.AddWithValue("@Genre", genre);
            command.Parameters.AddWithValue("@Borrowed", borrowed);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void DeleteData(string booktitle, int authorid, int publishyear, string genre, bool borrowed)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM Books WHERE Title = @booktitle", connection))
            {
                command.Parameters.AddWithValue("@BookTitle", booktitle);
                command.CommandType = CommandType.Text;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void ReserveData(string booktitle, bool borrowed)
        {
            borrowed = true;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("UPDATE Books SET Borrowed = @Borrowed WHERE Title = @BookTitle", connection)) //Virker ikke. Fix
            {
                command.Parameters.AddWithValue("@BookTitle", booktitle);
                command.Parameters.AddWithValue("@Borrowed", borrowed);
                command.CommandType = CommandType.Text;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }



        public List<DataItem> GetData()
        {
            var data = new List<DataItem>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT Title AS \"Book Title\", AuthorName AS \"Author\", PublishYear AS \"Published\", Genre, Borrowed FROM Books B INNER JOIN Authors A ON B.Author_ID = A.AuthorID GROUP BY A.AuthorID, Title, AuthorName, PublishYear, Genre, Borrowed;", connection))
            {
                command.CommandType = CommandType.Text;
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new DataItem
                        {
                            BookTitle = reader.GetString(reader.GetOrdinal("Book Title")),
                            AuthorName = reader.GetString(reader.GetOrdinal("Author")),
                            PublishYear = reader.GetInt32(reader.GetOrdinal("Published")),
                            Genre = reader.GetString(reader.GetOrdinal("Genre"))
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

            }

        }

    }
}


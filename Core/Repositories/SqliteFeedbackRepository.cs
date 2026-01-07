using Core.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    internal class SqliteFeedbackRepository : FeedbackRepository
    {
        private readonly string _connectionString;

        public SqliteFeedbackRepository()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var databaseDirectory = Path.Combine(documentsPath, "AdhdFeedback");
            var databasePath = Path.Combine(databaseDirectory, "database.sqlite3");

            Directory.CreateDirectory(databaseDirectory);

            _connectionString = $"Data Source={databasePath}";

            EnsureTableExistsAsync().GetAwaiter().GetResult();
        }

        private async Task EnsureTableExistsAsync()
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
            CREATE TABLE IF NOT EXISTS feedbacks (
                id INTEGER PRIMARY KEY,
                number INTEGER NOT NULL,
                text TEXT NOT NULL
            );
            """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertNew(Feedback feedback) { return 0; }

        //public async Task<int> InsertNew(Feedback feedback)
        //{
        //    await using var connection = new SqliteConnection(_connectionString);
        //    await connection.OpenAsync();

        //    const string sql = """
        //    INSERT INTO template (id, number, text)
        //    VALUES (@id, @number, @text);
        //    """;

        //    await using var command = connection.CreateCommand();
        //    command.CommandText = sql;

        //    command.Parameters.AddWithValue("@id", feedback.personal_analytics_data..Id);
        //    command.Parameters.AddWithValue("@number", record.Number);
        //    command.Parameters.AddWithValue("@text", record.Text);

        //    await command.ExecuteNonQueryAsync();

        //}
    }
}

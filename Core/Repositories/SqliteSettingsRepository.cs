using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public class SqliteSettingsRepository
    {
        private readonly string _connectionString;
        public SqliteSettingsRepository()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var databaseDirectory = Path.Combine(documentsPath, "AdhdFeedback");
            var databasePath = Path.Combine(databaseDirectory, "database.sqlite3");

            Directory.CreateDirectory(databaseDirectory);

            _connectionString = $"Data Source={databasePath};Pooling=False;";

            //EnsureTableExistsAsync().GetAwaiter().GetResult();
            EnsureTableExistsSync();
            Trace.WriteLine("[ SqliteSettingsRepository ] Initialized SqliteSettingsRepository with database at: " + databasePath);
        }

        private void EnsureTableExistsSync()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
                CREATE TABLE IF NOT EXISTS settings (
                    setting_name TEXT PRIMARY KEY,
                    setting_value TEXT NOT NULL
                );
                ";

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        private async Task EnsureTableExistsAsync()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string sql = @"
                CREATE TABLE IF NOT EXISTS settings (
                    setting_name TEXT PRIMARY KEY,
                    setting_value TEXT NOT NULL
                );
                ";

                await using var command = connection.CreateCommand();
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task SaveSettingAsync(string settingKey, string settingValue)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string sql = @"
                    INSERT OR REPLACE INTO settings (setting_name, setting_value)
                    VALUES ($settingKey, $settingValue)
                ";
                await using var command = connection.CreateCommand();
                command.Parameters.AddWithValue("$settingKey", settingKey);
                command.Parameters.AddWithValue("$settingValue", settingValue);
                command.CommandText = sql;
                try
                {
                    await command.ExecuteNonQueryAsync();
                }
                catch (DbException ex)
                {
                    Trace.WriteLine("[ SqliteSettingsRepository ] Error saving setting: " + ex.Message);
                    throw;
                }
            }
        }

        public void SaveSettingSync(string settingKey, string settingValue)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
                    INSERT OR REPLACE INTO settings (setting_name, setting_value)
                    VALUES ($settingKey, $settingValue)
                ";
                using var command = connection.CreateCommand();
                command.Parameters.AddWithValue("$settingKey", settingKey);
                command.Parameters.AddWithValue("$settingValue", settingValue);
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        public async Task<string?> LoadSettingAsync(string settingKey)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string sql = @"
                    SELECT setting_value
                    FROM settings
                    WHERE setting_name = $settingKey;
                ";

                await using var command = connection.CreateCommand();
                command.Parameters.AddWithValue("$settingKey", settingKey);
                command.CommandText = sql;
                try
                {
                    var result = await command.ExecuteScalarAsync();
                    return result?.ToString();
                }
                catch (DbException ex)
                {
                    Trace.WriteLine("[ SqliteSettingsRepository ] Error saving setting: " + ex.Message);
                    throw;
                }
            }
        }

        public string? LoadSettingSync(string settingKey)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
                    SELECT setting_value
                    FROM settings
                    WHERE setting_name = $settingKey;
                ";

                using var command = connection.CreateCommand();
                command.Parameters.AddWithValue("$settingKey", settingKey);
                command.CommandText = sql;
                try
                {
                    var result = command.ExecuteScalar();
                    string message = $"[ SqliteSettingsRepository.LoadSettingSync ] Loaded {settingKey}={result?.ToString()}";
                    Trace.WriteLine(message);
                    Debug.WriteLine(message);
                    return result?.ToString();
                }
                catch (DbException ex)
                {
                    Trace.WriteLine("[ SqliteSettingsRepository ] Error saving setting: " + ex.Message);
                    throw;
                }
            }
        }
    }
}

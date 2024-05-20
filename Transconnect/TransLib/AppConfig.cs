using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;
namespace TransLib;

public class AppConfig {

    public required string bing_maps_key { get; set; }
    public required string mysql_connection_string { get; set; }

    /// <summary>
    /// Reads the app configuration.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static AppConfig read_config(string path)
    {
        using (StreamReader reader = new System.IO.StreamReader(path))
        {
            string text = reader.ReadToEnd();
            AppConfig? config = JsonSerializer.Deserialize<AppConfig>(text);
            if (config == null) throw new Exception("Failed to read config file");
            return config;
        }
    }

    /// <summary>
    /// Connects to the database, query and returns the reader
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public async Task<DbDataReader> query(MySqlCommand command)
    {
        MySqlConnection connection = new MySqlConnection(this.mysql_connection_string);
        await connection.OpenAsync();
        command.Connection = connection;
        return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns></returns>
    public async Task<MySqlConnection> execute(MySqlCommand command)
    {
        MySqlConnection connection = new MySqlConnection(this.mysql_connection_string);
        await connection.OpenAsync();
        command.Connection = connection;
        await command.ExecuteNonQueryAsync();
        return connection;
    }
}
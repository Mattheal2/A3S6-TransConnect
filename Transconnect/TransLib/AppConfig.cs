using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;
namespace TransLib;

public class AppConfig {

    public required string bing_maps_key { get; set; }
    public required string mysql_connection_string { get; set; }
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

    /// Connects to the database, query and returns the reader
    public async Task<DbDataReader?> query(MySqlCommand command)
    {
        MySqlConnection connection = new MySqlConnection(this.mysql_connection_string);
        await connection.OpenAsync();
        try
        {
            command.Connection = connection;
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
using Core.Models;
using MySqlConnector;

namespace Core.Managers;

public class DatabaseManager
{
    private readonly string _connectionString;
    public bool Initialized = false;

    public DatabaseManager(Config.DatabaseConfig databaseConfig)
    {
        MySqlConnectionStringBuilder builder = new()
        {
            Server = databaseConfig.Host,
            Database = databaseConfig.Database,
            UserID = databaseConfig.Username,
            Password = databaseConfig.Password,
            Port = databaseConfig.Port,
        };

        _connectionString = builder.ConnectionString;
    }

    public async Task Initialize()
    {

        await CreateTablesAsync();
        Initialized = true;
    }

    private async Task CreateTablesAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await ExecuteCommandAsync(connection, @"
            CREATE TABLE Players (
            id INT PRIMARY KEY AUTO_INCREMENT,
            steamid64 VARCHAR(63) NOT NULL UNIQUE,
            name VARCHAR(255),
            lastconnect DATETIME
            );");

        await ExecuteCommandAsync(connection, $@"
            CREATE TABLE Services (
            id INT PRIMARY KEY AUTO_INCREMENT,
            availability TINYINT(1) DEFAULT {ServiceAvailability.Enabled},
            player_id INT NOT NULL,
            start_date DATE,
            end_date DATE,
            flags VARCHAR(255),
            group_id VARCHAR(64),
            notes TEXT,
            FOREIGN KEY (player_id) REFERENCES Player(id),
            FOREIGN KEY (server_id) REFERENCES Server(server_id)
            );");
    }

    private static async Task ExecuteCommandAsync(MySqlConnection connection, string commandText)
    {
        using var command = new MySqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> GetPlayerData(ulong steamid64, string name)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO Players(steamid64, name, lastconnect) 
            VALUES (@steamid64, @name, @lastconnect)
            ON DUPLICATE KEY UPDATE name = @name, lastconnect = @lastconnect;
            SELECT id FROM Players WHERE steamid64 = @steamid64;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@steamid64", steamid64);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("@lastconnect", DateTime.UtcNow);

        int id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return id;
    }

    public async Task<List<PlayerServiceData>> GetPlayerServices(int playerId, ServiceAvailability availability = ServiceAvailability.Enabled)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT id, availability, start_date, end_date, flags, group_id, notes
            FROM Services WHERE 
            player_id = @playerId
            AND (availability & @availability) !=0
            ";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@playerId", playerId);
        command.Parameters.AddWithValue("@availability", (int)availability);

        using var reader = await command.ExecuteReaderAsync();
        List<PlayerServiceData> services = [];
        while (await reader.ReadAsync())
        {
            var service = new PlayerServiceData
            {
                Id = reader.GetInt32("id"),
                Availability = (ServiceAvailability)reader.GetInt32("availability"),
                Start = reader.GetDateTime("start_date"),
                End = reader.GetDateTime("end_date"),
                Flags = PlayerServiceData.FlagsToList(reader.GetString("flags")),
                GroupId = reader.IsDBNull(reader.GetOrdinal("group_id")) ? string.Empty : reader.GetString("group_id"),
                Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? string.Empty : reader.GetString("notes")
            };

            services.Add(service);
        }

        return services;
    }

    public async Task<int> AddNewService(int playerId, DateTime start, DateTime end, string flags, string groupId, string notes = "")
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO Services(availability, player_id, start_date, end_date, flags, group_id, notes)
            VALUES(@availability, @playerId, @start, @end, @flags, @groupId, @notes)
            SELECT LAST_INSERT_ID();";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@availability", ServiceAvailability.Enabled);
        command.Parameters.AddWithValue("@playerId", playerId);
        command.Parameters.AddWithValue("@start", start);
        command.Parameters.AddWithValue("@end", end);
        command.Parameters.AddWithValue("@flags", flags);
        command.Parameters.AddWithValue("@groupId", groupId);
        command.Parameters.AddWithValue("@notes", notes);

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? reader.GetInt32(0) : throw new Exception("Added new service but didn't get the last inserted id");
    }

    public async Task<int> RemoveService(int serviceId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            DELETE FROM Services WHERE id = @serviceId;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@serviceId", serviceId);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected;
    }

    public async Task<int> SetServiceAvailability(int serviceId, ServiceAvailability availability)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            UPDATE Services SET availability = @availability WHERE id = @serviceId;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@serviceId", serviceId);
        command.Parameters.AddWithValue("@availability", availability);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected;
    }
}

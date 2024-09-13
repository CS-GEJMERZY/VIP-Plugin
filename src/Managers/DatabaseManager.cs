using Core.Models;
using MySqlConnector;

namespace Core.Managers;

public class DatabaseManager
{
    private readonly string _connectionString;
    public bool Initialized = false;
    private readonly string Prefix = string.Empty;

    public DatabaseManager(Config.DatabaseSqlServerConfig databaseConfig)
    {
        MySqlConnectionStringBuilder builder = new()
        {
            Server = databaseConfig.Host,
            Database = databaseConfig.Database,
            UserID = databaseConfig.Username,
            Password = databaseConfig.Password,
            Port = databaseConfig.Port,
        };

        Prefix = databaseConfig.Prefix;
        if (!string.IsNullOrEmpty(Prefix) && !Prefix.EndsWith('_'))
        {
            Prefix += '_';
        }
      
        _connectionString = builder.ConnectionString;
    }

    public async Task Initialize()
    {

        await CreateTablesAsync();
        Initialized = true;
    }

    private async Task CreateTablesAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await ExecuteCommandAsync(connection, $@"
            CREATE TABLE IF NOT EXISTS {Prefix}Players (
                id INT PRIMARY KEY AUTO_INCREMENT,
                steamid64 VARCHAR(63) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL UNIQUE,
                name VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
                lastconnect TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;");

            await ExecuteCommandAsync(connection, $@"

            CREATE TABLE IF NOT EXISTS {Prefix}Services (
                id INT PRIMARY KEY AUTO_INCREMENT,
                availability INT DEFAULT {(int)ServiceAvailability.Enabled},
                player_id INT NOT NULL,
                start_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                end_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                flags VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
                group_id VARCHAR(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
                notes TEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
                FOREIGN KEY (player_id) REFERENCES {Prefix}Players(id)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;");
        }
                                      
        catch (Exception e)
        {
            throw new Exception("Encountered exception while creating tables", e);
        }
    }

    private static async Task ExecuteCommandAsync(MySqlConnection connection, string commandText)
    {
        try
        {
            using var command = new MySqlCommand(commandText, connection);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            throw new Exception($"Error executing SQL command: {commandText}", e);
        }
    }

    public async Task<int> GetPlayerId(ulong steamid64, string name)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            INSERT INTO {Prefix}Players(steamid64, name, lastconnect) 
            VALUES (@steamid64, @name, @lastconnect)
            ON DUPLICATE KEY UPDATE name = @name, lastconnect = @lastconnect;
            SELECT id FROM {Prefix}Players WHERE steamid64 = @steamid64;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@steamid64", steamid64);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("@lastconnect", DateTime.UtcNow);

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? reader.GetInt32(0) : throw new Exception("Couldn't retrieve player's id");
    }

    public async Task<int?> GetPlayerIdRaw(ulong steamid64)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            SELECT id FROM {Prefix}Players WHERE steamid64 = @steamid64;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@steamid64", steamid64);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? reader.GetInt32(0) : null;
    }

    public async Task<List<PlayerServiceData>> GetPlayerServices(int playerId, ServiceAvailability availability = ServiceAvailability.Enabled)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            SELECT id, availability, start_date, end_date, flags, group_id, notes
            FROM {Prefix}Services WHERE 
            player_id = @playerId
            AND (availability & @availability) != 0;
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
                PlayerId = playerId,
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

    public async Task<PlayerServiceData?> GetService(int serviceId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            SELECT availability, player_id, start_date, end_date, flags, group_id, notes 
            FROM {Prefix}Services WHERE id = @serviceId;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@serviceId", serviceId);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var service = new PlayerServiceData
            {
                Id = serviceId,
                PlayerId = reader.GetInt32("player_id"),
                Availability = (ServiceAvailability)reader.GetInt32("availability"),
                Start = reader.GetDateTime("start_date"),
                End = reader.GetDateTime("end_date"),
                Flags = PlayerServiceData.FlagsToList(reader.GetString("flags")),
                GroupId = reader.IsDBNull(reader.GetOrdinal("group_id")) ? string.Empty : reader.GetString("group_id"),
                Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? string.Empty : reader.GetString("notes")
            };

            return service;
        }

        return null;
    }

    public async Task<int> AddNewService(int playerId, DateTime start, DateTime end, string flags, string groupId, string notes = "")
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            INSERT INTO {Prefix}Services(availability, player_id, start_date, end_date, flags, group_id, notes)
            VALUES(@availability, @playerId, @start, @end, @flags, @groupId, @notes);
            SELECT LAST_INSERT_ID();
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@availability", (int)ServiceAvailability.Enabled);
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

        string query = $@"
            DELETE FROM {Prefix}Services WHERE id = @serviceId;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@serviceId", serviceId);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected;
    }

    public async Task<int> SetServiceAvailability(int serviceId, ServiceAvailability availability)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            UPDATE {Prefix}Services SET availability = @availability WHERE id = @serviceId;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@serviceId", serviceId);
        command.Parameters.AddWithValue("@availability", availability);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected;
    }

    public async Task<int> SetServiceEndTime(int serviceId, DateTime end)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            UPDATE {Prefix}Services SET end_date = @end WHERE id = @serviceId;
        ";
      
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@serviceId", serviceId);
        command.Parameters.AddWithValue("@end", end);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected;
    }
}
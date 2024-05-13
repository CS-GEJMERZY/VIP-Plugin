using Core.Models;
using MySqlConnector;

namespace Core.Managers;

public class DatabaseManager
{
    private readonly string _connectionString;
    public bool Initialized = false;

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

            await ExecuteCommandAsync(connection, @"
            CREATE TABLE IF NOT EXISTS Players (
            id INT PRIMARY KEY AUTO_INCREMENT,
            steamid64 VARCHAR(63) NOT NULL UNIQUE,
            name VARCHAR(255),
            lastconnect TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );");

            await ExecuteCommandAsync(connection, $@"
            CREATE TABLE IF NOT EXISTS Services (
            id INT PRIMARY KEY AUTO_INCREMENT,
            availability INT DEFAULT {(int)ServiceAvailability.Enabled},
            player_id INT NOT NULL,
            start_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            end_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            flags VARCHAR(255),
            group_id VARCHAR(64),
            notes TEXT,
            FOREIGN KEY (player_id) REFERENCES Players(id)
            );");

            await ExecuteCommandAsync(connection, @"
            CREATE TABLE IF NOT EXISTS TestVip (
            id INT PRIMARY KEY AUTO_INCREMENT,
            player_id INT NOT NULL,
            mode INT NOT NULL,
            start_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            end_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            timeleft INT NULL,
            completed BOOLEAN,
            FOREIGN KEY (player_id) REFERENCES Players(id)
             );");
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

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? reader.GetInt32(0) : throw new Exception("Couldn't retrieve player's id");
    }

    public async Task<int?> GetPlayerIdRaw(ulong steamid64)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT id FROM Players WHERE steamid64 = @steamid64;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@steamid64", steamid64);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? reader.GetInt32(0) : null;
    }

    public async Task<List<PlayerServiceData>> GetPlayerServices(int playerId, ServiceAvailability availability = ServiceAvailability.Enabled)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT id, availability, start_date, end_date, flags, group_id, notes
            FROM Services WHERE 
            player_id = @playerId
            AND (availability & @availability) != 0;";

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

        const string query = @"
            SELECT availability, player_id, start_date, end_date, flags, group_id, notes 
            FROM Services WHERE id = @serviceId";

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

        const string query = @"
            INSERT INTO Services(availability, player_id, start_date, end_date, flags, group_id, notes)
            VALUES(@availability, @playerId, @start, @end, @flags, @groupId, @notes);
            SELECT LAST_INSERT_ID();";

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

    public async Task<int> SetServiceEndTime(int serviceId, DateTime end)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            UPDATE Services SET end = @end WHERE id = @serviceId;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@serviceId", serviceId);
        command.Parameters.AddWithValue("@end", end);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected;
    }

    public async Task<TestVipData?> GetPlayerTestVipData(int playerId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT id, player_id, mode, start_date, end_date, timeleft, completed FROM TestVip
            WHERE player_id = @playerId AND completed = false;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@player_id", playerId);

        var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var testVipData = new TestVipData
            {
                Id = reader.GetInt32("id"),
                PlayerId = playerId,
                Mode = (TestVipMode)reader.GetInt32("mode"),
                Start = reader.GetDateTime("start_date"),
                End = reader.IsDBNull(reader.GetOrdinal("end_date")) ? null : reader.GetDateTime("end_date"),
                TimeLeft = reader.IsDBNull(reader.GetOrdinal("timeleft")) ? null : reader.GetInt32("timeleft"),
                Completed = reader.GetBoolean("completed")
            };

            return testVipData;
        }

        return null;
    }

    public async Task<int> InsertTestVip(TestVipData testVipData)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO TestVip(player_id, mode, start_date, end_date, timeleft, completed
            VALUES(@PlayerId, @Mode, @Start, @End, @TimeLeft, @Completed);
            SELECT LAST_INSERT_ID();";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@PlayerId", testVipData.PlayerId);
        command.Parameters.AddWithValue("@Mode", testVipData.Mode);
        command.Parameters.AddWithValue("@Start", testVipData.Start);
        command.Parameters.AddWithValue("@End", testVipData.End);
        command.Parameters.AddWithValue("@TimeLeft", testVipData.TimeLeft);
        command.Parameters.AddWithValue("@Completed", testVipData.Completed);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            int id = reader.GetInt32(0);
            testVipData.Id = id;
            return id;
        }
        else
        {
            throw new Exception("Inserted test vip, but didn't get the last inserted id");
        }
    }

    public async Task<int> UpdateTestVipCompleted(int TestVipId, bool Completed)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            UPDATE TestVip SET completed = @Completed WHERE id = @TestVipId;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@TestVipId", TestVipId);
        command.Parameters.AddWithValue("@Completed", Completed);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected;
    }

    public async Task<int> UpdateTestVipEndTime(int TestVipId, DateTime End)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            UPDATE TestVip SET end_date = @End WHERE id = @TestVipId;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@TestVipId", TestVipId);
        command.Parameters.AddWithValue("@End", End);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected;
    }

    public async Task<DateTime?> GetPlayerTestVipLatestUsedDate(int playerId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT MAX(start_date) AS latest_used_date
                    FROM TestVip
                    WHERE player_id = @playerId AND completed = true;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@playerId", playerId);

        var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            DateTime time = reader.GetDateTime(0);
            return time;
        }

        return null;
    }
}

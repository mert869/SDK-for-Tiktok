using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace TikTokRecoverySdk;

public sealed class TikTokRecoveryDatabase
{
    private readonly string _connectionString;

    public TikTokRecoveryDatabase(string databasePath = "recovery.db")
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Veritabanı yolu boş olamaz.", nameof(databasePath));
        }

        _connectionString = $"Data Source={databasePath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = @"PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        pragma.ExecuteNonQuery();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS RecoverySessions (
                SessionId TEXT PRIMARY KEY,
                FullName TEXT NOT NULL,
                Username TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                Email TEXT NOT NULL,
                PhoneNumber TEXT NOT NULL,
                VerificationCode TEXT,
                Status TEXT NOT NULL,
                Message TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    public async Task SaveSessionAsync(RecoverySession session)
    {
        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO RecoverySessions
                (SessionId, FullName, Username, PasswordHash, Email, PhoneNumber,
                 VerificationCode, Status, Message, CreatedAt, UpdatedAt)
            VALUES
                ($sessionId, $fullName, $username, $passwordHash, $email, $phoneNumber,
                 $verificationCode, $status, $message, $createdAt, $updatedAt);";
        command.Parameters.AddWithValue("$sessionId", session.SessionId);
        command.Parameters.AddWithValue("$fullName", session.FullName);
        command.Parameters.AddWithValue("$username", session.Username);
        command.Parameters.AddWithValue("$passwordHash", session.PasswordHash);
        command.Parameters.AddWithValue("$email", session.Email);
        command.Parameters.AddWithValue("$phoneNumber", session.PhoneNumber);
        command.Parameters.AddWithValue("$verificationCode", session.VerificationCode ?? string.Empty);
        command.Parameters.AddWithValue("$status", session.Status);
        command.Parameters.AddWithValue("$message", session.Message ?? string.Empty);
        command.Parameters.AddWithValue("$createdAt", session.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", session.UpdatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<RecoverySession?> GetSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT SessionId, FullName, Username, PasswordHash, Email, PhoneNumber,
                   VerificationCode, Status, Message, CreatedAt, UpdatedAt
            FROM RecoverySessions
            WHERE SessionId = $sessionId;";
        command.Parameters.AddWithValue("$sessionId", sessionId);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new RecoverySession(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetString(8),
            DateTime.Parse(reader.GetString(9)),
            DateTime.Parse(reader.GetString(10)));
    }

    public async Task<List<RecoverySession>> GetAllSessionsAsync()
    {
        var sessions = new List<RecoverySession>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT SessionId, FullName, Username, PasswordHash, Email, PhoneNumber,
                   VerificationCode, Status, Message, CreatedAt, UpdatedAt
            FROM RecoverySessions
            ORDER BY UpdatedAt DESC;";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            sessions.Add(new RecoverySession(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7),
                reader.GetString(8),
                DateTime.Parse(reader.GetString(9)),
                DateTime.Parse(reader.GetString(10))));
        }

        return sessions;
    }

    public async Task<RecoverySession?> FindSessionByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT SessionId, FullName, Username, PasswordHash, Email, PhoneNumber,
                   VerificationCode, Status, Message, CreatedAt, UpdatedAt
            FROM RecoverySessions
            WHERE Username = $username
            LIMIT 1;";
        command.Parameters.AddWithValue("$username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new RecoverySession(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetString(8),
            DateTime.Parse(reader.GetString(9)),
            DateTime.Parse(reader.GetString(10)));
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM RecoverySessions
            WHERE SessionId = $sessionId;";
        command.Parameters.AddWithValue("$sessionId", sessionId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdateVerificationCodeAsync(string sessionId, string verificationCode)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE RecoverySessions
            SET VerificationCode = $verificationCode,
                UpdatedAt = $updatedAt
            WHERE SessionId = $sessionId;";
        command.Parameters.AddWithValue("$verificationCode", verificationCode);
        command.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$sessionId", sessionId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdateSessionStatusAsync(string sessionId, string status, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE RecoverySessions
            SET Status = $status,
                Message = $message,
                UpdatedAt = $updatedAt
            WHERE SessionId = $sessionId;";
        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$message", message ?? string.Empty);
        command.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$sessionId", sessionId);

        return await command.ExecuteNonQueryAsync() > 0;
    }
}

public sealed record RecoverySession(
    string SessionId,
    string FullName,
    string Username,
    string PasswordHash,
    string Email,
    string PhoneNumber,
    string VerificationCode,
    string Status,
    string Message,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static RecoverySession CreateNew(
        string sessionId,
        string fullName,
        string username,
        string password,
        string email,
        string phoneNumber)
    {
        return new RecoverySession(
            sessionId,
            fullName,
            username,
            HashPassword(password),
            email,
            phoneNumber,
            string.Empty,
            "started",
            string.Empty,
            DateTime.UtcNow,
            DateTime.UtcNow);
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty));
        return Convert.ToHexString(hashBytes);
    }
}

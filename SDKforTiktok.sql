-- SQLite schema for TikTok recovery sessions
PRAGMA foreign_keys = ON;
PRAGMA journal_mode = WAL;

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
);

-- Örnek veri ekleme
-- INSERT INTO RecoverySessions (SessionId, FullName, Username, PasswordHash, Email, PhoneNumber, VerificationCode, Status, Message, CreatedAt, UpdatedAt)
-- VALUES ('session-123', 'Ad Soyad', 'kullanici', 'HASH', 'example@gmail.com', '+905551112233', '123456', 'started', 'Oturum oluşturuldu', '2026-06-03T12:00:00Z', '2026-06-03T12:00:00Z');

-- ============================================================
-- Script T-SQL alternativo para SQL Server / SQLEXPRESS03
-- A forma recomendada de criar o schema é aplicar as migrations do EF Core.
-- Execute este arquivo no SQL Server Management Studio somente se quiser
-- preparar o banco manualmente.
-- ============================================================

IF DB_ID(N'usuarios_db') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [usuarios_db]');
END;
GO

USE [usuarios_db];
GO

IF OBJECT_ID(N'dbo.usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.usuarios
    (
        id              BIGINT IDENTITY(1, 1) NOT NULL,
        nome            NVARCHAR(100) NOT NULL,
        sobrenome       NVARCHAR(100) NOT NULL,
        email           NVARCHAR(150) NOT NULL,
        genero          NVARCHAR(20) NOT NULL,
        data_nascimento DATE NULL,
        criado_em       DATETIME2 NOT NULL,
        atualizado_em   DATETIME2 NOT NULL,

        CONSTRAINT PK_usuarios PRIMARY KEY (id)
    );

    CREATE UNIQUE INDEX idx_usuarios_email
        ON dbo.usuarios(email);
END;
GO

-- Registra a migration equivalente ao schema criado manualmente. Assim,
-- Database.Migrate() não tenta criar a tabela usuarios uma segunda vez.
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__EFMigrationsHistory
    (
        MigrationId   NVARCHAR(150) NOT NULL,
        ProductVersion NVARCHAR(32) NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.__EFMigrationsHistory
    WHERE MigrationId = N'20260622193943_InitialCreate'
)
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    -- A migration original foi criada com EF Core 8.0.6. Este valor identifica
    -- o artefato histórico e não precisa acompanhar a versão atual do runtime.
    VALUES (N'20260622193943_InitialCreate', N'8.0.6');
END;
GO

-- Dados opcionais para testes manuais. Cada registro só é inserido
-- quando o e-mail ainda não existe, permitindo reexecutar o script.
IF NOT EXISTS (SELECT 1 FROM dbo.usuarios WHERE email = N'victor@exemplo.com')
BEGIN
    INSERT INTO dbo.usuarios
        (nome, sobrenome, email, genero, data_nascimento, criado_em, atualizado_em)
    VALUES
        (N'Victor', N'Souza', N'victor@exemplo.com', N'Masculino', '1998-07-14', SYSUTCDATETIME(), SYSUTCDATETIME());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.usuarios WHERE email = N'ana@exemplo.com')
BEGIN
    INSERT INTO dbo.usuarios
        (nome, sobrenome, email, genero, data_nascimento, criado_em, atualizado_em)
    VALUES
        (N'Ana', N'Lima', N'ana@exemplo.com', N'Feminino', '2000-03-22', SYSUTCDATETIME(), SYSUTCDATETIME());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.usuarios WHERE email = N'carlos@exemplo.com')
BEGIN
    INSERT INTO dbo.usuarios
        (nome, sobrenome, email, genero, data_nascimento, criado_em, atualizado_em)
    VALUES
        (N'Carlos', N'Mendes', N'carlos@exemplo.com', N'Masculino', '1985-11-05', SYSUTCDATETIME(), SYSUTCDATETIME());
END;
GO

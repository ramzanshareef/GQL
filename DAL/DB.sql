USE master;
GO

IF DB_ID('GQLDb') IS NOT NULL
BEGIN
	DROP DATABASE GQLDb;
END
GO

CREATE DATABASE GQLDb;
GO

USE GQLDb;
GO

IF OBJECT_ID('dbo.Todos', 'U') IS NOT NULL DROP TABLE dbo.Todos;
GO

CREATE TABLE dbo.Todos (
	TodoId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
	Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Status BIT DEFAULT 0
);
GO

INSERT INTO dbo.Todos (UserId, Title, Description, Status)
VALUES
(1, 'Buy groceries', 'Milk, eggs, bread', 0),
(1, 'Complete GraphQL API', 'Finish queries and mutations', 1),
(1, 'Workout', 'Gym session for 1 hour', 1),
(2, 'Read book', 'Read 30 pages of a novel', 1),
(3, 'Deploy app', 'Deploy .NET app to Azure', 0);
GO

SELECT * FROM dbo.Todos;
GO
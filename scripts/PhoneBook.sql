IF DB_ID('PhoneBook') IS NULL
BEGIN
    CREATE DATABASE PhoneBook;
END
GO

USE PhoneBook;
GO

IF OBJECT_ID('dbo.PhoneContacts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhoneContacts
    (
        Id INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_PhoneContacts PRIMARY KEY,

        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(50) NULL,
        Email NVARCHAR(255) NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PhoneContacts)
BEGIN
    INSERT INTO dbo.PhoneContacts
        (FirstName, LastName, PhoneNumber, Email)
    VALUES
        ('John', 'Smith', '+1 555-100-1000', 'john.smith@example.com'),
        ('Jane', 'Doe', '+1 555-100-1001', 'jane.doe@example.com'),
        ('Michael', 'Brown', '+1 555-100-1002', 'michael.brown@example.com'),
        ('Emily', 'Wilson', '+1 555-100-1003', 'emily.wilson@example.com'),
        ('David', 'Taylor', '+1 555-100-1004', 'david.taylor@example.com'),
        ('Sarah', 'Anderson', '+1 555-100-1005', 'sarah.anderson@example.com');
END
GO

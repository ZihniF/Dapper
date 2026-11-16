SET XACT_ABORT ON;

IF OBJECT_ID('dbo.City', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.City
    (
        Id int NOT NULL PRIMARY KEY,
        Name nvarchar(40) NOT NULL UNIQUE,
        Scale decimal(8, 3) NOT NULL,
        Climate tinyint NOT NULL
    );

    CREATE TABLE dbo.District
    (
        Id int NOT NULL PRIMARY KEY,
        CityId int NOT NULL REFERENCES dbo.City(Id),
        Name nvarchar(60) NOT NULL,
        CONSTRAINT UQ_District UNIQUE (CityId, Name)
    );

    CREATE TABLE dbo.Category
    (
        Id int NOT NULL PRIMARY KEY,
        Name nvarchar(20) NOT NULL
    );

    INSERT dbo.Category (Id, Name)
    VALUES
        (1, N'Konut'),
        (2, N'Ticari'),
        (3, N'Sanayi');

    CREATE TABLE dbo.Consumption
    (
        Id bigint IDENTITY PRIMARY KEY,
        DistrictId int NOT NULL REFERENCES dbo.District(Id),
        [Date] date NOT NULL,
        CategoryId int NOT NULL REFERENCES dbo.Category(Id),
        Kwh decimal(18, 2) NOT NULL CHECK (Kwh > 0),
        Subscribers int NOT NULL CHECK (Subscribers > 0),
        [Version] rowversion NOT NULL,
        CONSTRAINT UQ_Consumption UNIQUE (DistrictId, CategoryId, [Date])
    );

    CREATE INDEX IX_District_City
        ON dbo.District (CityId, Id);

    CREATE INDEX IX_Consumption_Date
        ON dbo.Consumption ([Date], CategoryId)
        INCLUDE (DistrictId, Kwh, Subscribers);
END;

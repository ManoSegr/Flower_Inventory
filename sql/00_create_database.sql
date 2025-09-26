IF DB_ID(N'FlowerShop') IS NULL
BEGIN
    CREATE DATABASE FlowerShop;
END
GO
USE FlowerShop;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'inv')
    EXEC('CREATE SCHEMA inv');
GO

IF OBJECT_ID(N'inv.Category', N'U') IS NULL
BEGIN
    CREATE TABLE inv.Category
    (
        CategoryId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Category PRIMARY KEY,
        Name         NVARCHAR(100)     NOT NULL,
        Description  NVARCHAR(500)     NULL,
        CreatedAt    DATETIME2(0)      NOT NULL CONSTRAINT DF_Category_CreatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion   ROWVERSION        NOT NULL,
        CONSTRAINT UQ_Category_Name UNIQUE(Name)
    );
END
GO

/* Flower table */
IF OBJECT_ID(N'inv.Flower', N'U') IS NULL
BEGIN
    CREATE TABLE inv.Flower
    (
        FlowerId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Flower PRIMARY KEY,
        CategoryId       INT              NOT NULL,
        Name             NVARCHAR(150)    NOT NULL,
        [Type]           NVARCHAR(50)     NOT NULL, 
        SKU              NVARCHAR(50)     NULL,
        Price            DECIMAL(10,2)    NOT NULL CONSTRAINT CK_Flower_Price CHECK (Price >= 0),
        QuantityInStock  INT              NOT NULL CONSTRAINT CK_Flower_Qty CHECK (QuantityInStock >= 0),
        Color            NVARCHAR(30)     NULL,
        StemLengthCm     DECIMAL(5,2)     NULL CONSTRAINT CK_Flower_Stem CHECK (StemLengthCm >= 0),
        ImageUrl         NVARCHAR(2083)   NULL,
        Active           BIT              NOT NULL CONSTRAINT DF_Flower_Active DEFAULT (1),
        CreatedAt        DATETIME2(0)     NOT NULL CONSTRAINT DF_Flower_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt        DATETIME2(0)     NOT NULL CONSTRAINT DF_Flower_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion       ROWVERSION       NOT NULL,
        IsInStock AS CAST(CASE WHEN QuantityInStock > 0 THEN 1 ELSE 0 END AS BIT) PERSISTED,
        CONSTRAINT FK_Flower_Category FOREIGN KEY(CategoryId) REFERENCES inv.Category(CategoryId) ON DELETE NO ACTION,
        CONSTRAINT UQ_Flower_SKU UNIQUE(SKU)
    );

    CREATE INDEX IX_Flower_CategoryId ON inv.Flower(CategoryId);
    CREATE INDEX IX_Flower_Name       ON inv.Flower(Name);
    CREATE INDEX IX_Flower_Active_InStock ON inv.Flower(Active, IsInStock) WHERE Active = 1;
END
GO
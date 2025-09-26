USE FlowerShop;
GO

/* Create */
CREATE OR ALTER PROCEDURE inv.usp_Flower_Create
    @CategoryId      INT,
    @Name            NVARCHAR(150),
    @Type            NVARCHAR(50),
    @SKU             NVARCHAR(50) = NULL,
    @Price           DECIMAL(10,2),
    @QuantityInStock INT = 0,
    @Color           NVARCHAR(30) = NULL,
    @StemLengthCm    DECIMAL(5,2) = NULL,
    @ImageUrl        NVARCHAR(2083) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO inv.Flower
        (CategoryId, Name, [Type], SKU, Price, QuantityInStock, Color, StemLengthCm, ImageUrl)
    VALUES
        (@CategoryId, @Name, @Type, @SKU, @Price, @QuantityInStock, @Color, @StemLengthCm, @ImageUrl);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS FlowerId;
END
GO

/* Update */
CREATE OR ALTER PROCEDURE inv.usp_Flower_Update
    @FlowerId        INT,
    @CategoryId      INT,
    @Name            NVARCHAR(150),
    @Type            NVARCHAR(50),
    @SKU             NVARCHAR(50) = NULL,
    @Price           DECIMAL(10,2),
    @QuantityInStock INT,
    @Color           NVARCHAR(30) = NULL,
    @StemLengthCm    DECIMAL(5,2) = NULL,
    @ImageUrl        NVARCHAR(2083) = NULL,
    @Active          BIT,
    @OriginalRowVersion VARBINARY(8)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE inv.Flower
    SET CategoryId = @CategoryId,
        Name = @Name,
        [Type] = @Type,
        SKU = @SKU,
        Price = @Price,
        QuantityInStock = @QuantityInStock,
        Color = @Color,
        StemLengthCm = @StemLengthCm,
        ImageUrl = @ImageUrl,
        Active = @Active,
        UpdatedAt = SYSUTCDATETIME()
    WHERE FlowerId = @FlowerId AND RowVersion = @OriginalRowVersion;

    IF @@ROWCOUNT = 0
        THROW 51000, 'Concurrency conflict or record not found.', 1;
END
GO

/* Delete */
CREATE OR ALTER PROCEDURE inv.usp_Flower_Delete
    @FlowerId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM inv.Flower WHERE FlowerId = @FlowerId;
    IF @@ROWCOUNT = 0 THROW 51001, 'Record not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE inv.usp_Flower_Get
    @FlowerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.*, c.Name AS CategoryName
    FROM inv.Flower f
        INNER JOIN inv.Category c ON c.CategoryId = f.CategoryId
    WHERE f.FlowerId = @FlowerId;
END
GO

CREATE OR ALTER PROCEDURE inv.usp_Category_Create
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO inv.Category
        (Name, Description)
    VALUES
        (@Name, @Description);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS CategoryId;
END
GO

CREATE OR ALTER PROCEDURE inv.usp_Category_Update
    @CategoryId INT,
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @OriginalRowVersion VARBINARY(8)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE inv.Category SET Name = @Name, Description = @Description
    WHERE CategoryId = @CategoryId AND RowVersion = @OriginalRowVersion;
    IF @@ROWCOUNT = 0 THROW 51000, 'Concurrency conflict or record not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE inv.usp_Category_Delete
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM inv.Category WHERE CategoryId = @CategoryId;
    IF @@ROWCOUNT = 0 THROW 51001, 'Record not found.', 1;
END
GO
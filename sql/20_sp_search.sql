USE FlowerShop;
GO
CREATE OR ALTER PROCEDURE inv.usp_Flower_Search
    @q          NVARCHAR(100) = NULL,
    @categoryId INT           = NULL,
    @minPrice   DECIMAL(10,2) = NULL,
    @maxPrice   DECIMAL(10,2) = NULL,
    @activeOnly BIT           = 1,
    @sort       NVARCHAR(20)  = N'name',
    
    @dir        NVARCHAR(4)   = N'ASC',
  
    @page       INT           = 1,
    @pageSize   INT           = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @pageSize < 1 SET @pageSize = 10;

    DECLARE @offset INT = (@page - 1) * @pageSize;

    DECLARE @orderBy NVARCHAR(50) =
    CASE LOWER(@sort)
      WHEN 'name'      THEN N'f.Name'
      WHEN 'price'     THEN N'f.Price'
      WHEN 'category'  THEN N'c.Name'
      WHEN 'createdat' THEN N'f.CreatedAt'
      ELSE N'f.Name'
    END;

    DECLARE @direction NVARCHAR(4) = CASE UPPER(@dir) WHEN 'DESC' THEN N'DESC' ELSE N'ASC' END;

    DECLARE @sql NVARCHAR(MAX) = N'
    SELECT
      f.FlowerId, f.Name, f.[Type], f.SKU, f.Price, f.QuantityInStock,
      f.Color, f.StemLengthCm, f.ImageUrl, f.IsInStock, f.Active,
      f.CreatedAt, f.UpdatedAt, f.RowVersion,
      c.CategoryId, c.Name AS CategoryName,
      COUNT(1) OVER() AS TotalCount
    FROM inv.Flower AS f
    INNER JOIN inv.Category AS c ON c.CategoryId = f.CategoryId
    WHERE ( @q IS NULL OR f.Name LIKE N''%'' + @q + N''%'' OR c.Name LIKE N''%'' + @q + N''%'' )
      AND ( @categoryId IS NULL OR f.CategoryId = @categoryId )
      AND ( @minPrice IS NULL OR f.Price >= @minPrice )
      AND ( @maxPrice IS NULL OR f.Price <= @maxPrice )
      AND ( @activeOnly = 0 OR f.Active = 1 )
    ORDER BY ' + @orderBy + N' ' + @direction + N'
    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;';

    EXEC sp_executesql
    @sql,
    N'@q NVARCHAR(100), @categoryId INT, @minPrice DECIMAL(10,2), @maxPrice DECIMAL(10,2), @activeOnly BIT, @offset INT, @pageSize INT',
    @q=@q, @categoryId=@categoryId, @minPrice=@minPrice, @maxPrice=@maxPrice, @activeOnly=@activeOnly,
    @offset=@offset, @pageSize=@pageSize;
END
GO

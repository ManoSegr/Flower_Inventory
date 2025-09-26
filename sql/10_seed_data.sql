USE FlowerShop;
GO

/* Categories */
IF NOT EXISTS (SELECT 1
FROM inv.Category
WHERE Name = N'Roses')
    INSERT INTO inv.Category
    (Name, Description)
VALUES
    (N'Roses', N'Rose varieties');
IF NOT EXISTS (SELECT 1
FROM inv.Category
WHERE Name = N'Tulips')
    INSERT INTO inv.Category
    (Name, Description)
VALUES
    (N'Tulips', N'Tulip varieties');

/* Flowers */
DECLARE @catRoses  INT = (SELECT CategoryId
FROM inv.Category
WHERE Name = N'Roses');
DECLARE @catTulips INT = (SELECT CategoryId
FROM inv.Category
WHERE Name = N'Tulips');

IF NOT EXISTS (SELECT 1
FROM inv.Flower
WHERE Name = N'Red Naomi')
INSERT INTO inv.Flower
    (CategoryId, Name, [Type], SKU, Price, QuantityInStock, Color, StemLengthCm)
VALUES
    (@catRoses, N'Red Naomi', N'Cut', N'ROSE-RED-NAOMI', 3.50, 120, N'Red', 60);

IF NOT EXISTS (SELECT 1
FROM inv.Flower
WHERE Name = N'White Avalanche')
INSERT INTO inv.Flower
    (CategoryId, Name, [Type], SKU, Price, QuantityInStock, Color, StemLengthCm)
VALUES
    (@catRoses, N'White Avalanche', N'Cut', N'ROSE-WHT-AVAL', 3.20, 80, N'White', 60);

IF NOT EXISTS (SELECT 1
FROM inv.Flower
WHERE Name = N'Queen of Night')
INSERT INTO inv.Flower
    (CategoryId, Name, [Type], SKU, Price, QuantityInStock, Color, StemLengthCm)
VALUES
    (@catTulips, N'Queen of Night', N'Potted', N'TUL-QON', 2.10, 50, N'Purple-Black', NULL);

IF NOT EXISTS (SELECT 1
FROM inv.Flower
WHERE Name = N'Apricot Beauty')
INSERT INTO inv.Flower
    (CategoryId, Name, [Type], SKU, Price, QuantityInStock, Color, StemLengthCm)
VALUES
    (@catTulips, N'Apricot Beauty', N'Cut', N'TUL-APR', 1.90, 200, N'Apricot', 35);
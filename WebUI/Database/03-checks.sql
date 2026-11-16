-- Read-only validation, run after setup in SSMS.
SELECT
    COUNT_BIG(*) AS RecordCount,
    MIN([Date]) AS FirstDate,
    MAX([Date]) AS LastDate
FROM dbo.Consumption;

SELECT COUNT(*) AS Cities
FROM dbo.City;

SELECT COUNT(*) AS Districts
FROM dbo.District;

SELECT COUNT_BIG(*) AS InvalidRows
FROM dbo.Consumption
WHERE Kwh <= 0
    OR Subscribers <= 0
    OR [Date] < '20160101'
    OR [Date] > '20251231';

SELECT
    DistrictId,
    [Date],
    CategoryId,
    COUNT_BIG(*) AS Duplicates
FROM dbo.Consumption
GROUP BY DistrictId, [Date], CategoryId
HAVING COUNT_BIG(*) > 1;

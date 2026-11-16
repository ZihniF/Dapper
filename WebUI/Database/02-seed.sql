-- AI-assisted deterministic synthetic data; not official statistics.
-- 100 real districts x 3653 days x 3 categories = 1,095,900 records.
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @lock int;

EXEC @lock = sys.sp_getapplock
    @Resource = N'EnergyPulseSeed',
    @LockMode = 'Exclusive',
    @LockOwner = 'Transaction',
    @LockTimeout = 10000;

IF @lock < 0
    THROW 50001, 'Seed lock unavailable.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.Consumption)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.City)
    BEGIN
        INSERT dbo.City (Id, Name, Scale, Climate)
        VALUES
            (34, N'İstanbul', 1.80, 0),
            (6, N'Ankara', 1.35, 1),
            (35, N'İzmir', 1.30, 2),
            (16, N'Bursa', 1.20, 0),
            (7, N'Antalya', 1.10, 2),
            (1, N'Adana', 1.00, 2),
            (42, N'Konya', 0.95, 1),
            (27, N'Gaziantep', 1.00, 2),
            (41, N'Kocaeli', 1.40, 0),
            (33, N'Mersin', 0.95, 2),
            (21, N'Diyarbakır', 0.80, 2),
            (55, N'Samsun', 0.85, 0),
            (26, N'Eskişehir', 0.85, 1),
            (38, N'Kayseri', 0.90, 1),
            (63, N'Şanlıurfa', 0.80, 2),
            (10, N'Balıkesir', 0.80, 0),
            (54, N'Sakarya', 0.85, 0),
            (45, N'Manisa', 0.90, 2),
            (20, N'Denizli', 0.90, 2),
            (61, N'Trabzon', 0.75, 0);

        INSERT dbo.District (Id, CityId, Name)
        SELECT
            c.Id * 10 + CONVERT(int, j.[key]) + 1,
            c.Id,
            j.value
        FROM dbo.City c
        CROSS APPLY OPENJSON(
            CASE c.Id
                WHEN 34 THEN N'["Kadıköy","Beşiktaş","Üsküdar","Şişli","Fatih"]'
                WHEN 6 THEN N'["Çankaya","Keçiören","Yenimahalle","Mamak","Etimesgut"]'
                WHEN 35 THEN N'["Konak","Karşıyaka","Bornova","Buca","Çiğli"]'
                WHEN 16 THEN N'["Osmangazi","Nilüfer","Yıldırım","İnegöl","Gemlik"]'
                WHEN 7 THEN N'["Muratpaşa","Kepez","Konyaaltı","Alanya","Manavgat"]'
                WHEN 1 THEN N'["Seyhan","Çukurova","Yüreğir","Sarıçam","Ceyhan"]'
                WHEN 42 THEN N'["Selçuklu","Meram","Karatay","Ereğli","Akşehir"]'
                WHEN 27 THEN N'["Şahinbey","Şehitkamil","Nizip","İslahiye","Nurdağı"]'
                WHEN 41 THEN N'["İzmit","Gebze","Darıca","Körfez","Gölcük"]'
                WHEN 33 THEN N'["Akdeniz","Mezitli","Toroslar","Yenişehir","Tarsus"]'
                WHEN 21 THEN N'["Bağlar","Kayapınar","Sur","Yenişehir","Ergani"]'
                WHEN 55 THEN N'["Atakum","İlkadım","Canik","Bafra","Çarşamba"]'
                WHEN 26 THEN N'["Odunpazarı","Tepebaşı","Sivrihisar","Çifteler","Seyitgazi"]'
                WHEN 38 THEN N'["Melikgazi","Kocasinan","Talas","Develi","İncesu"]'
                WHEN 63 THEN N'["Haliliye","Eyyübiye","Karaköprü","Siverek","Viranşehir"]'
                WHEN 10 THEN N'["Karesi","Altıeylül","Bandırma","Edremit","Ayvalık"]'
                WHEN 54 THEN N'["Adapazarı","Serdivan","Erenler","Akyazı","Hendek"]'
                WHEN 45 THEN N'["Yunusemre","Şehzadeler","Akhisar","Turgutlu","Salihli"]'
                WHEN 20 THEN N'["Pamukkale","Merkezefendi","Çivril","Acıpayam","Tavas"]'
                WHEN 61 THEN N'["Ortahisar","Akçaabat","Yomra","Arsin","Of"]'
            END
        ) j;
    END;

    ;WITH digits AS
    (
        SELECT n
        FROM (VALUES (0), (1), (2), (3), (4), (5), (6), (7), (8), (9)) x(n)
    ),
    days AS
    (
        SELECT DATEADD(
            day,
            a.n + 10 * b.n + 100 * c.n + 1000 * d.n,
            CONVERT(date, '20160101')
        ) AS dt
        FROM digits a
        CROSS JOIN digits b
        CROSS JOIN digits c
        CROSS JOIN digits d
        WHERE a.n + 10 * b.n + 100 * c.n + 1000 * d.n < 3653
    )
    INSERT dbo.Consumption (DistrictId, [Date], CategoryId, Kwh, Subscribers)
    SELECT
        d.Id,
        dt,
        k.Id,
        CAST(s.subscribers * u.daily * season.factor * noise.factor AS decimal(18, 2)),
        s.subscribers
    FROM dbo.District d
    JOIN dbo.City c ON c.Id = d.CityId
    CROSS JOIN days
    CROSS JOIN dbo.Category k
    CROSS APPLY
    (
        SELECT CAST(
            (CASE k.Id WHEN 1 THEN 6500 WHEN 2 THEN 950 ELSE 65 END)
            * c.Scale
            * (0.7 + (d.Id % 5) * 0.15)
            * (1 + (YEAR(dt) - 2016) * 0.018)
            AS int
        ) AS subscribers
    ) s
    CROSS APPLY
    (
        SELECT CASE k.Id
            WHEN 1 THEN 7.8
            WHEN 2 THEN 38.0
            ELSE 920.0
        END AS daily
    ) u
    CROSS APPLY
    (
        SELECT CASE
            WHEN k.Id = 3 THEN
                CASE
                    WHEN DATEDIFF(day, '19000101', dt) % 7 IN (5, 6) THEN 0.72
                    ELSE 1.00
                END
            WHEN c.Climate = 2 AND MONTH(dt) IN (6, 7, 8, 9) THEN 1.38
            WHEN c.Climate = 1 AND MONTH(dt) IN (11, 12, 1, 2) THEN 1.32
            WHEN MONTH(dt) IN (12, 1, 2) THEN 1.15
            ELSE 0.95
        END AS factor
    ) season
    CROSS APPLY
    (
        SELECT 0.90 + ABS(CONVERT(bigint, CHECKSUM(d.Id, dt, k.Id))) % 2001 / 10000.0 AS factor
    ) noise;
END;

COMMIT;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Consumption')
        AND name = 'IX_Consumption_Analytics'
)
BEGIN
    CREATE NONCLUSTERED COLUMNSTORE INDEX IX_Consumption_Analytics
        ON dbo.Consumption (DistrictId, [Date], CategoryId, Kwh, Subscribers);
END;

SELECT COUNT_BIG(*) AS RecordCount
FROM dbo.Consumption;

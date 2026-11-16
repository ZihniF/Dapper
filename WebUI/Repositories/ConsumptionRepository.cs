using WebUI.Data;
using Dapper;
using WebUI.DTOs;

namespace WebUI.Repositories;

public sealed class ConsumptionRepository(SqlConnectionFactory factory)
{
    private const string Projection = """
        SELECT
            r.Id,
            r.DistrictId,
            c.Name AS City,
            d.Name AS District,
            r.[Date],
            r.CategoryId,
            k.Name AS Category,
            r.Kwh,
            r.Subscribers,
            CONVERT(varchar(18), CONVERT(binary(8), r.[Version]), 1) AS [Version]
        FROM Consumption r
        JOIN District d ON d.Id = r.DistrictId
        JOIN City c ON c.Id = d.CityId
        JOIN Category k ON k.Id = r.CategoryId
        """;

    public async Task<List<LookupDto>> CitiesAsync(CancellationToken ct)
    {
        await using var db = factory.Create();
        var command = new CommandDefinition(
            "SELECT Id, Name FROM City ORDER BY Name",
            cancellationToken: ct);

        var cities = await db.QueryAsync<LookupDto>(command);
        return cities.AsList();
    }

    public async Task<PagedRecordsDto> ListAsync(RecordFilterDto filter, CancellationToken ct)
    {
        var conditions = new List<string>();

        if (filter.Id.HasValue)
        {
            conditions.Add("r.Id = @Id");
        }

        if (filter.CityId.HasValue)
        {
            conditions.Add("d.CityId = @CityId");
        }

        if (filter.CategoryId.HasValue)
        {
            conditions.Add("r.CategoryId = @CategoryId");
        }

        if (filter.From.HasValue)
        {
            conditions.Add("r.[Date] >= @From");
        }

        if (filter.To.HasValue)
        {
            conditions.Add("r.[Date] <= @To");
        }

        var where = conditions.Count == 0
            ? ""
            : " WHERE " + string.Join(" AND ", conditions);

        var parameters = new DynamicParameters(filter);
        await using var db = factory.Create();

        var countSql = """
            SELECT COUNT_BIG(*)
            FROM Consumption r
            JOIN District d ON d.Id = r.DistrictId
            """ + where;

        var total = await db.ExecuteScalarAsync<long>(new CommandDefinition(
            countSql,
            parameters,
            cancellationToken: ct));

        var lastPage = Math.Max(1, (int)Math.Ceiling((double)total / filter.PageSize));
        filter.Page = Math.Min(filter.Page, lastPage);
        parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);

        var pageSql = Projection + where + """
             ORDER BY r.Id DESC
             OFFSET @Offset ROWS
             FETCH NEXT @PageSize ROWS ONLY
             OPTION(RECOMPILE)
            """;

        var items = await db.QueryAsync<ConsumptionDto>(new CommandDefinition(
            pageSql,
            parameters,
            cancellationToken: ct));

        return new PagedRecordsDto(items.AsList(), total, filter);
    }

    public async Task<ConsumptionDto?> FindAsync(long id, CancellationToken ct)
    {
        await using var db = factory.Create();
        var command = new CommandDefinition(
            Projection + " WHERE r.Id = @id",
            new
            {
                id
            },
            cancellationToken: ct);

        return await db.QuerySingleOrDefaultAsync<ConsumptionDto>(command);
    }

    public async Task<bool> UpdateAsync(UpdateConsumptionDto dto, CancellationToken ct)
    {
        const string sql = """
            UPDATE Consumption
            SET
                Kwh = @Kwh,
                Subscribers = @Subscribers
            WHERE Id = @Id
                AND [Version] = CONVERT(binary(8), @Version, 1)
            """;

        await using var db = factory.Create();
        var affected = await db.ExecuteAsync(new CommandDefinition(
            sql,
            dto,
            cancellationToken: ct));

        return affected == 1;
    }

    public async Task<bool> DeleteAsync(long id, string version, CancellationToken ct)
    {
        const string sql = """
            DELETE FROM Consumption
            WHERE Id = @id
                AND [Version] = CONVERT(binary(8), @version, 1)
            """;

        await using var db = factory.Create();
        var affected = await db.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                id,
                version
            },
            cancellationToken: ct));

        return affected == 1;
    }

    public async Task<DashboardDto> DashboardAsync(CancellationToken ct)
    {
        const string sql = """
            SELECT
                COUNT_BIG(*) AS RecordCount,
                COALESCE(SUM(Kwh), 0) AS TotalKwh,
                COALESCE(SUM(Kwh) / NULLIF(SUM(CONVERT(bigint, Subscribers)), 0), 0)
                    AS AveragePerSubscriber,
                COUNT(DISTINCT d.CityId) AS Cities,
                MIN([Date]) AS FirstDate,
                MAX([Date]) AS LastDate
            FROM Consumption r
            JOIN District d ON d.Id = r.DistrictId;

            SELECT
                CONVERT(char(7), [Date], 126) AS Label,
                SUM(Kwh) AS Value
            FROM Consumption
            GROUP BY CONVERT(char(7), [Date], 126)
            ORDER BY Label;

            SELECT k.Name AS Label, SUM(r.Kwh) AS Value
            FROM Consumption r
            JOIN Category k ON k.Id = r.CategoryId
            GROUP BY k.Name;

            SELECT TOP (7) c.Name AS Label, SUM(r.Kwh) AS Value
            FROM Consumption r
            JOIN District d ON d.Id = r.DistrictId
            JOIN City c ON c.Id = d.CityId
            GROUP BY c.Name
            ORDER BY Value DESC;
            """;

        await using var db = factory.Create();
        using var results = await db.QueryMultipleAsync(new CommandDefinition(
            sql,
            commandTimeout: 120,
            cancellationToken: ct));

        var dto = await results.ReadSingleAsync<DashboardDto>();
        dto.Months = (await results.ReadAsync<MetricDto>()).AsList();
        dto.Categories = (await results.ReadAsync<MetricDto>()).AsList();
        dto.TopCities = (await results.ReadAsync<MetricDto>()).AsList();

        return dto;
    }
}



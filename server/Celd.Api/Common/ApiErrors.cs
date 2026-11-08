using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Common;

public static class ApiErrors
{
    public static IResult Invalid(string detail) => Results.Problem(statusCode:400, title:"Validation failed", detail:detail, type:"https://httpstatuses.io/400");
    public static IResult Unauthenticated() => Results.Problem(statusCode:401, title:"Authentication required", detail:"Sign in to continue.", type:"https://httpstatuses.io/401");
    public static IResult Forbidden() => Results.Problem(statusCode:403, title:"Forbidden", detail:"This operation is not permitted.", type:"https://httpstatuses.io/403");
    public static IResult Missing() => Results.Problem(statusCode:404, title:"Not found", detail:"The requested record was not found.", type:"https://httpstatuses.io/404");
    public static IResult Conflict(string? detail=null) => Results.Problem(statusCode:409, title:"State conflict", detail:detail ?? "The record has changed. Refresh and try again.", type:"https://httpstatuses.io/409");
    public static bool IsConcurrencyFailure(Exception ex)
    {
        if (ex is DbUpdateConcurrencyException) return true;
        if (ex is SqliteException direct && SqliteConcurrency(direct)) return true;
        if (ex is DbUpdateException db && db.InnerException is SqliteException sqlite && SqliteConcurrency(sqlite)) return true;
        return false;
    }
    private static bool SqliteConcurrency(SqliteException e) =>
        e.SqliteErrorCode is 5 or 6 || e.SqliteExtendedErrorCode is 2067 or 1555;
}

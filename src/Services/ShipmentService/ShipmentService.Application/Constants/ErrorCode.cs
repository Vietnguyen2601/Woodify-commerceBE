namespace ShipmentService.Application.Constants;

public static class ErrorCode
{
    // 4xx
    public const int BadRequest = 400;
    public const int NotFound = 404;
    public const int Conflict = 409;

    // 5xx
    public const int ServerError = 500;
}

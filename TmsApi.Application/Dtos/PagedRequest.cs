namespace TmsApi.Application.Dtos;

public record PagedRequest(int Page = 1, int PageSize = 10, string? Search = null);

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<CourseService> _logger;
    private readonly IMemoryCache _cache;

    public CourseService(
        TmsDbContext context,
        ILogger<CourseService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        var cacheKey = $"course:{id}";

        // 1. Check cache
        if (_cache.TryGetValue<CourseResponseDto>(
            cacheKey,
            out var cached))
        {
            TmsMeters.CacheHits.Add(
                1,
                new KeyValuePair<string, object?>(
                    "key.kind",
                    "course"));

            return cached;
        }

        // 2. Cache miss -> get from database
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course == null)
            return null;

        var count = await _context.Enrollments
            .CountAsync(e => e.CourseId == id, ct);

        var result = new CourseResponseDto(
            course.Id,
            course.Code,
            course.Title,
            course.MaxCapacity,
            count
        );

        // 3. Save result in cache for 5 minutes
        _cache.Set(
            cacheKey,
            result,
            TimeSpan.FromMinutes(5));

        // 4. Record cache miss
        TmsMeters.CacheMisses.Add(
            1,
            new KeyValuePair<string, object?>(
                "key.kind",
                "course"));

        return result;
    }

    public async Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct = default)
    {
        return await _context.Courses
            .AnyAsync(c => c.Code == code, ct);
    }

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct = default)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(ct);

        return new CourseResponseDto(
            course.Id,
            course.Code,
            course.Title,
            course.MaxCapacity,
            0
        );
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct = default)
    {
        var query = _context.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();

            query = query.Where(c =>
                c.Code.ToLower().Contains(search) ||
                c.Title.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);

        // N+1 to remove the N+1 problem,
        var items = await query
            .OrderBy(c => c.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                _context.Enrollments.Count(e => e.CourseId == c.Id)
            ))
            .ToListAsync(ct);

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record CreateCourseRequest(
	[param: Required] string Code,
	string Title,
	int MaxCapacity);

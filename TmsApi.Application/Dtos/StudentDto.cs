using System.Text.Json.Serialization;

namespace TmsApi.Application.Dtos;

public class StudentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string? InternalNotes { get; set; }
}
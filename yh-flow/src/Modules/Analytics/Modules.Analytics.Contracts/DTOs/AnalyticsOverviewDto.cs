using System.Text.Json.Serialization;

namespace YH.Modules.Analytics.Contracts.DTOs;

public sealed class AnalyticsOverviewDto
{
    [JsonPropertyName("total_users")] public CountValue TotalUsers { get; set; } = new();
    [JsonPropertyName("total_admins")] public CountValue TotalAdmins { get; set; } = new();
    [JsonPropertyName("total_members")] public CountValue TotalMembers { get; set; } = new();
    [JsonPropertyName("total_guests")] public CountValue TotalGuests { get; set; } = new();
    [JsonPropertyName("total_projects")] public CountValue TotalProjects { get; set; } = new();
    [JsonPropertyName("total_work_items")] public CountValue TotalWorkItems { get; set; } = new();
    [JsonPropertyName("total_cycles")] public CountValue TotalCycles { get; set; } = new();
    [JsonPropertyName("total_modules")] public CountValue TotalModules { get; set; } = new();
}

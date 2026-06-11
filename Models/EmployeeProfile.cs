namespace SheetFlow.Models;

public class EmployeeProfile
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? HighestEducation { get; set; }
    public string? SchoolName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? IdNumber { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

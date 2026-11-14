using System.ComponentModel.DataAnnotations;
using static HSCrm.Common.PublicTools.Enums;

namespace HSCrm.Models.ModelDto
{
    public class ProjectMemberDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserImageUrl { get; set; }
        public string? ProjectRole { get; set; }
        public decimal? HourlyRate { get; set; }
        public bool IsActive { get; set; }
        public DateTime JoinedAt { get; set; }
        public string JoinedAtShamsi { get; set; } = string.Empty;
    }

    public class AssignableUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public byte UserType { get; set; }
        public string? UserImageUrl { get; set; }
    }

    public class ProjectMemberUpsertModel
    {
        public int ProjectId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? ProjectRole { get; set; }
        public decimal? HourlyRate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ProjectWorkLogDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int FiscalYearId { get; set; }
        public DateTime WorkDate { get; set; }
        public string WorkDateShamsi { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public decimal HourlyRateSnapshot { get; set; }
        public decimal WageCost { get; set; }
        public string Description { get; set; } = string.Empty;
        public WorkLogStatus Status { get; set; }
        public string StatusTitle { get; set; } = string.Empty;
        public string? ReviewedByUserId { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedAtShamsi { get; set; }
        public string? ReviewNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeProjectSummaryDto
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? ProjectLocation { get; set; }
        public string? ProjectRole { get; set; }
        public decimal? HourlyRate { get; set; }
        public decimal MyTotalHours { get; set; }
        public decimal MyApprovedHours { get; set; }
        public decimal MyPendingHours { get; set; }
        public decimal MyApprovedWageCost { get; set; }
    }

    public class EmployeeDashboardDto
    {
        public bool IsFullAccess { get; set; }
        public int AssignedProjectsCount { get; set; }
        public decimal TotalLoggedHours { get; set; }
        public decimal ApprovedHours { get; set; }
        public decimal PendingHours { get; set; }
        public decimal RejectedHours { get; set; }
        public decimal ApprovedWageCost { get; set; }
        public List<EmployeeProjectSummaryDto> Projects { get; set; } = new();
        public List<ProjectWorkLogDto> RecentWorkLogs { get; set; } = new();
    }
}

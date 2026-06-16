using System.ComponentModel.DataAnnotations;

namespace HSCrm.Models.ModelDto
{
    public class RoleModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public int TenantId { get; set; }
    }

    public class RoleCreateDto
    {
        public string Name { get; set; }

        public int TenantId { get; set; }
    }

    public class RoleEditDto
    {
        [Required]
        public required string Id { get; set; }

        [Required(ErrorMessage = "نام نقش الزامی است")]
        public required string Name { get; set; }

        public int TenantId { get; set; }
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Title { get; set; }
        public required string Category { get; set; }
    }

    public class PermissionCheckboxDto
    {
        public int PermissionId { get; set; }
        public required string Name { get; set; }
        public required string Title { get; set; }
        public required string Category { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ManageRolePermissionsDto
    {
        public required string RoleId { get; set; }
        public required string RoleName { get; set; }
        public List<int> SelectedPermissionIds { get; set; } = new();
        public List<PermissionCheckboxDto> Permissions { get; set; } = new();
    }

    public class UpdateRolePermissionsDto
    {
        public required string RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }
}

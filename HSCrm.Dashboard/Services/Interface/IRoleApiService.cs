using HSCrm.Models.ModelDto;

namespace HSCrm.Dashboard.Services.Interface
{
    public interface IRoleApiService
    {
        Task<List<RoleModel>> GetRoles();
        Task<RoleModel?> GetRoleById(string roleId);
        Task<bool> CreateRole(RoleCreateDto model);
        Task<bool> UpdateRole(RoleEditDto model);
        Task<bool> DeleteRole(string roleId);
        Task<List<PermissionDto>> GetPermissions();
        Task<List<int>> GetRolePermissions(string roleId);
        Task<bool> UpdateRolePermissions(UpdateRolePermissionsDto model);
    }
}
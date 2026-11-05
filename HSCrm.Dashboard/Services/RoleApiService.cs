using global::HSCrm.Dashboard.Services.Interface;
using global::HSCrm.Models.Common;
using global::HSCrm.Models.ModelDto;
using System.Net.Http.Json;

namespace HSCrm.Dashboard.Services
{
    public class RoleApiService : IRoleApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RoleApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            return _httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<List<RoleModel>> GetRoles()
        {
            var http = CreateClient();

            var response = await http.GetFromJsonAsync<ApiResponse<List<RoleModel>>>(
                "Role/GetRoles");

            return response?.Data ?? new List<RoleModel>();
        }

        public async Task<RoleModel?> GetRoleById(string roleId)
        {
            var http = CreateClient();

            var safeId = Uri.EscapeDataString(roleId);

            var response = await http.GetFromJsonAsync<ApiResponse<RoleModel>>(
                $"Role/GetRoleById?roleId={safeId}");

            return response?.Data;
        }

        public async Task<bool> CreateRole(RoleCreateDto model)
        {
            var http = CreateClient();

            var response = await http.PostAsJsonAsync(
                "Role/CreateRole",
                model);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRole(RoleEditDto model)
        {
            var http = CreateClient();

            var response = await http.PutAsJsonAsync(
                "Role/UpdateRole",
                model);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRole(string roleId)
        {
            var http = CreateClient();

            var safeId = Uri.EscapeDataString(roleId);

            var response = await http.DeleteAsync(
                $"Role/DeleteRole?roleId={safeId}");

            return response.IsSuccessStatusCode;
        }

        public async Task<List<PermissionDto>> GetPermissions()
        {
            var http = CreateClient();

            var response = await http.GetFromJsonAsync<ApiResponse<List<PermissionDto>>>(
                "Role/GetPermissions");

            return response?.Data ?? new List<PermissionDto>();
        }

        public async Task<List<int>> GetRolePermissions(string roleId)
        {
            var http = CreateClient();

            var safeId = Uri.EscapeDataString(roleId);

            var response = await http.GetFromJsonAsync<ApiResponse<List<int>>>(
                $"Role/GetRolePermissions?roleId={safeId}");

            return response?.Data ?? new List<int>();
        }

        public async Task<bool> UpdateRolePermissions(
            UpdateRolePermissionsDto model)
        {
            var http = CreateClient();

            var response = await http.PostAsJsonAsync(
                "Role/UpdateRolePermissions",
                model);

            return response.IsSuccessStatusCode;
        }
    }
}

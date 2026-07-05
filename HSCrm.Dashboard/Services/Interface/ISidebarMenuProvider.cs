using System.Security.Claims;
using HSCrm.Dashboard.Models;

namespace HSCrm.Dashboard.Services.Interface
{
    public interface ISidebarMenuProvider
    {
        List<SidebarMenuItem> GetMenus(ClaimsPrincipal user);
    }
}
using HSCrm.Dashboard.Models.ModelDto;

namespace HSCrm.Dashboard.Services.Interface
{
    public interface IRegisterService
    {
        Task<RegisterResult> Register(RegisterTenantModel model);
    }
}
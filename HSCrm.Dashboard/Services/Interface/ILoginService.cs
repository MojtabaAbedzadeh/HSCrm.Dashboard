using HSCrm.Models.ModelDto;

namespace HSCrm.Dashboard.Services.Interface
{
    public interface ILoginService
    {
        Task<UserLoginDto> Login(LoginModel model);
    }
}

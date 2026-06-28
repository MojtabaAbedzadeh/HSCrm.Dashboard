using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Models.ModelDto;
using HSCrm.Dashboard.Services.Interface;
using Newtonsoft.Json;

namespace HSCrm.Dashboard.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _api;

        public RegisterService(IConfiguration config)
        {
            _config = config;
            _api = new GetListApi();
        }

        public async Task<RegisterResult> Register(RegisterTenantModel model)
        {
            string apiUrl = _config["ApiAddress"] + "Account/RegisterTenant";

            try
            {
                var jsonResponse = await _api.PostApi(apiUrl, model);
                var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                if (result != null && result.token != null)
                {
                    return new RegisterResult
                    {
                        Success = true,
                        Token = result.token,
                        TenantId = result.tenantId.ToString(),
                        UserId = result.userId.ToString()
                    };
                }

                return new RegisterResult { Success = false, Message = "خطا در ثبت‌نام" };
            }
            catch (Exception ex)
            {
                return new RegisterResult { Success = false, Message = ex.Message };
            }
        }
    }
}
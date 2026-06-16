using HSCrm.Dashboard.Services.Interface;
using HSCrm.Models.ModelDto;
using System.Net.Http.Json;

namespace HSCrm.Dashboard.Services
{
    public class LoginService : ILoginService
    {
        private readonly IConfiguration _config;

        public LoginService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<UserLoginDto?> Login(LoginModel model)
        {
            try
            {
                string requestUrl = _config["ApiAddress"] + "Account/Login";

                using HttpClient http = new HttpClient();

                var response = await http.PostAsJsonAsync(requestUrl, model);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"خطا در ورود به سیستم: {error}");
                }

                var result = await response.Content.ReadFromJsonAsync<UserLoginDto>();

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("خطا در ارتباط با سرور", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("خطای غیرمنتظره در عملیات ورود", ex);
            }
        }
    }
}

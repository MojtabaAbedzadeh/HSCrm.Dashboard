using System.Net;
using HSCrm.Dashboard.Services.Interface;
using HSCrm.Models.ModelDto;
using Newtonsoft.Json.Linq;

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
                var baseUrl = (_config["ApiAddress"] ?? string.Empty).TrimEnd('/') + "/";
                string requestUrl = baseUrl + "Account/check-user";

                using HttpClient http = new HttpClient();

                var response = await http.PostAsJsonAsync(requestUrl, model);

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"خطا در ورود به سیستم: {error}");
                }

                var result = await response.Content.ReadFromJsonAsync<UserLoginDto>();
                if (result == null)
                    return null;

                // check-user توکن پیش‌ورود بدون FiscalYearId می‌دهد؛ پس از انتخاب سال مالی باید توکن نهایی از Account/login گرفته شود
                if (model.FiscalYearId > 0)
                {
                    var loginUrl = baseUrl + "Account/login";
                    using var loginResponse = await http.PostAsJsonAsync(loginUrl, model);
                    if (loginResponse.StatusCode == HttpStatusCode.Unauthorized || loginResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        return null;
                    }

                    if (!loginResponse.IsSuccessStatusCode)
                    {
                        var loginError = await loginResponse.Content.ReadAsStringAsync();
                        throw new Exception($"خطا در ورود با سال مالی انتخاب‌شده: {loginError}");
                    }

                    var loginJson = JObject.Parse(await loginResponse.Content.ReadAsStringAsync());
                    var fiscalYearToken = loginJson.Value<string>("token") ?? loginJson.Value<string>("Token");
                    if (string.IsNullOrWhiteSpace(fiscalYearToken))
                        throw new Exception("توکن معتبر برای سال مالی انتخاب‌شده از وب‌سرویس دریافت نشد.");

                    result.Token = fiscalYearToken;
                    result.FiscalYearStatus = loginJson.Value<int?>("fiscalYearStatus")
                        ?? loginJson.Value<int?>("FiscalYearStatus")
                        ?? result.FiscalYearStatus;
                }

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
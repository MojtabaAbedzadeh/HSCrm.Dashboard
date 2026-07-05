using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HSCrm.BussinessLogic.PublicMethod
{
    // تعریف خطاهای اختصاصی برای مدیریت بهتر در کنترلرها
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException() : base("دسترسی به این بخش مجاز نمی‌باشد.") { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("اعتبارنامه شما منقضی یا نامعتبر است.") { }
    }

    public class GetListApi
    {
        public async Task<string> GetApiList(string apiUrl, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // مدیریت کدهای خطا به جای منفجر کردن چرخه با EnsureSuccessStatusCode
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new AccessDeniedException();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException();
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> PostApi(string apiUrl, object model, string token = "")
        {
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new AccessDeniedException();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException();
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}

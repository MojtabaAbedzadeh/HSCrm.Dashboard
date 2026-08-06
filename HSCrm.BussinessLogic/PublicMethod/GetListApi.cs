using Newtonsoft.Json;
using System.Net;
using System.Text;

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
        private readonly IHttpClientFactory _httpClientFactory;

        public GetListApi(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetApiList(string apiUrl)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }
    
        public async Task<string> PostApi(string endpoint, object model)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(endpoint, content);

            if (response.StatusCode == HttpStatusCode.Forbidden) throw new AccessDeniedException();
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}

using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace HSCrm.BussinessLogic.PublicMethod
{
    public class GetListApi
    {
        public async Task<string> GetApiList(string apiUrl, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}

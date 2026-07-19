using System.Net.Http.Headers;

namespace HSCrm.Dashboard.Services
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                // ۱. اضافه کردن توکن احراز هویت
                var token = context.User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // ۲. اضافه کردن هدر سال مالی (X-FiscalYear-Id)
                // اول سعی می‌کند از Claim بخواند، اگر نبود از کوکی می‌گیرد
                var fiscalYearId = context.User.Claims.FirstOrDefault(c => c.Type == "FiscalYearId")?.Value
                                   ?? context.Request.Cookies["ActiveFiscalYearId"];

                if (!string.IsNullOrEmpty(fiscalYearId))
                {
                    request.Headers.Remove("X-FiscalYear-Id"); // جلوگیری از تکرار
                    request.Headers.TryAddWithoutValidation("X-FiscalYear-Id", fiscalYearId);
                }

                // ۳. اضافه کردن هدر TenantId
                var tenantId = context.User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
                if (!string.IsNullOrEmpty(tenantId))
                {
                    request.Headers.Remove("TenantId");
                    request.Headers.TryAddWithoutValidation("TenantId", tenantId);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Services.Interface;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Automation.Dashboard.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginService _login;
        private readonly IRegisterService _registerService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public AccountController(ILoginService login, IMemoryCache cache, IConfiguration config, IRegisterService registerService, GetListApi getListApi)
        {
            _login = login;
            _cache = cache;
            _config = config;
            _registerService = registerService;
            _getListApi = getListApi;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/AdminArea/Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterTenantModel model)
        {
            var result = await _registerService.Register(model);
            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(model);
            }

            // ساخت Claims
            var claims = new List<Claim>
            {
                new Claim("Token", result.Token),
                new Claim("TenantId", result.TenantId),
                new Claim("UserId", result.UserId),
                new Claim(ClaimTypes.NameIdentifier, result.UserId),
                new Claim(ClaimTypes.Name, model.UserName),
                new Claim(ClaimTypes.Email, model.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            return Redirect("/AdminArea/Home/Index");
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/AdminArea/Home");
            }

            ViewBag.FiscalsYearList = new List<FiscalYearDropDown>();
            string apiUrl = "FiscalYear/FiscalYearDropdownList";
            try
            {
                string jsonFullModel = await _getListApi.GetApiList(apiUrl);
                if (!string.IsNullOrWhiteSpace(jsonFullModel))
                {
                    var jsonDataParse = JsonConvert.DeserializeObject<dynamic>(jsonFullModel);
                    if (jsonDataParse != null && jsonDataParse.data != null)
                    {
                        ViewBag.FiscalsYearList = JsonConvert.DeserializeObject<List<FiscalYearDropDown>>(jsonDataParse.data.ToString()) 
                                                  ?? new List<FiscalYearDropDown>();
                    }
                }
            }
            catch
            {
                // جلوگیری از کرش کردن صفحه ورود در صورت عدم دسترسی اولیه به وب‌سرویس سال‌های مالی
                ViewBag.FiscalsYearList = new List<FiscalYearDropDown>();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (model.FiscalYearId <= 0)
            {
                var loginView = await Login();
                ViewBag.ErrorMessage = "لطفاً سال مالی را انتخاب کنید.";
                return loginView;
            }

            UserLoginDto? result;
            try
            {
                result = await _login.Login(model);
            }
            catch
            {
                var loginView = await Login();
                ViewBag.ErrorMessage = "خطا در برقراری ارتباط با وب‌سرویس ورود.";
                return loginView;
            }

            if (result == null || string.IsNullOrWhiteSpace(result.Token))
            {
                var loginView = await Login();
                ViewBag.ErrorMessage = "ورود انجام نشد. نام کاربری، رمز عبور و سال مالی انتخاب‌شده را بررسی کنید.";
                return loginView;
            }

            // ایجاد کل Claimهای مورد نیاز فرانت از روی اطلاعات واقعی کاربر
            var claims = new List<Claim>
            {
                new Claim("Token", result.Token ?? ""),
                new Claim("TenantId", result.TenantId.ToString()),
                new Claim("FiscalYearId", model.FiscalYearId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, result.UserId ?? ""),
                new Claim(ClaimTypes.Name, result.UserName ?? ""),
                new Claim("FiscalYearStatus", result.FiscalYearStatus.ToString()),
                
                // Claimهای جدید پروفایل
                new Claim("UserFirstName", result.UserFirstName ?? ""),
                new Claim("UserLastName", result.UserLastName ?? ""),
                new Claim(ClaimTypes.Email, result.Email ?? ""),
                new Claim(ClaimTypes.MobilePhone, result.PhoneNumber ?? ""),
                new Claim("UserImageUrl", result.UserImageUrl ?? "")
            };

            if (result.Roles != null)
                claims.AddRange(result.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            if (result.Permissions != null)
                claims.AddRange(result.Permissions.Select(p => new Claim("Permission", p)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            return Redirect("/AdminArea/Home/Index");
        }

        public async Task<IActionResult> Logout()
        {
            _cache.Remove("UserInfo");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> CheckUserAndGetFiscals(string userName, string password)
        {
            // ۱. بررسی نام کاربری و رمز عبور از طریق سرویس لاگین
            try
            {
                var result = await _login.Login(new LoginModel { UserName = userName, Password = password });
                if (result == null)
                    return Json(new { success = false, message = "نام کاربری یا رمز عبور اشتباه است." });

                return Json(new
                {
                    success = true,
                    fiscalYears = result.FiscalYears ?? new List<FiscalYearDropDownDto>(),
                    tenantId = result.TenantId,
                    userId = result.UserId
                });
            }
            catch (Exception ex) when (ex.Message.Contains("ارتباط با سرور"))
            {
                return Json(new { success = false, message = "خطا در برقراری ارتباط با وب‌سرویس." });
            }
            catch
            {
                return Json(new { success = false, message = "نام کاربری یا رمز عبور اشتباه است." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SwitchFiscalYear(int fiscalYearId, int fiscalYearStatus = 0, string? fiscalYearTitle = null)
        {
            if (HttpContext.User.Identity is not ClaimsIdentity currentIdentity || !currentIdentity.IsAuthenticated)
            {
                return Unauthorized(new { status = false, message = "کاربر احراز هویت نشده است." });
            }

            try
            {
                var apiResponse = await _getListApi.PostApi("Account/SwitchFiscalYear", fiscalYearId);
                var tokenResponse = JObject.Parse(apiResponse);
                string? apiToken = tokenResponse.Value<string>("token") ?? tokenResponse.Value<string>("Token");
                int? apiFiscalYearId = tokenResponse.Value<int?>("fiscalYearId") ?? tokenResponse.Value<int?>("FiscalYearId");
                string? apiFiscalYearTitle = tokenResponse.Value<string>("fiscalYearTitle") ?? tokenResponse.Value<string>("FiscalYearTitle") ?? fiscalYearTitle;
                int apiFiscalYearStatus = tokenResponse.Value<int?>("fiscalYearStatus")
                    ?? tokenResponse.Value<int?>("FiscalYearStatus")
                    ?? fiscalYearStatus;

                if (string.IsNullOrWhiteSpace(apiToken) || !apiFiscalYearId.HasValue)
                {
                    return StatusCode(502, new { status = false, message = "صدور توکن سال مالی جدید از وب‌سرویس ناموفق بود." });
                }

                var claims = currentIdentity.Claims
                    .Where(c => c.Type != "Token"
                        && c.Type != "FiscalYearId"
                        && c.Type != "FiscalYearStatus"
                        && c.Type != "FiscalYearTitle")
                    .ToList();

                claims.Add(new Claim("Token", apiToken));
                claims.Add(new Claim("FiscalYearId", apiFiscalYearId.Value.ToString()));
                claims.Add(new Claim("FiscalYearStatus", apiFiscalYearStatus.ToString()));
                if (!string.IsNullOrWhiteSpace(apiFiscalYearTitle))
                {
                    claims.Add(new Claim("FiscalYearTitle", apiFiscalYearTitle));
                }

                Response.Cookies.Append("ActiveFiscalYearId", apiFiscalYearId.Value.ToString(), new CookieOptions
                {
                    HttpOnly = false,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });

                var newIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var newPrincipal = new ClaimsPrincipal(newIdentity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    newPrincipal,
                    new AuthenticationProperties { IsPersistent = true });

                return Json(new
                {
                    status = true,
                    message = $"سال مالی فعال به «{apiFiscalYearTitle ?? apiFiscalYearId.Value.ToString()}» تغییر یافت.",
                    fiscalYearId = apiFiscalYearId.Value,
                    fiscalYearStatus = apiFiscalYearStatus,
                    fiscalYearTitle = apiFiscalYearTitle
                });
            }
            catch
            {
                return BadRequest(new { status = false, message = "تغییر سال مالی انجام نشد. لطفاً دوباره تلاش کنید." });
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

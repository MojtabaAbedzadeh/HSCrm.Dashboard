using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Services.Interface;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Automation.Dashboard.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginService _login;
        private readonly IRegisterService _registerService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public AccountController(ILoginService login, IMemoryCache cache, IConfiguration config, IRegisterService registerService)
        {
            _login = login;
            _cache = cache;
            _config = config;
            _registerService = registerService;
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

            string apiUrl = _config["ApiAddress"] + "FiscalYear/FiscalYearDropdownList";

            GetListApi getList = new GetListApi();

            string jsonFullModel = await getList.GetApiList(apiUrl, "");

            var jsonDataParse = JsonConvert.DeserializeObject<dynamic>(jsonFullModel);

            ViewBag.FiscalsYearList = JsonConvert.DeserializeObject<List<FiscalYearDropDown>>(jsonDataParse.data.ToString());

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckUserAndGetFiscals(string userName, string password)
        {
            // ۱. بررسی نام کاربری و رمز عبور از طریق سرویس لاگین
            var result = await _login.Login(new LoginModel { UserName = userName, Password = password });

            if (result == null)
                return Json(new { success = false, message = "نام کاربری یا رمز عبور اشتباه است." });

            // ۲. فراخوانی API برای گرفتن سال‌های مالی بر اساس TenantId کاربر
            string apiUrl = _config["ApiAddress"] + $"FiscalYear/FiscalYearDropdownList?tenantId={result.TenantId}";
            GetListApi getList = new GetListApi();
            string jsonFullModel = await getList.GetApiList(apiUrl, "");
            var jsonDataParse = JsonConvert.DeserializeObject<dynamic>(jsonFullModel);
            
            return Json(new
            {
                success = true,
                fiscalYears = result.FiscalYears,
                tenantId = result.TenantId,
                userId = result.UserId
            });
        }

        // متد Login نهایی (تغییر یافته)
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await _login.Login(model);

            if (result == null)
            {
                ViewBag.ErrorMessage = "خطا در برقراری ارتباط نهایی.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim("Token", result.Token ?? ""),
                new Claim("TenantId", result.TenantId.ToString()),
                new Claim("FiscalYearId", model.FiscalYearId.ToString()), // ذخیره سال مالی انتخاب شده
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.UserName ?? ""),
                new Claim("FiscalYearStatus", result.FiscalYearStatus.ToString())
            };

            if (result.Roles != null)
                claims.AddRange(result.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            if (result.Permissions != null)
                claims.AddRange(result.Permissions.Select(p => new Claim("Permission", p)));


            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });

            return Redirect("/AdminArea/Home/Index");
        }

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginModel model)
        //{
        //    var result = await _login.Login(model);

        //    if (result == null)
        //    {
        //        ViewBag.ErrorMessage = "نام کاربری یا رمز عبور صحیح نیست!";
        //        return View(model);
        //    }

        //    var claims = new List<Claim>
        //    {
        //        new Claim("Token", result.Token ?? ""),
        //        new Claim("TenantId", result.TenantId ?? ""),
        //        new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
        //        new Claim("UserId", result.UserId.ToString()),
        //        new Claim(ClaimTypes.Name, result.UserName ?? ""),
        //        new Claim(ClaimTypes.Email, result.Email ?? ""),
        //        new Claim("FiscalYearStatus", result.FiscalYearStatus.ToString())
        //    };

        //    claims.AddRange(result.Roles.Select(role => new Claim(ClaimTypes.Role, role)));


        //    // ✅ Roles
        //    if (result.Roles != null)
        //    {
        //        foreach (var role in result.Roles)
        //        {
        //            claims.Add(new Claim(ClaimTypes.Role, role));
        //        }
        //    }

        //    // ✅ Permissions (مهم برای سیستم تو)
        //    if (result.Permissions != null)
        //    {
        //        foreach (var permission in result.Permissions)
        //        {
        //            claims.Add(new Claim("Permission", permission));
        //        }
        //    }

        //    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        //    var principal = new ClaimsPrincipal(identity);

        //    var properties = new AuthenticationProperties
        //    {
        //        IsPersistent = true,
        //        ExpiresUtc = DateTime.UtcNow.AddHours(6),
        //        AllowRefresh = true
        //    };

        //    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        //    return Redirect("/AdminArea/Home/Index");
        //}

        public async Task<IActionResult> Logout()
        {
            _cache.Remove("UserInfo");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
